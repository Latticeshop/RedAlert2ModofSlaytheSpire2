using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobasPatch
{
    [HarmonyPostfix]
    static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is RedAlert2ModCode.Common.Relics.DollarRelic)
        {
            __result = ModelDb.Relic<RedAlert2ModCode.Common.Relics.DollarAncientRelic>();
        }
    }
}