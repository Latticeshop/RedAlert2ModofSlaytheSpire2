#nullable enable

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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class V3RocketPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.V3RocketPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDamage { get; set; } = (int)Values.Damage;

    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/v3.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int displayDamage = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : CurrentDamage;
            locString.Add("Damage", displayDamage);
            return locString;
        }
    }

    public static async Task<V3RocketPower?> ApplyV3Rocket(Creature owner, bool isUpgraded = false)
    {
        int damage = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
        
        var existingPower = owner.Powers.OfType<V3RocketPower>().FirstOrDefault(p => p.CurrentDamage == damage);
        
        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[V3RocketPower] 叠加到已存在的V3火箭能力，层数: {existingPower.Amount}，伤害: {damage}");
            return existingPower;
        }
        
        var power = await PowerCmd.Apply<V3RocketPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentDamage = damage;
            power.IsUpgraded = isUpgraded;
        }
        return power;
    }

    private void PlaySmashEffect(Creature target)
    {
        try
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bloody_impact");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[V3RocketPower] 播放特效失败: {ex.Message}");
        }
    }

    private static AudioStreamPlayer? _launchAudioPlayer;
    private static AudioStreamPlayer? _explosionAudioPlayer;

    private static void EnsureLaunchAudioPlayer()
    {
        if (_launchAudioPlayer != null && GodotObject.IsInstanceValid(_launchAudioPlayer))
            return;

        _launchAudioPlayer = new AudioStreamPlayer();
        _launchAudioPlayer.Name = "V3LaunchAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_launchAudioPlayer);
        GD.Print("[V3RocketPower] 创建发射音效播放器");
    }

    private static void EnsureExplosionAudioPlayer()
    {
        if (_explosionAudioPlayer != null && GodotObject.IsInstanceValid(_explosionAudioPlayer))
            return;

        _explosionAudioPlayer = new AudioStreamPlayer();
        _explosionAudioPlayer.Name = "V3ExplosionAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_explosionAudioPlayer);
        GD.Print("[V3RocketPower] 创建爆炸音效播放器");
    }

    private void PlayV3LaunchSound()
    {
        try
        {
            EnsureLaunchAudioPlayer();
            if (_launchAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/v3_launch.mp3");
            if (soundFile != null)
            {
                _launchAudioPlayer.Stream = soundFile;
                _launchAudioPlayer.Play();
                GD.Print("[V3RocketPower] 播放V3发射音效");
            }
            else
            {
                GD.PrintErr("[V3RocketPower] 无法加载V3发射音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[V3RocketPower] 播放发射音效失败: {ex.Message}");
        }
    }

    private void PlayRandomExplosionSound()
    {
        try
        {
            var explosionFiles = new List<string>
            {
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_01.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_02.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_03.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_04.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_05.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_06.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_07.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_08.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_09.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_10.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_11.wav",
                "res://RedAlert2ModResources/audio/ExplosionSFX/explosion_12.wav",
            };

            var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
            int randomIndex = rng?.NextInt(explosionFiles.Count) ?? new Random().Next(explosionFiles.Count);
            string randomFile = explosionFiles[randomIndex];

            EnsureExplosionAudioPlayer();
            if (_explosionAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>(randomFile);
            if (soundFile != null)
            {
                _explosionAudioPlayer.Stream = soundFile;
                _explosionAudioPlayer.Play();
                GD.Print($"[V3RocketPower] 播放随机爆炸音效: {randomFile}");
            }
            else
            {
                GD.PrintErr($"[V3RocketPower] 无法加载爆炸音效: {randomFile}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[V3RocketPower] 播放爆炸音效失败: {ex.Message}");
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner != null)
        {
            var targetLockedEnemies = combatState.Enemies
                .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive &&
                               enemy.Powers.Any(p => p is TargetLockedPower))
                .ToList();

            if (targetLockedEnemies.Any())
            {
                int stacks = (int)Amount;
                GD.Print($"[V3RocketPower] 触发V3火箭，层数: {stacks}，伤害: {CurrentDamage}");

                for (int i = 0; i < stacks; i++)
                {
                    PlayV3LaunchSound();
                    await Cmd.Wait(0.5f);

                    Creature target = targetLockedEnemies.First();
                    PlaySmashEffect(target);
                    PlayRandomExplosionSound();
                    await Cmd.Wait(0.3f);

                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                        new List<Creature> { target },
                        (decimal)CurrentDamage,
                        ValueProp.Move,
                        Owner,
                        null);

                    if (i < stacks - 1)
                    {
                        await Cmd.Wait(0.2f);
                    }
                }

                await PowerCmd.Remove(this);
            }
        }
    }
}