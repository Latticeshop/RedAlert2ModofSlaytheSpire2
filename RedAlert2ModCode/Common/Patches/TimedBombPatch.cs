#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Patches;

/// <summary>
/// 定时炸弹效果补丁
/// 1. 在卡牌打出前触发定时炸弹效果（获得活力）
/// 2. 为有定时炸弹效果的卡牌添加描述前缀
/// 3. 为有定时炸弹效果的卡牌添加悬浮提示
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class TimedBombPatch
{
    [HarmonyPostfix]
    public static async void Postfix(ICombatState combatState, CardPlay cardPlay)
    {
        try
        {
            await TimedBombManager.TryTriggerTimedBombEffect(cardPlay.Card);
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[TimedBombPatch] 触发定时炸弹效果失败: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), new Type[] { typeof(PileType), typeof(Creature) })]
public static class TimedBombDescriptionPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        try
        {
            if (TimedBombManager.HasTimedBombEffect(__instance))
            {
                __result = ModCardKeywords.TimedBomb.GetCardText() + "\n" + __result;
            }
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[TimedBombDescriptionPatch] 添加定时炸弹描述失败: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
public static class TimedBombHoverTipsPatch
{
    [HarmonyPostfix]
    public static IEnumerable<IHoverTip> Postfix(IEnumerable<IHoverTip> __result, CardModel __instance)
    {
        try
        {
            if (TimedBombManager.HasTimedBombEffect(__instance))
            {
                var list = __result.ToList();
                list.Add(ModCardKeywords.TimedBomb.CreateHoverTip());
                return list;
            }
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[TimedBombHoverTipsPatch] 添加定时炸弹悬浮提示失败: {ex.Message}");
        }
        return __result;
    }
}
