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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class BattleBunkerCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.BattleBunker;
	private static readonly MethodInfo? CardOnPlayMethod = typeof(CardModel).GetMethod("OnPlay", BindingFlags.NonPublic | BindingFlags.Instance);

	private readonly List<CardModel> _storedCards = new();

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
		ModCardKeywords.DefenseTower.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Garrison.CreateHoverTip()
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards.Clear();
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

		var selectedCards = await CardSelectionScreen.ShowMultiSelection(
			soldierCards,
			maxSelect,
			0,
			Owner,
			FactionType.Soviet
		);

		if (selectedCards == null || selectedCards.Count == 0)
			return;

		_storedCards.Clear();
		foreach (var card in selectedCards)
		{
			card.HasBeenRemovedFromState = true;
			_storedCards.Add(card);
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
		var cardType = card.GetType().Name.ToUpper();
		return AlliedCardRegistry.Soldiers.Any(f => f().GetType().Name.ToUpper() == cardType) ||
			   SovietCardRegistry.Soldiers.Any(f => f().GetType().Name.ToUpper() == cardType);
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
				storedCardsVar.StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["StoreCount"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
	}
}