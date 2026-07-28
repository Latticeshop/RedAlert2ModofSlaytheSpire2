using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class BattleBunkerCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.BattleBunker;

	private List<CardModel> _storedCards = new();

	public BattleBunkerCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/bnkricon.png";

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("StoreCount", (int)Values.MagicNumber),
		new StringVar("StoredCards", string.Empty),
		new IntVar("DollarNumber", (int)Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.DefenseTower.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Garrison.CreateHoverTip()
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
		}

		BuildingSoundHelper.PlayBuildingPlaceSound();
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		int maxSelect = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
		await StoreSoldierCards(choiceContext, maxSelect);

		if (_storedCards.Count > 0)
		{
			await BattleBunkerPower.ApplyBattleBunker(Owner.Creature, IsUpgraded, _storedCards);
		}
	}

	private async Task StoreSoldierCards(PlayerChoiceContext choiceContext, int maxSelect)
	{
		var soldierCards = GetSoldierCardsFromHand();
		if (soldierCards.Count == 0)
			return;

		var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_BATTLE_BUNKER.select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", maxSelect);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, maxSelect)
		{
			RequireManualConfirmation = true
		};

		var selectedCards = (await CardSelectCmd.FromHand(
			choiceContext,
			Owner,
			prefs,
			c => soldierCards.Contains(c),
			this
		)).ToList();

		if (selectedCards == null || selectedCards.Count == 0)
			return;

		_storedCards.Clear();
		foreach (var card in selectedCards)
		{
			_storedCards.Add(card);
		}

		foreach (var card in _storedCards)
		{
			await CardPileCmd.RemoveFromCombat(card);
		}

		UpdateStoredCardsVar();
	}

	private List<CardModel> GetSoldierCardsFromHand()
	{
		var handPile = PileType.Hand.GetPile(Owner);
		var handCards = handPile.Cards.ToList();
		return handCards
			.Where(card => card is not null && IsSoldierCard(card))
			.ToList();
	}

	private bool IsSoldierCard(CardModel card)
	{
		var cardType = card.GetType();
		
		// 检查盟军所有士兵（基础、雷达解锁、高科技、遗物解锁）
		bool isAlliedSoldier = AlliedCardRegistry.Soldiers.Any(f => f().GetType() == cardType) ||
							   AlliedCardRegistry.RadarSoldiers.Any(f => f().GetType() == cardType) ||
							   AlliedCardRegistry.HighTechSoldiers.Any(f => f().GetType() == cardType) ||
							   AlliedCardRegistry.RelicUnlockedSoldiers.Any(f => f().GetType() == cardType);
		
		// 检查苏军所有士兵（基础、雷达解锁、遗物解锁）
		bool isSovietSoldier = SovietCardRegistry.Soldiers.Any(f => f().GetType() == cardType) ||
							   SovietCardRegistry.RadarSoldiers.Any(f => f().GetType() == cardType) ||
							   SovietCardRegistry.RelicUnlockedSoldiers.Any(f => f().GetType() == cardType);
		
		// 检查尤里所有士兵
		bool isYuriSoldier = YuriCardRegistry.Soldiers.Any(f => f().GetType() == cardType);
		
		// 检查尤里特殊卡（尤里改等）
		bool isYuriSpecial = SovietCardRegistry.SpecialCards.Any(f => 
		{
			var c = f();
			return c is YuriCard or YuriPrimeCard && c.GetType() == cardType;
		});
		
		return isAlliedSoldier || isSovietSoldier || isYuriSoldier || isYuriSpecial;
	}

	private void UpdateStoredCardsVar()
	{
		var storedCardsVar = DynamicVars["StoredCards"] as StringVar;
		if (storedCardsVar != null)
		{
			if (_storedCards.Count == 0)
			{
				storedCardsVar.StringValue = string.Empty;
			}
			else
			{
				var storedText = new LocString("cards", $"{Id.Entry}.stored_info");
				storedText.Add("0", string.Join(", ", _storedCards.Select(c => c.Title)));
				storedCardsVar.StringValue = storedText.GetFormattedText();
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["StoreCount"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
	}
}