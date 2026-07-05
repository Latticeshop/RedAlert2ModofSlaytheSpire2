using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace RedAlert2ModCode.Common.Powers;

public static class TargetLockedManager
{
    public static async Task ApplyTargetLocked(Creature target, Creature? source, object? sourceCard = null)
    {
        var combatState = source?.CombatState;
        if (combatState == null)
        {
            GD.PrintErr("[TargetLockedManager] 无法获取战斗状态");
            return;
        }

        var existingPower = target.Powers.OfType<TargetLockedPower>().FirstOrDefault();
        
        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, source, null);
            GD.Print($"[TargetLockedManager] 目标已有目标锁定，增加层数 - 当前层数: {existingPower.Amount}");
            return;
        }

        var allEnemies = combatState.Enemies
            .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
            .ToList();

        foreach (var enemy in allEnemies)
        {
            var targetPower = enemy.Powers.OfType<TargetLockedPower>().FirstOrDefault();
            if (targetPower != null)
            {
                await PowerCmd.Remove(targetPower);
                GD.Print($"[TargetLockedManager] 清除敌人 {enemy.Name} 的目标锁定");
            }
        }

        await PowerCmd.Apply<TargetLockedPower>(new ThrowingPlayerChoiceContext(), target, 1m, source, null);
        GD.Print($"[TargetLockedManager] 为目标 {target.Name} 赋予目标锁定");
    }
}
