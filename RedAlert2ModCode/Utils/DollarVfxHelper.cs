#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 资金动画辅助类
/// 用于播放资金增加/扣除时的视觉反馈动画
/// </summary>
public static class DollarVfxHelper
{
    /// <summary>
    /// 播放资金增加动画
    /// 效果：刀乐能力图标闪烁 + 增益动画（绿色粒子）
    /// </summary>
    public static void PlayGainVfx(Creature owner, int amount)
    {
        if (TestMode.IsOn || amount <= 0) return;

        try
        {
            // 闪烁刀乐能力图标
            var dollarPower = owner.Powers.FirstOrDefault(p => p is DollarPower) as DollarPower;
            if (dollarPower != null)
            {
                dollarPower.FlashPower();
                GD.Print($"[DollarVfxHelper] 资金增加 {amount}，闪烁刀乐能力");
            }

            // 播放增益动画（绿色粒子）
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(owner);
            if (creatureNode != null)
            {
                var buffVfx = NPowerAppliedBuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                if (buffVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(buffVfx);
                    GD.Print("[DollarVfxHelper] 播放增益动画（绿色粒子）");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarVfxHelper] 播放资金增加动画失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 播放资金扣除动画
    /// 效果：刀乐能力图标闪烁 + 减益动画（虚弱效果）
    /// </summary>
    public static void PlaySpendVfx(Creature owner, int amount)
    {
        if (TestMode.IsOn || amount <= 0) return;

        try
        {
            // 闪烁刀乐能力图标
            var dollarPower = owner.Powers.FirstOrDefault(p => p is DollarPower) as DollarPower;
            if (dollarPower != null)
            {
                dollarPower.FlashPower();
                GD.Print($"[DollarVfxHelper] 资金扣除 {amount}，闪烁刀乐能力");
            }

            // 播放减益动画（虚弱效果）
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(owner);
            if (creatureNode != null)
            {
                var debuffVfx = NPowerAppliedDebuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                if (debuffVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(debuffVfx);
                    GD.Print("[DollarVfxHelper] 播放减益动画（虚弱效果）");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarVfxHelper] 播放资金扣除动画失败: {ex.Message}");
        }
    }
}