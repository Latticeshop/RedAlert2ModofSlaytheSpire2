#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class DreadnoughtPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.DreadnoughtPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/dredicon.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Repeat", (int)Values.Repeat);
            return locString;
        }
    }

    private static AudioStreamPlayer? _launchAudioPlayer;

    private static void EnsureLaunchAudioPlayer()
    {
        if (_launchAudioPlayer != null && GodotObject.IsInstanceValid(_launchAudioPlayer))
            return;

        _launchAudioPlayer = new AudioStreamPlayer();
        _launchAudioPlayer.Name = "DreadnoughtLaunchAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_launchAudioPlayer);
    }

    private void PlayDreadnoughtLaunchSound()
    {
        try
        {
            EnsureLaunchAudioPlayer();
            if (_launchAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/dreadnought_launch.mp3");
            if (soundFile != null)
            {
                _launchAudioPlayer.Stream = soundFile;
                _launchAudioPlayer.Play();
                GD.Print("[DreadnoughtPower] 播放无畏级战舰发射音效");
            }
            else
            {
                GD.PrintErr("[DreadnoughtPower] 无法加载无畏级战舰发射音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DreadnoughtPower] 播放发射音效失败: {ex.Message}");
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner != null && Amount > 0)
        {
            int stacks = (int)Amount;
            GD.Print($"[DreadnoughtPower] 触发无畏级战舰，层数: {stacks}");

            for (int i = 0; i < stacks; i++)
            {
                PlayDreadnoughtLaunchSound();

                int damage = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
                for (int j = 0; j < (int)Values.Repeat; j++)
                {
                    await V3RocketPower.ApplyV3Rocket(Owner, IsUpgraded, damage);
                }

                var targetLockedEnemies = combatState.Enemies
                    .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive &&
                                   enemy.Powers.Any(p => p is TargetLockedPower))
                    .ToList();

                if (!targetLockedEnemies.Any())
                {
                    var aliveEnemies = combatState.Enemies
                        .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
                        .ToList();

                    if (aliveEnemies.Any())
                    {
                        var rng = Owner.Player?.RunState?.Rng?.CombatCardSelection;
                        int randomIndex = rng?.NextInt(aliveEnemies.Count) ?? (int)GD.RandRange(0, aliveEnemies.Count - 1);
                        Creature randomEnemy = aliveEnemies[randomIndex];

                        await TargetLockedManager.ApplyTargetLocked(randomEnemy, Owner, null);
                        GD.Print($"[DreadnoughtPower] 随机给敌人 {randomEnemy.Name} 施加目标锁定");
                    }
                }

                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1m, Owner, null);
            }
        }
    }
}