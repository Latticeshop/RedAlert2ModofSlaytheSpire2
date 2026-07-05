using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Common.Powers;

public class TargetLockedPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/target_locked.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            return locString;
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented) return;
        
        var combatState = Owner?.CombatState;
        if (combatState == null) return;

        var aliveEnemies = combatState.Enemies
            .Where(e => e != Owner && e.IsAlive && e.Side == CombatSide.Enemy)
            .ToList();

        if (aliveEnemies.Count == 0)
        {
            GD.Print("[TargetLockedPower] 没有存活的敌人，目标锁定结束");
            return;
        }

        var hasTargetLocked = aliveEnemies.Any(e => e.Powers.Any(p => p is TargetLockedPower));
        if (hasTargetLocked)
        {
            GD.Print("[TargetLockedPower] 场上还有其他目标锁定的敌人，不转移");
            return;
        }

        var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
        int randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
        var newTarget = aliveEnemies[randomIndex];

        int stacks = (int)Amount;
        await PowerCmd.Remove(this);

        var transferredPower = await PowerCmd.Apply<TargetLockedPower>(
            new ThrowingPlayerChoiceContext(),
            newTarget,
            stacks,
            null,
            null
        );

        GD.Print($"[TargetLockedPower] 目标锁定已转移到 {newTarget.Name}，层数: {stacks}");
    }
}