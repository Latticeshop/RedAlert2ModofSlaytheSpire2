using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietEngineer : CardModel
{
	private const int COST = 1;
	private const int BASE_CHOICE_COUNT = 2;
	private const int UPGRADED_CHOICE_COUNT = 1;

	public SovietEngineer() : base(COST, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/engnicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("ChoiceCount", BASE_CHOICE_COUNT)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		List<EngineerChoiceScreen.EngineerChoice> choices = GenerateRandomChoices();

		var selectedChoice = await EngineerChoiceScreen.ShowSelectionWithSync(choices, PortraitPath, Owner, FactionType.Soviet);

		if (selectedChoice != null)
		{
			await ExecuteChoice(ctx, selectedChoice);
		}
	}

	private List<EngineerChoiceScreen.EngineerChoice> GenerateRandomChoices()
	{
		int choiceCount = IsUpgraded ? BASE_CHOICE_COUNT + UPGRADED_CHOICE_COUNT : BASE_CHOICE_COUNT;
		var selected = WeightedRandomSelection(RedAlert2ModCode.Common.Cards.EngineerChoiceValues.AllChoices, choiceCount);

		return selected;
	}

	private List<EngineerChoice> WeightedRandomSelection(
		List<EngineerChoice> choices, int count)
	{
		List<EngineerChoice> result = new();
		List<EngineerChoice> remaining = new List<EngineerChoice>(choices);
		
		Random random = new();

		for (int i = 0; i < count && remaining.Count > 0; i++)
		{
			int totalWeight = remaining.Sum(c => c.Weight);
			int randomValue = random.Next(totalWeight);
			int currentWeight = 0;

			foreach (var choice in remaining)
			{
				currentWeight += choice.Weight;
				if (randomValue < currentWeight)
				{
					result.Add(choice);
					remaining.Remove(choice);
					break;
				}
			}
		}

		return result;
	}

	private async Task ExecuteChoice(PlayerChoiceContext ctx, EngineerChoice choice)
	{
		switch (choice.Type)
		{
			case EngineerChoiceScreen.ChoiceType.CaptureOilDerrick:
				var oilDerrickCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietOilDerrickCard>(), Owner);
				await CardPileCmd.AddGeneratedCardToCombat(oilDerrickCard, PileType.Hand, Owner);
				break;

			case EngineerChoiceScreen.ChoiceType.RepairBuilding:
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PlatingPower>(ctx, Owner.Creature, 3, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureAirfield:
				var paratrooperCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<Paratrooper>(), Owner);
				await CardPileCmd.AddGeneratedCardToCombat(paratrooperCard, PileType.Hand, Owner);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureHospital:
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.DexterityPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureWorkshop:
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureTechOutpost:
				await PowerCmd.Apply<RedAlert2ModCode.Allies.Powers.PatriotMissilePower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				await RedAlert2ModCode.Soviet.Powers.SovietRepairDepotPower.ApplyRepairDepot(Owner.Creature);
				break;

			case EngineerChoiceScreen.ChoiceType.RepairBridge:
				var handPile = PileType.Hand.GetPile(Owner);
				var handCards = handPile.Cards.ToList();
				
				if (handCards.Any())
				{
					var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(handCards, 1, 1, Owner);
					
					if (selectedCards != null && selectedCards.Any())
					{
						foreach (var card in selectedCards)
						{
							await CardPileCmd.Add(card, PileType.Exhaust);
						}
						await CardPileCmd.Draw(ctx, 2, Owner);
					}
				}
				break;
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["ChoiceCount"].UpgradeValueBy(UPGRADED_CHOICE_COUNT);
	}
}