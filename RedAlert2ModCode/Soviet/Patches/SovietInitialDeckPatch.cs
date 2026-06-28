using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class SovietInitialDeckPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
    public static void LaunchPostfix(RunState __result)
    {
        // 遍历所有玩家（多人游戏中需要处理每个玩家）
        foreach (var player in __result.Players)
        {
            if (player == null)
                continue;

            if (player.Character is not Soviet)
                continue;

            foreach (var card in player.Deck.Cards)
            {
                if (card is Conscript)
                {
                    card.EnergyCost.SetCustomBaseCost(0);
                    card.AddKeyword(CardKeyword.Exhaust);
                }
                else if (card is RhinoTank)
                {
                    card.AddKeyword(CardKeyword.Exhaust);
                }
            }
        }
    }
}