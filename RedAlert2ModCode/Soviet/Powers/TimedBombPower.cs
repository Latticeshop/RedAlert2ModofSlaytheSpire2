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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class TimedBombPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/dynamite.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Stacks", (int)Amount);
            return locString;
        }
    }

    private static AudioStreamPlayer? _countdownAudioPlayer;
    private static bool _isCountdownPlaying;

    private static void EnsureCountdownAudioPlayer()
    {
        if (_countdownAudioPlayer != null && GodotObject.IsInstanceValid(_countdownAudioPlayer))
            return;

        _countdownAudioPlayer = new AudioStreamPlayer();
        _countdownAudioPlayer.Name = "TimedBombCountdownAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_countdownAudioPlayer);
        
        // 监听音效播放完毕信号，实现循环播放
        _countdownAudioPlayer.Finished += OnCountdownFinished;
    }

    private static void OnCountdownFinished()
    {
        // 如果应该继续播放，则重新播放
        if (_isCountdownPlaying && _countdownAudioPlayer != null && GodotObject.IsInstanceValid(_countdownAudioPlayer))
        {
            _countdownAudioPlayer.Play();
            GD.Print("[TimedBombPower] 倒计时音效循环播放");
        }
    }

    public void StartCountdownSound()
    {
        try
        {
            EnsureCountdownAudioPlayer();
            if (_countdownAudioPlayer == null) return;

            // 如果已经在播放，先停止
            if (_countdownAudioPlayer.Playing)
            {
                _countdownAudioPlayer.Stop();
            }

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/CommonSFX/timed_bomb/Icraloop_countdown.mp3");
            if (soundFile != null)
            {
                _countdownAudioPlayer.Stream = soundFile;
                _countdownAudioPlayer.VolumeDb = -25;
                _isCountdownPlaying = true;
                _countdownAudioPlayer.Play();
                GD.Print("[TimedBombPower] 开始播放炸弹倒计时音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TimedBombPower] 播放倒计时音效失败: {ex.Message}");
        }
    }

    public void StopCountdownSound()
    {
        try
        {
            _isCountdownPlaying = false;
            if (_countdownAudioPlayer != null && GodotObject.IsInstanceValid(_countdownAudioPlayer))
            {
                _countdownAudioPlayer.Stop();
                GD.Print("[TimedBombPower] 停止播放炸弹倒计时音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TimedBombPower] 停止倒计时音效失败: {ex.Message}");
        }
    }

    private void PlayExplosionSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "TimedBombExplosionAudioPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/CommonSFX/timed_bomb/timed_bomb_explosion.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[TimedBombPower] 播放炸弹爆炸音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TimedBombPower] 播放爆炸音效失败: {ex.Message}");
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Enemy && Owner != null && Owner.IsAlive)
        {
            int currentStacks = (int)Amount;
            GD.Print($"[TimedBombPower] 回合开始，当前层数: {currentStacks}");

            if (currentStacks > 0)
            {
                int newStacks = currentStacks - 1;
                
                if (newStacks == 0)
                {
                    StopCountdownSound();
                    PlayExplosionSound();

                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                        Owner,
                        15m,
                        ValueProp.Move,
                        null,
                        null);

                    GD.Print("[TimedBombPower] 炸弹爆炸，造成15点伤害");

                    // 直接移除能力，确保伤害已触发
                    await PowerCmd.Remove(this);
                    GD.Print("[TimedBombPower] 移除定时炸弹能力");
                }
                else
                {
                    await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1m, Owner, null);
                    GD.Print($"[TimedBombPower] 减少1层，剩余层数: {newStacks}");
                }
            }
        }
    }

    public void OnPowerApplied()
    {
        if ((int)Amount > 0)
        {
            StartCountdownSound();
        }
    }

    public void OnPowerRemoved()
    {
        StopCountdownSound();
    }

    /// <summary>
    /// 当所有者死亡时停止倒计时音效
    /// </summary>
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (target == Owner)
        {
            StopCountdownSound();
            GD.Print("[TimedBombPower] 所有者死亡，停止倒计时音效");
        }
        await base.AfterDeath(choiceContext, target, wasRemovalPrevented, deathAnimLength);
    }
}
