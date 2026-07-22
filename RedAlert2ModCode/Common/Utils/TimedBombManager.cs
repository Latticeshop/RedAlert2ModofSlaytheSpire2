#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 定时炸弹效果管理器
/// 用于管理部署到单位卡牌上的定时炸弹效果
/// </summary>
public static class TimedBombManager
{
    /// <summary>
    /// 存储需要触发定时炸弹效果的卡牌和对应的活力值
    /// 使用WeakReference避免内存泄漏
    /// </summary>
    private static readonly Dictionary<int, int> _timedBombCards = new();

    /// <summary>
    /// 为卡牌添加定时炸弹效果
    /// </summary>
    /// <param name="card">卡牌</param>
    /// <param name="vigorAmount">活力数量</param>
    public static void AddTimedBombEffect(CardModel card, int vigorAmount)
    {
        if (card == null) return;
        
        // 使用对象的唯一HashCode作为标识
        int cardKey = RuntimeHelpers.GetHashCode(card);
        if (!_timedBombCards.ContainsKey(cardKey))
        {
            _timedBombCards[cardKey] = vigorAmount;
        }
    }

    /// <summary>
    /// 检查并触发定时炸弹效果
    /// </summary>
    /// <param name="card">打出的卡牌</param>
    /// <returns>是否触发了效果</returns>
    public static async Task<bool> TryTriggerTimedBombEffect(CardModel card)
    {
        if (card == null) return false;

        int cardKey = RuntimeHelpers.GetHashCode(card);
        if (_timedBombCards.TryGetValue(cardKey, out int vigorAmount))
        {
            // 移除记录（只触发一次）
            _timedBombCards.Remove(cardKey);

            // 获得活力buff
            if (card.Owner != null && card.Owner.Creature != null)
            {
                await PowerCmd.Apply<VigorPower>(
                    new ThrowingPlayerChoiceContext(),
                    card.Owner.Creature,
                    (decimal)vigorAmount,
                    card.Owner.Creature,
                    card
                );
                GD.Print($"[TimedBombManager] 卡牌打出前获得 {vigorAmount} 点活力");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查卡牌是否有定时炸弹效果
    /// </summary>
    public static bool HasTimedBombEffect(CardModel card)
    {
        if (card == null) return false;
        int cardKey = RuntimeHelpers.GetHashCode(card);
        return _timedBombCards.ContainsKey(cardKey);
    }

    /// <summary>
    /// 获取卡牌的定时炸弹活力值
    /// </summary>
    public static int GetTimedBombVigor(CardModel card)
    {
        if (card == null) return 0;
        int cardKey = RuntimeHelpers.GetHashCode(card);
        return _timedBombCards.TryGetValue(cardKey, out int value) ? value : 0;
    }

    /// <summary>
    /// 清除所有定时炸弹效果（战斗结束时调用）
    /// </summary>
    public static void ClearAll()
    {
        _timedBombCards.Clear();
    }
}