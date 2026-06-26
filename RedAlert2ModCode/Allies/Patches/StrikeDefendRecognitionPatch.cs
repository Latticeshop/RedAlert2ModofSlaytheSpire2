using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// Harmony补丁：让盟军卡牌被识别为打击/防御卡牌
/// 
/// 使以下卡牌能够被游戏机制识别：
/// - 美国大兵 (AmericanSoldier) → 视为打击卡 (Strike)
/// - 盟军基地车 (AlliedMCV) → 视为打击卡 (Strike)
/// - 灰熊坦克 (GrizzlyTank) → 视为防御卡 (Defend)  
/// - 围墙 (AlliedWallCard) → 视为防御卡 (Defend)
/// 
/// 这允许StrikeDummy等遗物对这些卡牌生效。
/// </summary>
[HarmonyPatch]
public static class StrikeDefendRecognitionPatch
{
    #region 视为打击卡的卡牌类型
    private static readonly HashSet<System.Type> StrikeCardTypes = new()
    {
        typeof(AmericanSoldier),    // 美国大兵 → 打击
    };
    #endregion

    #region 视为防御卡的卡牌类型
    private static readonly HashSet<System.Type> DefendCardTypes = new()
    {
        typeof(GrizzlyTank),        // 灰熊坦克 → 防御
    };
    #endregion

    /// <summary>
    /// 补丁：让卡牌被识别为基础打击/防御卡
    /// 这允许 StrikeDummy 等遗物对这些卡牌生效
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsBasicStrikeOrDefend), MethodType.Getter)]
    public static bool IsBasicStrikeOrDefendPrefix(CardModel __instance, ref bool __result)
    {
        // 检查是否是我们要识别的卡牌类型
        System.Type cardType = __instance.GetType();
        
        if (StrikeCardTypes.Contains(cardType) || DefendCardTypes.Contains(cardType))
        {
            __result = true;
            return false; // 跳过原方法
        }

        return true; // 执行原方法
    }
}
