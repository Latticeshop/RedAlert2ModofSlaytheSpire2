// 小格子铺 | Latticeshop
#nullable enable

using System;
using System.Collections;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace RedAlert2ModCode.Common.Patches;

/// <summary>
/// 防御式补丁：多人并发选择（例如两名玩家几乎同时打出兵营并打开选择面板）时，
/// 原版 NCardPlayQueue 的暂停/恢复记账可能丢失卡牌节点的父级，导致恢复时
/// "Node needs a parent to be reparented" / "Child is not a child of this node" 报错，
/// 并让打出中的卡牌视觉错乱。
/// 这里在恢复逻辑执行前，把无父级的卡牌节点安全挂回目标容器，避免报错并保持卡牌正常归位。
/// </summary>
[HarmonyPatch]
public static class NCardPlayQueueChoiceResumePatch
{
    private static readonly FieldInfo? _playQueueField = AccessTools.Field(typeof(NCardPlayQueue), "_playQueue");
    private static readonly Type? _queueItemType = typeof(NCardPlayQueue).GetNestedType("QueueItem", BindingFlags.NonPublic);
    private static readonly FieldInfo? _queueItemCardField = _queueItemType != null ? AccessTools.Field(_queueItemType, "card") : null;
    private static readonly FieldInfo? _queueItemActionField = _queueItemType != null ? AccessTools.Field(_queueItemType, "action") : null;

    /// <summary>
    /// ReAddCardAfterPlayerChoice 前置：卡牌节点无父级时先安全挂回目标容器，
    /// 避免后续 Reparent 抛 "Node needs a parent to be reparented"。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NCardPlayQueue), nameof(NCardPlayQueue.ReAddCardAfterPlayerChoice))]
    public static bool ReAddCardAfterPlayerChoicePrefix(NCardPlayQueue __instance, ref NCard card, GameAction action)
    {
        if (card == null || !GodotObject.IsInstanceValid(card) || card.IsQueuedForDeletion())
        {
            // 节点已失效或已排队待删除，跳过原逻辑，避免重挂/双释放
            return false;
        }

        if (card.GetParent() == null)
        {
            if (action != null && action.State == GameActionState.Executing)
            {
                NCombatRoom.Instance?.Ui.PlayContainer.AddChildSafely(card);
            }
            else
            {
                __instance.AddChildSafely(card);
            }
            GD.Print($"[NCardPlayQueueChoiceResumePatch] 修复无父级的卡牌节点: {card.Model?.Id.Entry ?? "unknown"}");
        }

        return true;
    }

    /// <summary>
    /// BeforeRemoteCardPlayResumedAfterPlayerChoice 前置：在远端卡牌动作恢复执行前，
    /// 同样修复队列中无父级的卡牌节点。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NCardPlayQueue), "BeforeRemoteCardPlayResumedAfterPlayerChoice")]
    public static bool BeforeRemoteCardResumePrefix(NCardPlayQueue __instance, GameAction action)
    {
        if (_playQueueField == null || _queueItemCardField == null || _queueItemActionField == null)
            return true;

        try
        {
            if (_playQueueField.GetValue(__instance) is IEnumerable queue)
            {
                foreach (object? item in queue)
                {
                    if (item == null) continue;
                    if (!ReferenceEquals(_queueItemActionField.GetValue(item), action)) continue;

                    if (_queueItemCardField.GetValue(item) is NCard card
                        && GodotObject.IsInstanceValid(card)
                        && !card.IsQueuedForDeletion()
                        && card.GetParent() == null)
                    {
                        NCombatRoom.Instance?.Ui.PlayContainer.AddChildSafely(card);
                        GD.Print($"[NCardPlayQueueChoiceResumePatch] 修复远端卡牌节点父级: {card.Model?.Id.Entry ?? "unknown"}");
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NCardPlayQueueChoiceResumePatch] 修复远端卡牌父级失败: {ex.Message}");
        }

        return true;
    }
}
