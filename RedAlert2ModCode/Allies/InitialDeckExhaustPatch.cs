using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

[HarmonyPatch]
public static class InitialDeckExhaustPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
    public static void LaunchPostfix(RunState __result)
    {
        foreach (var player in __result.Players)
        {
            if (player == null)
                continue;

            if (player.Character is not Allies)
                continue;
        }
    }
}