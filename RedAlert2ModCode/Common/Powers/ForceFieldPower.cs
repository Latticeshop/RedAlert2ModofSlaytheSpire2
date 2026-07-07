using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Common.Powers;

public sealed class ForceFieldPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new IntVar("EnergyLoss", (int)CommonPowerValues.ForceFieldPower.Damage) };

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner.CombatState == null || Owner.Player == null)
            return;

        int energyLoss = (int)CommonPowerValues.ForceFieldPower.Damage;
        int totalLoss = energyLoss * (int)Amount;

        GD.Print($"[ForceFieldPower] 回合开始，失去 {totalLoss} 点能量（{Amount}层 x {energyLoss}）");

        await PlayerCmd.LoseEnergy(totalLoss, Owner.Player);

        await PowerCmd.Remove(this);
    }
}