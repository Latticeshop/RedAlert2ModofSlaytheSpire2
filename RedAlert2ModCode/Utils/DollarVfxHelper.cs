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
/// 资金动画类型枚举
/// </summary>
public enum DollarVfxType
{
    /// <summary>无动画 - 用于资金返还但不展示动画</summary>
    None,
    /// <summary>加钱动画 - 绿色粒子 + 图标闪烁</summary>
    Gain,
    /// <summary>扣钱动画 - 虚弱效果 + 图标闪烁</summary>
    Spend
}

/// <summary>
/// 资金动画辅助类
/// 用于播放资金增加/扣除时的视觉反馈动画
/// </summary>
public static class DollarVfxHelper
{
    /// <summary>
    /// 播放资金动画（带类型控制）
    /// </summary>
    /// <param name="owner">拥有刀乐能力的生物</param>
    /// <param name="amount">资金数量</param>
    /// <param name="vfxType">动画类型</param>
    public static void PlayVfx(Creature owner, int amount, DollarVfxType vfxType)
    {
        if (TestMode.IsOn || amount <= 0) return;

        try
        {
            var dollarPower = owner.Powers.FirstOrDefault(p => p is DollarPower) as DollarPower;
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(owner);

            switch (vfxType)
            {
                case DollarVfxType.None:
                    // 不展示任何动画
                    GD.Print($"[DollarVfxHelper] 资金变化 {amount}，无动画");
                    break;

                case DollarVfxType.Gain:
                    // 播放加钱动画
                    if (dollarPower != null)
                    {
                        dollarPower.FlashPower();
                        GD.Print($"[DollarVfxHelper] 资金增加 {amount}，闪烁刀乐能力");
                    }
                    if (creatureNode != null)
                    {
                        var buffVfx = NPowerAppliedBuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                        if (buffVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(buffVfx);
                            GD.Print("[DollarVfxHelper] 播放增益动画（绿色粒子）");
                        }
                    }
                    break;

                case DollarVfxType.Spend:
                    // 播放扣钱动画
                    if (dollarPower != null)
                    {
                        dollarPower.FlashPower();
                        GD.Print($"[DollarVfxHelper] 资金扣除 {amount}，闪烁刀乐能力");
                    }
                    if (creatureNode != null)
                    {
                        var debuffVfx = NPowerAppliedDebuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                        if (debuffVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(debuffVfx);
                            GD.Print("[DollarVfxHelper] 播放减益动画（虚弱效果）");
                        }
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarVfxHelper] 播放资金动画失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 播放资金增加动画
    /// 效果：刀乐能力图标闪烁 + 增益动画（绿色粒子）
    /// </summary>
    public static void PlayGainVfx(Creature owner, int amount)
    {
        PlayVfx(owner, amount, DollarVfxType.Gain);
    }

    /// <summary>
    /// 播放资金扣除动画
    /// 效果：刀乐能力图标闪烁 + 减益动画（虚弱效果）
    /// </summary>
    public static void PlaySpendVfx(Creature owner, int amount)
    {
        PlayVfx(owner, amount, DollarVfxType.Spend);
    }
}