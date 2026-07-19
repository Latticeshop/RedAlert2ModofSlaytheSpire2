using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class EagleMachineGunPower : PowerModel, IDesperateMeasurePower
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.EagleMachineGunPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDamage { get; set; } = (int)Values.Damage;

    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/EagleMachineGunPower.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Damage", CurrentDamage);
            locString.Add("Repeat", (int)Values.Repeat);
            return locString;
        }
    }

    public static async Task<EagleMachineGunPower?> ApplyEagleMachineGun(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[EagleMachineGunPower] ApplyEagleMachineGun 被调用 - IsUpgraded={isUpgraded}");
        GD.Print($"[EagleMachineGunPower] 当前 powers 列表中的能力数量: {owner.Powers.Count(p => p is EagleMachineGunPower)}");
        
        foreach (var p in owner.Powers.OfType<EagleMachineGunPower>())
        {
            GD.Print($"[EagleMachineGunPower] 现有能力 - IsUpgraded={p.IsUpgraded}, Amount={p.Amount}");
        }

        var existingPower = owner.Powers
            .OfType<EagleMachineGunPower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            GD.Print($"[EagleMachineGunPower] 发现相同升级状态的能力，增加层数 - 当前层数={existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[EagleMachineGunPower] 层数增加完成 - 新层数={existingPower.Amount}");
            return existingPower;
        }
        else
        {
            GD.Print($"[EagleMachineGunPower] 未发现相同升级状态的能力，创建新能力");
            var power = await PowerCmd.Apply<EagleMachineGunPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
            if (power != null)
            {
                power.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
                power.IsUpgraded = isUpgraded;
                GD.Print($"[EagleMachineGunPower] 创建成功 - Damage={power.CurrentDamage}, IsUpgraded={power.IsUpgraded}, Amount={power.Amount}");
            }
            return power;
        }
    }

    public async Task<bool> ExecuteDesperateMeasureAttack(Creature target, PlayerChoiceContext ctx)
    {
        GD.Print($"[EagleMachineGunPower] ExecuteDesperateMeasureAttack 被调用 - Target={target?.Name}, Damage={CurrentDamage}");

        try
        {
            if (target == null || !target.IsAlive)
            {
                GD.Print("[EagleMachineGunPower] 目标无效");
                return false;
            }

            if (base.Owner == null)
            {
                GD.Print("[EagleMachineGunPower] Owner 为空");
                return false;
            }

            bool hasTargetLock = target.Powers.Any(p => p is TargetLockedPower);
            if (!hasTargetLock)
            {
                var combatState = CombatState;
                if (combatState != null)
                {
                    var aliveEnemies = combatState.Enemies
                        .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
                        .ToList();

                    if (aliveEnemies.Count > 0)
                    {
                        var rng = base.Owner?.Player?.RunState?.Rng?.CombatCardSelection;
                        var randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
                        var randomEnemy = aliveEnemies[randomIndex];
                        await PowerCmd.Apply<TargetLockedPower>(new ThrowingPlayerChoiceContext(), randomEnemy, 1m, base.Owner, null);
                        GD.Print($"[EagleMachineGunPower] 随机赋予 {randomEnemy.Name} 目标锁定");
                        target = randomEnemy;
                    }
                }
            }

            GD.Print($"[EagleMachineGunPower] 开始执行绝地战备攻击 - 目标: {target.Name}, 伤害: {CurrentDamage}, 次数: {Values.Repeat}");

            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");

            int repeatCount = (int)Values.Repeat;
            for (int i = 0; i < repeatCount; i++)
            {
                await Cmd.Wait(0.1f);
                
                await CreatureCmd.Damage(ctx ?? new ThrowingPlayerChoiceContext(),
                    target,
                    (decimal)CurrentDamage,
                    ValueProp.Move,
                    null,
                    null);
                
                GD.Print($"[EagleMachineGunPower] 第 {i + 1} 次攻击 - 对 {target.Name} 造成 {CurrentDamage} 点伤害");
            }

            int currentAmount = (int)base.Amount;
            if (currentAmount > 1)
            {
                await PowerCmd.ModifyAmount(ctx ?? new ThrowingPlayerChoiceContext(), this, -1m, base.Owner, null);
                GD.Print($"[EagleMachineGunPower] 绝地战备已消耗一层，剩余层数: {currentAmount - 1}");
            }
            else
            {
                await PowerCmd.Remove(this);
                GD.Print("[EagleMachineGunPower] 绝地战备已消耗完毕，移除能力");
            }

            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[EagleMachineGunPower] 执行绝地战备攻击失败: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
}