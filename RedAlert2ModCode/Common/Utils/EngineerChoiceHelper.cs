using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.UI;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Common.Utils;

public static class EngineerChoiceHelper
{
    private const int BASE_CHOICE_COUNT = 2;
    private const int UPGRADED_CHOICE_COUNT = 1;

    public static List<EngineerChoice> GenerateRandomChoices(bool isUpgraded, Player player)
    {
        int choiceCount = isUpgraded ? BASE_CHOICE_COUNT + UPGRADED_CHOICE_COUNT : BASE_CHOICE_COUNT;
        return WeightedRandomSelection(EngineerChoiceValues.AllChoices, choiceCount, player.RunState.Rng.CombatCardSelection);
    }

    private static List<EngineerChoice> WeightedRandomSelection(
        List<EngineerChoice> choices, int count, Rng rng)
    {
        List<EngineerChoice> result = new();
        List<EngineerChoice> remaining = new List<EngineerChoice>(choices);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            int totalWeight = remaining.Sum(c => c.Weight);
            int randomValue = rng.NextInt(totalWeight);
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

    public static async Task ExecuteChoice(PlayerChoiceContext ctx, EngineerChoice choice, CardModel card)
    {
        switch (choice.Type)
        {
            case EngineerChoiceScreen.ChoiceType.CaptureOilDerrick:
                var oilDerrickCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<OilDerrickCard>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(oilDerrickCard, PileType.Hand, card.Owner);
                break;

            case EngineerChoiceScreen.ChoiceType.RepairBuilding:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PlatingPower>(ctx, card.Owner.Creature, 3, card.Owner.Creature, card);
                break;

            case EngineerChoiceScreen.ChoiceType.CaptureAirfield:
                var paratrooperCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<Paratrooper>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(paratrooperCard, PileType.Hand, card.Owner);
                break;

            case EngineerChoiceScreen.ChoiceType.CaptureHospital:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.DexterityPower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                break;

            case EngineerChoiceScreen.ChoiceType.CaptureWorkshop:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                break;

            case EngineerChoiceScreen.ChoiceType.CaptureTechOutpost:
                await PowerCmd.Apply<PatriotMissilePower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                var repairDepotCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesRepairDepot>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(repairDepotCard, PileType.Hand, card.Owner);
                break;

            case EngineerChoiceScreen.ChoiceType.RepairBridge:
                var handPile = PileType.Hand.GetPile(card.Owner);
                var handCards = handPile.Cards.ToList();
                
                if (handCards.Any())
                {
                    var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(handCards, 1, 1, card.Owner);
                    
                    if (selectedCards != null && selectedCards.Any())
                    {
                        foreach (var selectedCard in selectedCards)
                        {
                            await CardPileCmd.Add(selectedCard, PileType.Exhaust);
                        }
                        await CardPileCmd.Draw(ctx, 2, card.Owner);
                    }
                }
                break;
        }
    }
}