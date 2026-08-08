using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.UI;

using Choice = RedAlert2ModCode.UI.ChoiceSelectionScreen.Choice;

namespace RedAlert2ModCode.Common.Utils;

public static class EngineerChoiceHelper
{
    private const int BASE_CHOICE_COUNT = 2;
    private const int UPGRADED_CHOICE_COUNT = 1;

    public static List<Choice> GenerateRandomChoices(bool isUpgraded, Player player)
    {
        int choiceCount = isUpgraded ? BASE_CHOICE_COUNT + UPGRADED_CHOICE_COUNT : BASE_CHOICE_COUNT;
        return WeightedRandomSelection(EngineerChoiceValues.AllChoices, choiceCount, player.RunState.Rng.CombatCardSelection);
    }

    private static List<Choice> WeightedRandomSelection(
        List<Choice> choices, int count, Rng rng)
    {
        List<Choice> result = new();
        List<Choice> remaining = new List<Choice>(choices);

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

    public static async Task ExecuteChoice(PlayerChoiceContext ctx, Choice choice, CardModel card)
    {
        switch (choice.Type)
        {
            case ChoiceSelectionScreen.ChoiceType.CaptureOilDerrick:
                var oilDerrickCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<OilDerrickCard>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(oilDerrickCard, PileType.Hand, card.Owner);
                break;

            case ChoiceSelectionScreen.ChoiceType.RepairBuilding:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PlatingPower>(ctx, card.Owner.Creature, 3, card.Owner.Creature, card);
                break;

            case ChoiceSelectionScreen.ChoiceType.CaptureAirfield:
                var paratrooperCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<Paratrooper>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(paratrooperCard, PileType.Hand, card.Owner);
                break;

            case ChoiceSelectionScreen.ChoiceType.CaptureHospital:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.DexterityPower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                break;

            case ChoiceSelectionScreen.ChoiceType.CaptureWorkshop:
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                break;

            case ChoiceSelectionScreen.ChoiceType.CaptureTechOutpost:
                await PowerCmd.Apply<PatriotMissilePower>(ctx, card.Owner.Creature, 1, card.Owner.Creature, card);
                var repairDepotCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesRepairDepot>(), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(repairDepotCard, PileType.Hand, card.Owner);
                break;

            case ChoiceSelectionScreen.ChoiceType.RepairBridge:
                // 手牌为空时无法选择消耗牌，跳过（避免卡死）
                if (!PileType.Hand.GetPile(card.Owner).Cards.Any())
                {
                    GD.Print("[EngineerChoiceHelper] 手牌为空，跳过维修桥梁选择");
                    break;
                }

                // 使用原版手牌选择UI，让玩家选择1张牌来消耗（参考苏联维修厂实现）
                var repairSelectPrompt = new LocString("card_keywords", "engineer_choice.repair_bridge.select_prompt");
                repairSelectPrompt.Add("0", 1);
                repairSelectPrompt.Add("1", 1);
                var repairPrefs = new CardSelectorPrefs(repairSelectPrompt, 1, 1)
                {
                    RequireManualConfirmation = true
                };

                var repairSelectedCards = (await CardSelectCmd.FromHand(
                    ctx,
                    card.Owner,
                    repairPrefs,
                    c => true,
                    card
                )).ToList();

                foreach (var repairCard in repairSelectedCards)
                {
                    await CardPileCmd.Add(repairCard, PileType.Exhaust);
                    GD.Print($"[EngineerChoiceHelper] 维修桥梁：消耗手牌 {repairCard.Id.Entry}");
                }

                if (repairSelectedCards.Any())
                {
                    await CardPileCmd.Draw(ctx, 2, card.Owner);
                    GD.Print("[EngineerChoiceHelper] 维修桥梁：抽2张牌");
                }
                break;

            case ChoiceSelectionScreen.ChoiceType.SurveyMineField:
                // 随机获得一张矿卡牌（宝石矿、黄金矿、黄金矿柱）
                var mineCardTypes = new List<Func<CardModel>>
                {
                    () => ModelDb.Card<GoldMineCard>(),
                    () => ModelDb.Card<GemMineCard>(),
                    () => ModelDb.Card<GoldMineColumnCard>()
                };
                int mineIndex = card.Owner.RunState.Rng.CombatCardSelection.NextInt(mineCardTypes.Count);
                var mineCard = card.Owner.Creature.CombatState.CreateCard(mineCardTypes[mineIndex](), card.Owner);
                await CardPileCmd.AddGeneratedCardToCombat(mineCard, PileType.Hand, card.Owner);
                GD.Print($"[EngineerChoiceHelper] 勘测矿区：获得矿卡牌 {mineCard.Id.Entry}");
                break;
        }
    }
}
