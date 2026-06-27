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
        var localPlayer = __result.Players.FirstOrDefault();
        if (localPlayer == null)
            return;

        if (localPlayer.Character is not Soviet)
            return;

        foreach (var card in localPlayer.Deck.Cards)
        {
            if (card is Conscript)
            {
                card.EnergyCost.SetCustomBaseCost(0);
                card.AddKeyword(CardKeyword.Exhaust);
            }
        }
    }
}