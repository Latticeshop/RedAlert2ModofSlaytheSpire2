#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 轨道380MM能力 - 轨道战备能力
/// 回合开始时对目标锁定的敌人造成伤害并溅射，可多次触发
/// 严格目标锁定模式：只攻击有目标锁定的敌人，若无则不触发（无随机保底）
/// 独立叠层：相同伤害值叠加层数，不同伤害值独立存在
/// </summary>
public sealed class Orbital380mmPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.Orbital380mmPower;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDamage { get; set; } = (int)Values.Damage;
    public int CurrentRepeat { get; set; } = (int)Values.Repeat;
    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Orbital/Orbital380mm.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int displayDamage = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : CurrentDamage;
            locString.Add("Damage", displayDamage);
            locString.Add("Repeat", CurrentRepeat);
            return locString;
        }
    }

    public static async Task<Orbital380mmPower?> ApplyOrbital380mm(Creature owner, bool isUpgraded = false)
    {
        int damage = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
        int repeat = (int)Values.Repeat;

        var existingPower = owner.Powers.OfType<Orbital380mmPower>()
            .FirstOrDefault(p => p.CurrentDamage == damage && p.CurrentRepeat == repeat);

        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[Orbital380mmPower] 叠加到已存在的380MM能力，层数: {existingPower.Amount}，伤害: {damage}x{repeat}");
            return existingPower;
        }

        var power = await PowerCmd.Apply<Orbital380mmPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentDamage = damage;
            power.CurrentRepeat = repeat;
            power.IsUpgraded = isUpgraded;
            GD.Print($"[Orbital380mmPower] 创建成功 - Damage={damage}x{repeat}, IsUpgraded={isUpgraded}");
        }
        return power;
    }

    private void PlayMainAttackEffect(Creature target)
    {
        try
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bloody_impact");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Orbital380mmPower] 播放特效失败: {ex.Message}");
        }
    }

    private void PlaySplashEffect(Creature target)
    {
        try
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_coin_explosion_small");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Orbital380mmPower] 播放溅射特效失败: {ex.Message}");
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        // 严格模式：只攻击目标锁定的敌人，若无则不触发
        Creature? target = ResolveTarget(combatState);

        if (target == null)
        {
            GD.Print("[Orbital380mmPower] 无目标锁定敌人，不触发");
            return;
        }

        int stacks = (int)Amount;
        GD.Print($"[Orbital380mmPower] 回合开始触发 - 目标={target.Name}, 层数={stacks}, Damage={CurrentDamage}x{CurrentRepeat}");

        for (int i = 0; i < stacks; i++)
        {
            for (int j = 0; j < CurrentRepeat; j++)
            {
                PlayMainAttackEffect(target);
                await Cmd.Wait(0.3f);

                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                    target,
                    (decimal)CurrentDamage,
                    ValueProp.Move,
                    null,
                    null);

                var otherEnemies = SplashDamageHelper.GetSplashTargets(target, combatState.HittableEnemies);
                if (otherEnemies.Count > 0)
                {
                    decimal splashDamage = SplashDamageHelper.CalculateSplashDamage((decimal)CurrentDamage);
                    foreach (var otherEnemy in otherEnemies)
                    {
                        PlaySplashEffect(otherEnemy);
                        await Cmd.Wait(0.15f);

                        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                            otherEnemy,
                            splashDamage,
                            ValueProp.Move,
                            null,
                            null);
                    }
                }

                if (j < CurrentRepeat - 1)
                    await Cmd.Wait(0.15f);
            }

            GD.Print($"[Orbital380mmPower] 第{i + 1}次触发 - 对{target.Name}造成{CurrentDamage}点伤害x{CurrentRepeat}次（含溅射）");

            if (i < stacks - 1)
                await Cmd.Wait(0.3f);
        }

        await PowerCmd.Remove(this);
    }

    /// <summary>
    /// 解析目标 - 严格模式：只查找目标锁定敌人，无随机保底
    /// </summary>
    private Creature? ResolveTarget(ICombatState combatState)
    {
        var targetLocked = combatState.Enemies
            .FirstOrDefault(e => e.Side == CombatSide.Enemy && e.IsAlive &&
                                e.Powers.Any(p => p is TargetLockedPower));

        if (targetLocked != null)
        {
            GD.Print($"[Orbital380mmPower] 使用目标锁定敌人: {targetLocked.Name}");
            return targetLocked;
        }

        return null;
    }
}
