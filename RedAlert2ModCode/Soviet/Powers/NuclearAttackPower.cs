#nullable enable

using System;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 核弹攻击能力 - 打出核弹攻击卡时获得。
/// 回合结束时按层数循环触发：对全部敌人造成伤害并赋予中毒，触发后移除。
/// 不同数值（升级/未升级）各自独立实例叠层（Instanced，参考苏联哨戒炮）。
/// </summary>
public sealed class NuclearAttackPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.NuclearAttack;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// Instanced：不同数值各自独立实例与层数（升级/未升级分别叠层）。
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDamage { get; set; } = (int)Values.Damage;

    public int CurrentPoison { get; set; } = (int)Values.MagicNumber;

    public bool IsUpgraded { get; set; }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Damage", CurrentDamage);
            locString.Add("Poison", CurrentPoison);
            return locString;
        }
    }

    /// <summary>
    /// 为指定角色获得核弹攻击能力（按数值独立叠层：相同 Damage/Poison 的实例合并层数）。
    /// </summary>
    public static async Task ApplyNuclearAttack(Creature owner, bool isUpgraded = false)
    {
        int damage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
        int poison = (int)Values.MagicNumber;

        var existingPower = owner.Powers
            .OfType<NuclearAttackPower>()
            .FirstOrDefault(p => p.CurrentDamage == damage && p.CurrentPoison == poison);
        if (existingPower != null)
        {
            GD.Print($"[NuclearAttackPower] 相同数值实例叠层 - Damage={damage}, Poison={poison}, 当前层数={existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            return;
        }

        var newPower = await PowerCmd.Apply<NuclearAttackPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (newPower != null)
        {
            newPower.CurrentDamage = damage;
            newPower.CurrentPoison = poison;
            newPower.IsUpgraded = isUpgraded;
            GD.Print($"[NuclearAttackPower] 创建成功 - Damage={newPower.CurrentDamage}, Poison={newPower.CurrentPoison}, IsUpgraded={newPower.IsUpgraded}");
        }
    }

    /// <summary>
    /// 玩家回合结束时触发：按层数循环对全部敌人造成伤害并赋予中毒，随后移除自身。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;
        if (Owner == null || !participants.Contains(Owner))
            return;

        int stacks = (int)base.Amount;
        GD.Print($"[NuclearAttackPower] 回合结束触发 - 层数={stacks}, Damage={CurrentDamage}, Poison={CurrentPoison}");

        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        var enemies = combatState.Enemies
            .Where(e => e.Side == CombatSide.Enemy && e.IsAlive)
            .ToList();
        if (enemies.Count == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        // 触发能力时播放核弹爆炸音效，并对全部敌人播放“下砸+火焰”攻击特效
        PlayNuclearExplosionSound();
        foreach (var enemy in enemies)
        {
            PlayNuclearImpactVfx(enemy);
        }
        await Cmd.Wait(0.4f);

        var ctx = new ThrowingPlayerChoiceContext();
        for (int i = 0; i < stacks; i++)
        {
            foreach (var enemy in enemies)
            {
                await CreatureCmd.Damage(ctx, enemy, (decimal)CurrentDamage, ValueProp.Unpowered, null, null);
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, enemy, (decimal)CurrentPoison, Owner, null);
            }
            GD.Print($"[NuclearAttackPower] 第{i + 1}层触发 - 对全部敌人造成 {CurrentDamage} 点伤害，赋予 {CurrentPoison} 层中毒");
        }

        // 触发后移除
        await PowerCmd.Remove(this);
    }

    /// <summary>
    /// 播放核弹“下砸+火焰”攻击特效（复用原版 vfx_heavy_blunt + NFireBurningVfx）
    /// </summary>
    private void PlayNuclearImpactVfx(Creature target)
    {
        try
        {
            if (target == null)
                return;

            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");

            var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
            }

            GD.Print($"[NuclearAttackPower] 播放核弹下砸+火焰特效 - 目标: {target.Name}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttackPower] 播放核弹特效失败: {ex.Message}");
        }
    }

    private void PlayNuclearExplosionSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "NuclearAttackPowerExplosion";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_explosion.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[NuclearAttackPower] 播放核弹爆炸音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttackPower] 播放爆炸音效失败: {ex.Message}");
        }
    }
}
