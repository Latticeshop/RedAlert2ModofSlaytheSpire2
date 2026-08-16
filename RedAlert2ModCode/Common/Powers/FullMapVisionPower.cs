using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 全图视野能力 - 本回合免疫[gold]虚弱[/gold]与[gold]脆弱[/gold]
/// 不可叠层（Single），玩家回合开始时移除
/// </summary>
public sealed class FullMapVisionPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 使用全图视野图片（注意：PowerModel.Icon 非 virtual，需配合 PowerIconPatch）
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/vision.png";

    public override LocString Title => new LocString("powers", "FULL_MAP_VISION_POWER.title");

    public override LocString Description => new LocString("powers", "FULL_MAP_VISION_POWER.description");

    /// <summary>
    /// 阻止自身获得原版虚弱（Weak）与脆弱（Frail）。
    /// </summary>
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? _, out decimal modifiedAmount)
    {
        if (target != base.Owner)
        {
            modifiedAmount = amount;
            return false;
        }

        if (canonicalPower is not WeakPower && canonicalPower is not FrailPower)
        {
            modifiedAmount = amount;
            return false;
        }

        modifiedAmount = default;
        return true;
    }

    /// <summary>
    /// 获得能力时清除自身已有的虚弱/脆弱。
    /// </summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);

        if (Owner == null) return;

        var existing = Owner.Powers
            .Where(p => p is WeakPower || p is FrailPower)
            .ToList();
        foreach (var power in existing)
        {
            await PowerCmd.Remove(power);
            GD.Print($"[FullMapVisionPower] 清除已有 {power.GetType().Name}");
        }
    }

    /// <summary>
    /// 玩家回合开始时：移除自身（能力只持续到下一次玩家回合开始）。
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        GD.Print("[FullMapVisionPower] 玩家回合开始，移除全图视野能力");
        await PowerCmd.Remove(this);
    }
}
