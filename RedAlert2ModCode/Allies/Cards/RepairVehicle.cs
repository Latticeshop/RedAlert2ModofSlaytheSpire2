#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class RepairVehicle : CardModel
{
	private List<CardModel> _storedCards = new();
	private bool _hasStored;
	private bool _inheritedExhaust;

	public override bool CanBeGeneratedInCombat => false;

	public RepairVehicle() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_repair.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("ReplayCount", 1),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
		_hasStored = false;
		_inheritedExhaust = false;
	}

	public void SetStoredCards(CardModel ifvCard, CardModel soldierCard, bool inheritedExhaust = false)
	{
		_storedCards.Clear();
		_storedCards.Add(ifvCard);
		_storedCards.Add(soldierCard);
		_hasStored = true;
		_inheritedExhaust = inheritedExhaust;

		if (inheritedExhaust)
		{
			AddKeyword(CardKeyword.Exhaust);
		}

		var storedText = new LocString("cards", "RED_ALERT2_MOD_CARD_REPAIR_VEHICLE.stored_info");
		storedText.Add("0", soldierCard.Title);
		((StringVar)DynamicVars["StoredCards"]).StringValue = GetLocStringText(storedText);
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "attack",
				Title = new LocString("card_keywords", "ui.repair_vehicle.repair_title"),
				Description = new LocString("card_keywords", "ui.repair_vehicle.repair_desc"),
				IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = new LocString("card_keywords", "ui.repair_vehicle.deploy_title"),
				Description = new LocString("card_keywords", "ui.repair_vehicle.stored_deploy_desc"),
				IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
			}
		};

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.repair_vehicle.title"), options, FactionType.Allied);

		if (selectedIndex.HasValue)
		{
			if (options[selectedIndex.Value].Id == "attack")
			{
				await ExecuteRepair(ctx, play);
			}
			else
			{
				await ExecuteDeploy(ctx, play);
			}
		}
	}

	private async Task ExecuteRepair(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvrepa_repair.mp3");
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

		bool isUpgraded = IsUpgraded;

		var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_REPAIR_VEHICLE.repair_select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", 1);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, 1)
		{
			RequireManualConfirmation = true
		};

		var selectedCards = (await CardSelectCmd.FromHand(
			ctx,
			Owner,
			prefs,
			c => c != this && (isUpgraded || IsUnitCard(c)),
			this
		)).ToList();

		foreach (var card in selectedCards)
		{
			card.BaseReplayCount += DynamicVars["ReplayCount"].IntValue;
			GD.Print($"[RepairVehicle] 为卡牌 {card.Title} 赋予 Replay {DynamicVars["ReplayCount"].IntValue}");
		}

		if (_inheritedExhaust && _hasStored)
		{
			GD.Print("[RepairVehicle] IFV有消耗词条，维修时消耗所有存储");
			var ifvCard = _storedCards[0];
			var engineerCard = _storedCards[1];

			engineerCard.HasBeenRemovedFromState = false;
			await CardPileCmd.Add(engineerCard, PileType.Exhaust, CardPilePosition.Bottom, this);
			GD.Print($"[RepairVehicle] 工程师 {engineerCard.Title} 已消耗");

			ifvCard.HasBeenRemovedFromState = false;
			await CardPileCmd.Add(ifvCard, PileType.Exhaust, CardPilePosition.Bottom, this);
			GD.Print($"[RepairVehicle] IFV卡牌 {ifvCard.Title} 已消耗");

			_storedCards.Clear();
			_hasStored = false;
			((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;

			await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
		}
		else
		{
			await CardPileCmd.Add(this, PileType.Discard, CardPilePosition.Bottom, this);
		}
	}

	private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
	{
		if (!_hasStored || _storedCards.Count == 0)
		{
			await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
			return;
		}

		UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvtran-deploy.mp3");

		GD.Print($"[RepairVehicle] 释放存储: IFV={_storedCards[0].Title}, Soldier={_storedCards[1].Title}");

		await ReleaseStoredCards();

		await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
	}

	private async Task ReleaseStoredCards()
	{
		var ifvCard = _storedCards[0];
		var soldierCard = _storedCards[1];

		soldierCard.HasBeenRemovedFromState = false;
		await CardPileCmd.Add(soldierCard, PileType.Hand, CardPilePosition.Bottom, this);
		GD.Print($"[RepairVehicle] 士兵卡牌 {soldierCard.Title} 已返回手牌");

		ifvCard.HasBeenRemovedFromState = false;
		var ifvTargetPile = _inheritedExhaust ? PileType.Exhaust : PileType.Discard;
		await CardPileCmd.Add(ifvCard, ifvTargetPile, CardPilePosition.Bottom, this);
		GD.Print($"[RepairVehicle] IFV卡牌 {ifvCard.Title} 已送往{(ifvTargetPile == PileType.Exhaust ? "消耗堆" : "弃牌堆")}");

		_storedCards.Clear();
		_hasStored = false;
		((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
		GD.Print("[RepairVehicle] 释放完成");
	}

	private static readonly Lazy<HashSet<System.Type>> LazyUnitTypes = new(InitializeUnitTypes);

	private static HashSet<System.Type> UnitTypes => LazyUnitTypes.Value;

	private static HashSet<System.Type> InitializeUnitTypes()
	{
		var set = new HashSet<System.Type>();
		set.UnionWith(AlliedCardRegistry.GetBasicUnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT1UnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT2UnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT3UnitTypes());
		set.UnionWith(SovietCardRegistry.GetBasicUnitTypes());
		set.UnionWith(SovietCardRegistry.GetT3UnitTypes());
		var yuriUnits = YuriCardRegistry.GetAllUnits();
		foreach (var u in yuriUnits)
			set.Add(u.GetType());
		return set;
	}

	public static bool IsUnitCard(CardModel card)
	{
		return UnitTypes.Contains(card.GetType());
	}

	protected override void OnUpgrade()
	{
		// DynamicVars["ReplayCount"].UpgradeValueBy(1);
	}

	private static string GetLocStringText(object locStringObj)
	{
		if (locStringObj == null) return string.Empty;
		if (locStringObj is string str) return str;

		System.Type locStringType = locStringObj.GetType();
		System.Reflection.MethodInfo? formattedMethod = locStringType.GetMethod("GetFormattedText", new System.Type[0]);
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

		System.Reflection.MethodInfo? rawMethod = locStringType.GetMethod("GetRawText");
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
