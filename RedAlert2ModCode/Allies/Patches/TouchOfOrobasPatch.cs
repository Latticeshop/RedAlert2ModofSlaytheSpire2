using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// Harmony补丁：处理TOUCH_OF_OROBAS遗物触发先古刀乐获取
/// 当玩家获取Touch of Orobas遗物时，将刀乐遗物替换为先古版本
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobasPatch
{
    [HarmonyPostfix]
    static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        // 检查起始遗物是否为刀乐遗物
        if (starterRelic is RedAlert2ModCode.Allies.Relics.DollarRelic)
        {
            // 将刀乐遗物替换为先古刀乐遗物
            __result = ModelDb.Relic<RedAlert2ModCode.Allies.Relics.DollarAncientRelic>();
        }
    }
}
