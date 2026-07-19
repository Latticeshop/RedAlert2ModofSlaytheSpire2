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
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Yuri;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class BattleFortress : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.BattleFortress;
	private static readonly MethodInfo? CardOnPlayMethod = typeof(CardModel).GetMethod("OnPlay", BindingFlags.NonPublic | BindingFlags.Instance);

	private List<CardModel> _storedCards = new List<CardModel>();
	private bool _hasStored;

	public BattleFortress() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/bfrticon.png";

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
		new BlockVar(Values.Block, ValueProp.Unpowered),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 3),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Garrison.CreateHoverTip()
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
		_hasStored = false;
		((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");
		await ShowActionChoice(choiceContext, cardPlay);
	}

	private async Task StoreSoldierCards(PlayerChoiceContext choiceContext)
	{
		var soldierCards = GetSoldierCardsFromHand();
		
		if (soldierCards.Count == 0)
		{
			await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
			return;
		}

		var storeCount = ((IntVar)DynamicVars["StoreCount"]).IntValue;
		var selectPrompt = new LocString("cards", "BATTLE_FORTRESS.select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", storeCount);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, storeCount)
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

		foreach (var card in selectedCards)
		{
			_storedCards.Add(card);
		}

		foreach (var card in _storedCards)
		{
			await CardPileCmd.RemoveFromCombat(card);
		}

		if (_storedCards.Count > 0)
		{
			_hasStored = true;
			((StringVar)DynamicVars["StoredCards"]).StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
		}

		await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
	}

	private async Task ShowActionChoice(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "attack",
				Title = new LocString("cards", "BATTLE_FORTRESS.attack_option"),
				Description = new LocString("cards", "BATTLE_FORTRESS.attack_option")
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = new LocString("cards", "BATTLE_FORTRESS.deploy_option"),
				Description = new LocString("cards", "BATTLE_FORTRESS.deploy_option")
			}
		};

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("cards", "BATTLE_FORTRESS.action_prompt"), options, FactionType.Allied);

		if (selectedIndex.HasValue)
		{
			if (options[selectedIndex.Value].Id == "attack")
			{
				await ExecuteAttack(choiceContext, cardPlay);
			}
			else
			{
				await ExecuteDeploy(choiceContext);
			}
		}
	}

	private async Task ExecuteAttack(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

		foreach (var storedCard in _storedCards)
		{
			if (CardOnPlayMethod != null)
			{
				var task = (Task)CardOnPlayMethod.Invoke(storedCard, new object[] { choiceContext, cardPlay })!;
				await task;
			}
		}

		await CardPileCmd.Add(this, PileType.Discard, CardPilePosition.Bottom, this);
	}

	private async Task ExecuteDeploy(PlayerChoiceContext choiceContext)
	{
		UnitVoiceHelper.PlayUnitVoice("BattleFortressDeploy", "Allies");

		if (!_hasStored)
		{
			await StoreSoldierCards(choiceContext);
		}
		else
		{
			foreach (var card in _storedCards)
			{
				card.HasBeenRemovedFromState = false;
				await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
			}

			_storedCards.Clear();
			_hasStored = false;
			((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
		}
	}

	private List<CardModel> GetSoldierCardsFromHand()
	{
		var handPile = PileType.Hand.GetPile(Owner);
		var handCards = handPile.Cards.ToList();

		var soldierTypes = new HashSet<Type>();
		foreach (var soldierFunc in AlliedCardRegistry.Soldiers)
		{
			var card = soldierFunc();
			soldierTypes.Add(card.GetType());
		}
		foreach (var soldierFunc in SovietCardRegistry.Soldiers)
		{
			var card = soldierFunc();
			soldierTypes.Add(card.GetType());
		}
		foreach (var soldierFunc in YuriCardRegistry.Soldiers)
		{
			var card = soldierFunc();
			soldierTypes.Add(card.GetType());
		}

		return handCards.Where(c => c != this && soldierTypes.Contains(c.GetType())).ToList();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["StoreCount"].UpgradeValueBy(2);
	}
}