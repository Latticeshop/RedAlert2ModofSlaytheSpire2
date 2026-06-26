using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 黄蜂舰载机能力 - 航空母舰发射的舰载机
/// 效果：每回合对目标锁定的敌人造成伤害
/// </summary>
public class HornetPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.HornetPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同伤害值的叠加逻辑在 ApplyHornets 中手动处理
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

    public HornetPower()
    {
        GD.Print($"[HornetPower] 构造函数被调用 - Damage={CurrentDamage}");
    }

    /// <summary>
    /// 使用黄蜂舰载机图标
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/hornet.png";

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
    /// 应用黄蜂舰载机能力
    /// </summary>
    public static async Task<HornetPower?> ApplyHornet(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[HornetPower] ApplyHornet 被调用 - IsUpgraded={isUpgraded}");

        var newPower = await PowerCmd.Apply<HornetPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (newPower != null)
        {
            newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
            newPower.IsUpgraded = isUpgraded;
            GD.Print($"[HornetPower] 创建成功 - Damage={newPower.CurrentDamage}, IsUpgraded={newPower.IsUpgraded}");
        }
        return newPower;
    }

    /// <summary>
    /// 应用多个黄蜂舰载机能力
    /// 根据升级状态区分：相同升级状态则叠加层数，不同则创建新能力
    /// </summary>
    public static async Task ApplyHornets(Creature owner, int count, bool isUpgraded = false)
    {
        GD.Print($"[HornetPower] ApplyHornets 被调用 - Count={count}, IsUpgraded={isUpgraded}");

        // 查找现有的黄蜂舰载机能力（按升级状态区分）
        var existingHornetPower = owner.Powers
            .OfType<HornetPower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingHornetPower != null)
        {
            // 如果存在相同升级状态的能力，增加层数
            GD.Print($"[HornetPower] 发现相同升级状态的能力，增加层数 - 当前层数={existingHornetPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingHornetPower, (decimal)count, owner, null);
            GD.Print($"[HornetPower] 层数增加完成 - 新层数={existingHornetPower.Amount}");
        }
        else
        {
            // 如果不存在相同升级状态的能力，创建一个新能力并设置初始层数为count
            GD.Print($"[HornetPower] 未发现相同升级状态的能力，创建新能力 - 初始层数={count}");
            var newPower = await PowerCmd.Apply<HornetPower>(new ThrowingPlayerChoiceContext(), owner, (decimal)count, owner, null);
            if (newPower != null)
            {
                newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
                newPower.IsUpgraded = isUpgraded;
                GD.Print($"[HornetPower] 创建成功 - Damage={newPower.CurrentDamage}, IsUpgraded={newPower.IsUpgraded}, Amount={newPower.Amount}");
            }
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        // 获取当前层数
        int stacks = (int)base.Amount;
        GD.Print($"[HornetPower] 回合开始触发 - 层数={stacks}, Damage={CurrentDamage}");

        // 按层数循环攻击
        for (int i = 0; i < stacks; i++)
        {
            // 每次攻击前检查目标锁定
            // 查找有目标锁定能力的敌人
            var targetLockedEnemies = combatState.Enemies
                .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive && 
                               enemy.Powers.Any(p => p is TargetLockedPower))
                .ToList();

            GD.Print($"[HornetPower] 第{i+1}次攻击 - 发现 {targetLockedEnemies.Count} 个目标锁定敌人");

            // 如果没有目标锁定的敌人，随机选择一个敌人赋予目标锁定
            if (targetLockedEnemies.Count == 0)
            {
                GD.Print("[HornetPower] 没有目标锁定的敌人，随机选择一个敌人");
                var aliveEnemies = combatState.Enemies
                    .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
                    .ToList();

                if (aliveEnemies.Count > 0)
                {
                    // 随机选择一个敌人
                    var randomEnemy = aliveEnemies[GD.RandRange(0, aliveEnemies.Count - 1)];
                    GD.Print($"[HornetPower] 随机选择敌人: {randomEnemy.Name}");
                    
                    // 清除其他敌人可能存在的目标锁定（保持唯一性）
                    foreach (var enemy in aliveEnemies)
                    {
                        var targetLockedPower = enemy.Powers.FirstOrDefault(p => p is TargetLockedPower) as TargetLockedPower;
                        if (targetLockedPower != null)
                        {
                            await PowerCmd.Remove(targetLockedPower);
                        }
                    }
                    
                    // 赋予目标锁定
                    await PowerCmd.Apply<TargetLockedPower>(new ThrowingPlayerChoiceContext(), randomEnemy, 1m, Owner, null);
                    targetLockedEnemies.Add(randomEnemy);
                    GD.Print($"[HornetPower] 已为 {randomEnemy.Name} 赋予目标锁定");
                }
                else
                {
                    GD.Print("[HornetPower] 没有存活的敌人，跳过攻击");
                    return;
                }
            }

            // 选择第一个目标锁定的敌人
            var target = targetLockedEnemies.First();
            GD.Print($"[HornetPower] 第{i+1}次攻击目标: {target.Name}");

            // 检查目标是否仍然存活
            if (!target.IsAlive)
            {
                GD.Print($"[HornetPower] 目标 {target.Name} 已死亡，跳过本次攻击");
                continue;
            }

            // 尝试执行绝地战备攻击（消耗一层）
            // 使用 base.Owner 确保获取正确的玩家对象
            bool desperateSuccess = await DesperateMeasures.TryExecuteDesperateMeasureAttack(base.Owner, target, new ThrowingPlayerChoiceContext());
            if (desperateSuccess)
            {
                GD.Print($"[HornetPower] 第{i+1}次攻击 - 绝地战备攻击成功，跳过普通攻击");
                continue;  // 绝地战备已执行，跳过普通攻击
            }

            // 播放下砸动画
            await PlaySmashAnimation(target);

            // 造成伤害
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                new List<Creature> { target },
                (decimal)CurrentDamage,
                ValueProp.Unpowered,
                base.Owner,
                null);

            GD.Print($"[HornetPower] 第{i+1}次攻击 - 造成 {CurrentDamage} 点伤害");
        }
    }

    /// <summary>
    /// 播放下砸攻击动画
    /// </summary>
    private async Task PlaySmashAnimation(Creature target)
    {
        try
        {
            // 使用游戏内置的下砸动画
            await CreatureCmd.TriggerAnim(target, "Hit", 0.2f);
            GD.Print($"[HornetPower] 播放下砸动画");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[HornetPower] 播放动画失败: {ex.Message}");
        }
    }
}
