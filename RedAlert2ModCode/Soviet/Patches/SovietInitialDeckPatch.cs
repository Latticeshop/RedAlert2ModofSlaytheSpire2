using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Relics;
using Godot;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class SovietInitialDeckPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
    public static void LaunchPostfix(RunState __result)
    {
        foreach (var player in __result.Players)
        {
            if (player == null)
                continue;

            if (player.Character is not Soviet)
                continue;

            var hasLibyaRelic = player.Relics.Any(r => r is LibyaRelic);
            if (!hasLibyaRelic)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await RelicCmd.Obtain<LibyaRelic>(player);
                    GD.Print("[SovietInitialDeckPatch] 为苏联角色添加利比亚遗物");
                });
            }
        }
    }
}