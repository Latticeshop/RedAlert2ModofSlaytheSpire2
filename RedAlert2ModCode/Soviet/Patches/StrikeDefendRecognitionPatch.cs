using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class StrikeDefendRecognitionPatch
{
    private static readonly HashSet<System.Type> StrikeCardTypes = new()
    {
        typeof(Conscript),
    };

    private static readonly HashSet<System.Type> DefendCardTypes = new()
    {
        typeof(RhinoTank),
    };

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsBasicStrikeOrDefend), MethodType.Getter)]
    public static bool IsBasicStrikeOrDefendPrefix(CardModel __instance, ref bool __result)
    {
        System.Type cardType = __instance.GetType();
        
        if (StrikeCardTypes.Contains(cardType) || DefendCardTypes.Contains(cardType))
        {
            __result = true;
            return false;
        }

        return true;
    }
}