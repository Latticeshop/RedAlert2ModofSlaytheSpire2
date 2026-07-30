using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 伤害追踪能力 - 隐藏能力，标记本回合是否已造成伤害
/// 回合开始时自动移除
/// </summary>
public sealed class DamageDealtTrackerPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { };

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            GD.Print("[DamageDealtTrackerPower] 新回合开始，移除伤害追踪标记");
            _ = PowerCmd.Remove(this);
        }
        return Task.CompletedTask;
    }
}
