// 小格子铺 | Latticeshop
using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Patches;

/// <summary>
/// A2 预选模式拦截：对指定建筑卡，点击手牌时不发起正常出牌（拖拽/目标选择），
/// 而是打开本地预选面板；确认后由 BuildingPrePlayHelper 入队打出+结算，取消则无事发生。
/// </summary>
[HarmonyPatch(typeof(NPlayerHand))]
public static class BuildingPrePlayInterceptPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("StartCardPlay")]
    public static bool StartCardPlayPrefix(NHandCardHolder holder)
    {
        try
        {
            if (holder?.CardNode?.Model is CardModel card && BuildingPrePlayHelper.IsA2Card(card))
            {
                BuildingPrePlayHelper.OpenPrePlayPanel(card);
                return false;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BuildingPrePlayIntercept] 拦截失败: {ex}");
        }
        return true;
    }
}
