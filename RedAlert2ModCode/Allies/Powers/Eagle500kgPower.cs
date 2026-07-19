using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 飞鹰500kg能力 - 绝地战备
/// 效果：对目标锁定的敌人造成50点伤害并溅射
/// 使用夸张的轰击+燃烧动画效果
/// </summary>
public class Eagle500kgPower : PowerModel, IDesperateMeasurePower
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.Eagle500kgPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 当前伤害值
    /// </summary>
    public int CurrentDamage { get; set; } = (int)Values.Damage;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

    /// <summary>
    /// 使用飞鹰500kg图标（放在powers目录下）
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png";

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
    /// 应用飞鹰500kg能力（支持堆叠）
    /// 注意：升级不再影响能力伤害，伤害始终保持基础值（50点）
    /// 使用 PowerStackType.Counter 实现自动堆叠
    /// </summary>
    public static async Task<Eagle500kgPower?> ApplyEagle500kg(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[Eagle500kgPower] ApplyEagle500kg 被调用 - IsUpgraded={isUpgraded}");

        // 使用 PowerCmd.Apply 应用能力，游戏会自动处理堆叠（因为 StackType = Counter）
        var power = await PowerCmd.Apply<Eagle500kgPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            // 升级不再影响伤害，伤害始终保持基础值
            power.CurrentDamage = (int)Values.Damage;
            power.IsUpgraded = isUpgraded;  // 保留升级状态标记，但不再影响伤害
            GD.Print($"[Eagle500kgPower] 应用成功 - Damage={power.CurrentDamage}, IsUpgraded={power.IsUpgraded}, Amount={power.Amount}");
        }
        return power;
    }

    /// <summary>
    /// 播放轰击+燃烧特效
    /// </summary>
    private void PlayBombardmentAndFireEffect(Creature target)
    {
        try
        {
            // 1. 播放重击特效（vfx_heavy_blunt）
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");
            
            // 2. 播放血腥冲击特效增强视觉效果
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bloody_impact");
            
            // 3. 播放火焰燃烧特效（NFireBurningVfx）
            var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
            }
            
            GD.Print("[Eagle500kgPower] 轰击+燃烧特效播放完成");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Eagle500kgPower] 播放特效失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 播放溅射特效
    /// </summary>
    private void PlaySplashEffect(Creature target)
    {
        try
        {
            // 播放溅射目标上的火焰特效
            var fireVfx = NFireBurningVfx.Create(target, 1f, goingRight: true);
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
            }
            
            // 播放小型爆炸特效
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_coin_explosion_small");
            
            GD.Print($"[Eagle500kgPower] 溅射特效播放完成 - 目标: {target.Name}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Eagle500kgPower] 播放溅射特效失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行绝地战备攻击效果
    /// 替换入侵者战机的普通攻击
    /// </summary>
    public async Task<bool> ExecuteDesperateMeasureAttack(Creature target, PlayerChoiceContext ctx)
    {
        GD.Print($"[Eagle500kgPower] ExecuteDesperateMeasureAttack 被调用 - Target={target?.Name}, Damage={CurrentDamage}");

        try
        {
            // 检查目标是否有效
            if (target == null)
            {
                GD.Print("[Eagle500kgPower] 目标为空");
                return false;
            }

            // 检查目标是否仍然存活
            if (!target.IsAlive)
            {
                GD.Print($"[Eagle500kgPower] 目标 {target.Name} 已死亡，跳过攻击");
                return false;
            }

            // 检查 Owner 是否有效
            if (base.Owner == null)
            {
                GD.Print("[Eagle500kgPower] Owner 为空");
                return false;
            }

            GD.Print($"[Eagle500kgPower] 开始执行绝地战备攻击 - 目标: {target.Name}, 伤害: {CurrentDamage}");

            // 播放轰击+燃烧特效
            PlayBombardmentAndFireEffect(target);

            // 等待特效播放
            await Cmd.Wait(0.3f);

            // 造成50点伤害
            await CreatureCmd.Damage(ctx ?? new ThrowingPlayerChoiceContext(),
                target,
                (decimal)CurrentDamage,
                ValueProp.Move,
                null,
                null);

            GD.Print($"[Eagle500kgPower] 对 {target.Name} 造成 {CurrentDamage} 点伤害");

            var otherEnemies = SplashDamageHelper.GetSplashTargets(target, CombatState?.HittableEnemies ?? new List<Creature>());

            if (otherEnemies.Count > 0)
            {
                decimal splashDamage = SplashDamageHelper.CalculateSplashDamage((decimal)CurrentDamage);
                GD.Print($"[Eagle500kgPower] 溅射伤害 = {splashDamage}");

                foreach (Creature otherEnemy in otherEnemies)
                {
                    PlaySplashEffect(otherEnemy);
                    await Cmd.Wait(0.15f);
                    
                    await CreatureCmd.Damage(ctx ?? new ThrowingPlayerChoiceContext(),
                        otherEnemy,
                        splashDamage,
                        ValueProp.Move,
                        null,
                        null);
                    GD.Print($"[Eagle500kgPower] 对 {otherEnemy.Name} 造成 {splashDamage} 点溅射伤害");
                }
            }

            // 减少一层能力（支持囤积战备）
            int currentAmount = (int)base.Amount;
            if (currentAmount > 1)
            {
                // 如果还有多层，减少一层
                await PowerCmd.ModifyAmount(ctx ?? new ThrowingPlayerChoiceContext(), this, -1m, base.Owner, null);
                GD.Print($"[Eagle500kgPower] 绝地战备已消耗一层，剩余层数: {currentAmount - 1}");
            }
            else
            {
                // 如果只有一层，移除能力
                await PowerCmd.Remove(this);
                GD.Print("[Eagle500kgPower] 绝地战备已消耗完毕，移除能力");
            }

            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Eagle500kgPower] 执行绝地战备攻击失败: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 查找或创建目标锁定敌人
    /// </summary>
    public static async Task<Creature?> FindOrCreateTargetLockedEnemy(ICombatState combatState, Creature owner)
    {
        // 查找有目标锁定能力的敌人
        var targetLockedEnemies = combatState.Enemies
            .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive && 
                           enemy.Powers.Any(p => p is TargetLockedPower))
            .ToList();

        GD.Print($"[Eagle500kgPower] 发现 {targetLockedEnemies.Count} 个目标锁定敌人");

        // 如果有目标锁定的敌人，返回第一个
        if (targetLockedEnemies.Count > 0)
        {
            return targetLockedEnemies.First();
        }

        // 如果没有目标锁定的敌人，随机选择一个敌人赋予目标锁定
        GD.Print("[Eagle500kgPower] 没有目标锁定的敌人，随机选择一个敌人");
        var aliveEnemies = combatState.Enemies
            .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
            .ToList();

        if (aliveEnemies.Count > 0)
        {
            var rng = owner?.Player?.RunState?.Rng?.CombatCardSelection;
            var randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
            var randomEnemy = aliveEnemies[randomIndex];
            GD.Print($"[Eagle500kgPower] 随机选择敌人: {randomEnemy.Name}");
            
            // 赋予目标锁定
            await PowerCmd.Apply<TargetLockedPower>(new ThrowingPlayerChoiceContext(), randomEnemy, 1m, owner, null);
            GD.Print($"[Eagle500kgPower] 已为 {randomEnemy.Name} 赋予目标锁定");
            
            return randomEnemy;
        }

        GD.Print("[Eagle500kgPower] 没有存活的敌人");
        return null;
    }
}