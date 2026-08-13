#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// IFV - 技能牌
/// 1费，抽2(升级3)张牌，弃0-2(升级3)张牌
/// 部署：选择手牌中1张士兵卡牌驻扎，获得5(升级7)格挡
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Ifv : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Ifv;

	private List<CardModel> _storedCards = new List<CardModel>();
	private bool _hasStored;

	public Ifv() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fvicon.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			var keywords = new List<CardKeyword>();
			if (_hasStored)
			{
				keywords.Add(CardKeyword.Exhaust);
			}
			return keywords;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("DrawCount", Values.MagicNumber),
		new IntVar("DiscardCount", Values.Stars),
            new BlockVar(Values.Block, ValueProp.Move),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Garrison.CreateHoverTip()
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "attack",
				Title = new LocString("card_keywords", "ui.ifv.normal_title"),
				Description = new LocString("card_keywords", _hasStored ? "ui.ifv.stored_attack_desc" : "ui.ifv.normal_desc"),
				IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = new LocString("card_keywords", "ui.ifv.deploy_title"),
				Description = new LocString("card_keywords", _hasStored ? "ui.ifv.stored_deploy_desc" : "ui.ifv.deploy_desc"),
				IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
			}
		};

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(ctx, Owner, new LocString("card_keywords", "ui.ifv.title"), options, FactionType.Allied);

		if (selectedIndex.HasValue)
		{
			if (options[selectedIndex.Value].Id == "attack")
			{
				if (_hasStored)
					await ExecuteAttack(ctx, play);
				else
					await ExecuteNormal(ctx, play);
			}
			else
			{
				if (_hasStored)
					await ExecuteDeployStored(ctx, play);
				else
					await ExecuteDeploy(ctx, play);
			}
		}
	}

	private async Task ExecuteNormal(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/missile.wav");
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");
		await CardPileCmd.Draw(ctx, (int)DynamicVars["DrawCount"].BaseValue, Owner);

		// 手牌中没有可弃的卡牌时，跳过弃牌选择界面（避免卡死）
		var handPile = PileType.Hand.GetPile(Owner);
		if (!handPile.Cards.Any(c => c != this))
		{
			GD.Print("[Ifv] 手牌中没有可弃卡牌，跳过弃牌选择");
			return;
		}

		int maxDiscard = (int)DynamicVars["DiscardCount"].BaseValue;
		// 原版 FromHand 会触发 CancelAllCardPlay（取消回手流程），联机中会阻塞其他玩家出牌；
		// 改用与超时空传送一致的 ExecuteSyncChoice + mod 选择 UI。
		var discardableCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).ToList();
		var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(
			ctx, discardableCards, maxDiscard, 0, Owner)
			?? new List<CardModel>();

		foreach (var card in selectedCards)
		{
			await CardPileCmd.Add(card, PileType.Discard);
			GD.Print($"[Ifv] 弃牌: {card.Title}");
		}
	}

	private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvtran-deploy.mp3");
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

		var soldierCards = GetSoldierCardsFromHand();

		if (soldierCards.Count == 0)
		{
			GD.Print("[Ifv] 手中没有士兵卡牌，正常打出");
			await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
			return;
		}

		// 原版 FromHand 会触发 CancelAllCardPlay（取消回手流程），联机中会阻塞其他玩家出牌；
		// 改用与超时空传送一致的 ExecuteSyncChoice + mod 选择 UI。
		var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(
			ctx, soldierCards, 1, 0, Owner)
			?? new List<CardModel>();

		if (selectedCards.Count == 0)
		{
			GD.Print("[Ifv] 取消选择，正常打出");
			await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
			return;
		}

		var selectedCard = selectedCards[0];

		// 定时炸弹检测（最优先）：关键词（任意被伊文部署的卡）或类型（炸弹单位本身）
		if (TimedBombManager.HasTimedBombEffect(selectedCard)
			|| selectedCard is TerrorMan or CrazyIvanCard or ChronoIvanCard)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<DemoVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is AlliesEngineer or SovietEngineer)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<RepairVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is AmericanSoldier or Conscript or SpyCard)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<MinigunVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is SovietFlakTrooper)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<AaVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is GuardianGi)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<HeavyVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is SovietTeslaTrooper)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<TeslaVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is Desolator)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<RadVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is Sniper)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<SniperVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is SealCommandos or ChronoCommandos or PsiCommandoCard)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<HmgVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is YuriCard)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<ShockwaveVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		if (selectedCard is ChronoLegionnaire)
		{
			await VehicleDeployHelper.DeploySpecialVehicle<ChronoVehicle>(ctx, this, selectedCard, Owner);
			return;
		}

		_storedCards.Add(selectedCard);
		GD.Print($"[Ifv] 存储士兵卡牌: {selectedCard.Title}");

		foreach (var card in _storedCards)
		{
			await CardPileCmd.RemoveFromCombat(card);
		}

		if (_storedCards.Count > 0)
		{
			_hasStored = true;
			var blockValue = IsUpgraded ? Values.Block + Values.BlockUpgraded : Values.Block;
			var storedText = new LocString("cards", "IFV.stored_block");
			storedText.Add("0", blockValue);
			storedText.Add("1", string.Join(", ", _storedCards.Select(c => c.Title)));
			((StringVar)DynamicVars["StoredCards"]).StringValue = GetLocStringText(storedText);
			GD.Print($"[Ifv] 存储完成，已存储 {_storedCards.Count} 张卡牌");
		}

		await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
	}

	private async Task ExecuteAttack(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/missile.wav");
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");
		await ExecuteNormal(ctx, play);
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
		GD.Print($"[Ifv] 获得 {DynamicVars.Block.IntValue} 点格挡");
		await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
	}

	private async Task ExecuteDeployStored(PlayerChoiceContext ctx, CardPlay play)
	{
		if (_storedCards.Count == 0)
		{
			await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
			return;
		}

		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvtran-deploy.mp3");

		GD.Print($"[Ifv] 释放存储的卡牌，数量: {_storedCards.Count}");

		foreach (var card in _storedCards)
		{
			card.HasBeenRemovedFromState = false;
			await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
			GD.Print($"[Ifv] 释放卡牌: {card.Title}");
		}

		_storedCards.Clear();
		_hasStored = false;
		((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
		GD.Print("[Ifv] 释放完成");

		await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
	}

	private PileType GetPlayTargetPile()
	{
		return Keywords.Contains(CardKeyword.Exhaust) ? PileType.Exhaust : PileType.Discard;
	}

	private List<CardModel> GetSoldierCardsFromHand()
	{
		var handPile = PileType.Hand.GetPile(Owner);
		var handCards = handPile.Cards.ToList();

		var soldierTypes = new HashSet<Type>();

		foreach (var soldierFunc in AlliedCardRegistry.Soldiers)
			soldierTypes.Add(soldierFunc().GetType());
		foreach (var soldierFunc in AlliedCardRegistry.RadarSoldiers)
			soldierTypes.Add(soldierFunc().GetType());
		foreach (var soldierFunc in AlliedCardRegistry.HighTechSoldiers)
			soldierTypes.Add(soldierFunc().GetType());
		foreach (var soldierFunc in AlliedCardRegistry.RelicUnlockedSoldiers)
			soldierTypes.Add(soldierFunc().GetType());

		foreach (var soldierFunc in SovietCardRegistry.Soldiers)
			soldierTypes.Add(soldierFunc().GetType());
		foreach (var soldierFunc in SovietCardRegistry.RadarSoldiers)
			soldierTypes.Add(soldierFunc().GetType());
		foreach (var soldierFunc in SovietCardRegistry.RelicUnlockedSoldiers)
			soldierTypes.Add(soldierFunc().GetType());

		foreach (var soldierFunc in YuriCardRegistry.Soldiers)
			soldierTypes.Add(soldierFunc().GetType());

		foreach (var specialFunc in SovietCardRegistry.SpecialCards)
		{
			var card = specialFunc();
			if (card is YuriCard or YuriPrimeCard)
				soldierTypes.Add(card.GetType());
		}

		return handCards.Where(c => c != this && soldierTypes.Contains(c.GetType())).ToList();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["DrawCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
		DynamicVars["DiscardCount"].UpgradeValueBy(Values.StarsUpgraded);
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}

	private static string GetLocStringText(object locStringObj)
	{
		if (locStringObj == null) return string.Empty;
		if (locStringObj is string str) return str;

		Type locStringType = locStringObj.GetType();
		MethodInfo? formattedMethod = locStringType.GetMethod("GetFormattedText", new Type[0]);
		if (formattedMethod != null)
		{
			try
			{
				object? result = formattedMethod.Invoke(locStringObj, null);
				if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
					return formattedText;
			}
			catch { }
		}

		MethodInfo? rawMethod = locStringType.GetMethod("GetRawText");
		if (rawMethod != null)
		{
			try
			{
				object? result = rawMethod.Invoke(locStringObj, null);
				if (result is string rawText)
					return rawText;
			}
			catch { }
		}

		return string.Empty;
	}
}
