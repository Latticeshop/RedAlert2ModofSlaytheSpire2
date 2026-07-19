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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 飞鹰空袭能力 - 绝地战备
/// 效果：对全部敌人造成8点伤害
/// </summary>
public class EagleAirStrikePower : PowerModel, IDesperateMeasurePower
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.EagleAirStrikePower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同升级状态的叠加逻辑在 ApplyEagleAirStrike 中手动处理
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>
    /// 当前伤害值
    /// </summary>
    public int CurrentDamage { get; set; } = (int)Values.Damage;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

    /// <summary>
    /// 使用飞鹰空袭图标
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/EagleAirStrikePower.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Damage", CurrentDamage);
            return locString;
        }
    }

    /// <summary>
    /// 应用飞鹰空袭能力（支持堆叠）
    /// 根据升级状态区分：相同升级状态则叠加层数，不同则创建新能力
    /// </summary>
    public static async Task<EagleAirStrikePower?> ApplyEagleAirStrike(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[EagleAirStrikePower] ApplyEagleAirStrike 被调用 - IsUpgraded={isUpgraded}");
        GD.Print($"[EagleAirStrikePower] 当前 powers 列表中的能力数量: {owner.Powers.Count(p => p is EagleAirStrikePower)}");
        
        // 遍历所有能力，检查状态
        foreach (var p in owner.Powers.OfType<EagleAirStrikePower>())
        {
            GD.Print($"[EagleAirStrikePower] 现有能力 - IsUpgraded={p.IsUpgraded}, Amount={p.Amount}");
        }

        // 查找现有的飞鹰空袭能力（按升级状态区分）
        var existingPower = owner.Powers
            .OfType<EagleAirStrikePower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            // 如果存在相同升级状态的能力，增加层数
            GD.Print($"[EagleAirStrikePower] 发现相同升级状态的能力，增加层数 - 当前层数={existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[EagleAirStrikePower] 层数增加完成 - 新层数={existingPower.Amount}");
            return existingPower;
        }
        else
        {
            // 如果不存在相同升级状态的能力，创建新能力
            GD.Print($"[EagleAirStrikePower] 未发现相同升级状态的能力，创建新能力");
            var power = await PowerCmd.Apply<EagleAirStrikePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
            if (power != null)
            {
                power.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
                power.IsUpgraded = isUpgraded;
                GD.Print($"[EagleAirStrikePower] 创建成功 - Damage={power.CurrentDamage}, IsUpgraded={power.IsUpgraded}, Amount={power.Amount}");
            }
            return power;
        }
    }

    /// <summary>
    /// 执行绝地战备攻击效果
    /// 替换入侵者战机的普通攻击
    /// </summary>
    public async Task<bool> ExecuteDesperateMeasureAttack(Creature target, PlayerChoiceContext ctx)
    {
        GD.Print($"[EagleAirStrikePower] ExecuteDesperateMeasureAttack 被调用 - Damage={CurrentDamage}");

        try
        {
            if (base.Owner == null)
            {
                GD.Print("[EagleAirStrikePower] Owner 为空");
                return false;
            }

            var combatState = CombatState;
            if (combatState == null)
            {
                GD.Print("[EagleAirStrikePower] CombatState 为空");
                return false;
            }

            // 获取所有存活的敌人
            var allEnemies = combatState.HittableEnemies
                .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
                .ToList();

            if (allEnemies.Count == 0)
            {
                GD.Print("[EagleAirStrikePower] 没有存活的敌人");
                return false;
            }

            GD.Print($"[EagleAirStrikePower] 开始执行绝地战备攻击 - 对 {allEnemies.Count} 个敌人造成 {CurrentDamage} 点伤害");

            // 对全部敌人造成伤害
            foreach (var enemy in allEnemies)
            {
                // 播放空袭特效
                VfxCmd.PlayOnCreatureCenter(enemy, "vfx/vfx_attack_blunt");
                
                await Cmd.Wait(0.1f);
                
                await CreatureCmd.Damage(ctx ?? new ThrowingPlayerChoiceContext(),
                    enemy,
                    (decimal)CurrentDamage,
                    ValueProp.Move,
                    null,
                    null);
                
                GD.Print($"[EagleAirStrikePower] 对 {enemy.Name} 造成 {CurrentDamage} 点伤害");
            }

            // 减少一层能力
            int currentAmount = (int)base.Amount;
            if (currentAmount > 1)
            {
                await PowerCmd.ModifyAmount(ctx ?? new ThrowingPlayerChoiceContext(), this, -1m, base.Owner, null);
                GD.Print($"[EagleAirStrikePower] 绝地战备已消耗一层，剩余层数: {currentAmount - 1}");
            }
            else
            {
                await PowerCmd.Remove(this);
                GD.Print("[EagleAirStrikePower] 绝地战备已消耗完毕，移除能力");
            }

            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[EagleAirStrikePower] 执行绝地战备攻击失败: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
}
