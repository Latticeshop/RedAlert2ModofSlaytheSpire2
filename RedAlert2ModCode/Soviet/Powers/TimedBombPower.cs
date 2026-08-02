#nullable enable

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

    private AudioStreamPlayer? _countdownAudioPlayer;
    private Godot.Timer? _countdownTimer;
    private bool _isAudioInitialized;
    private bool _isExploding;

    private void InitializeAudio()
    {
        if (_isAudioInitialized)
            return;

        _countdownAudioPlayer = new AudioStreamPlayer();
        _countdownAudioPlayer.Name = $"TimedBombCountdownAudioPlayer_{GetHashCode()}";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_countdownAudioPlayer);

        var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/CommonSFX/timed_bomb/Icraloop_countdown.mp3");
        if (soundFile != null)
        {
            _countdownAudioPlayer.Stream = soundFile;
            _countdownAudioPlayer.VolumeDb = -25;
        }

        _countdownTimer = new Godot.Timer();
        _countdownTimer.Name = $"TimedBombCountdownTimer_{GetHashCode()}";
        _countdownTimer.WaitTime = 0.5f;
        _countdownTimer.Autostart = false;
        _countdownTimer.OneShot = false;
        _countdownTimer.Timeout += OnCountdownTick;
        root?.Root.AddChild(_countdownTimer);

        _isAudioInitialized = true;
        GD.Print($"[TimedBombPower] 音频组件初始化完成");
    }

    private void OnCountdownTick()
    {
        if (!IsPowerActive())
        {
            StopCountdownSound();
            return;
        }

        if (_countdownAudioPlayer != null && GodotObject.IsInstanceValid(_countdownAudioPlayer) && !_countdownAudioPlayer.Playing)
        {
            _countdownAudioPlayer.Play();
            GD.Print($"[TimedBombPower] 倒计时音效触发播放");
        }
    }

    private bool IsPowerActive()
    {
        if (Owner == null || !Owner.IsAlive)
            return false;

        try
        {
            return Owner.Powers.Contains(this);
        }
        catch
        {
            return false;
        }
    }

    public void StartCountdownSound()
    {
        try
        {
            InitializeAudio();

            if (_countdownTimer != null && GodotObject.IsInstanceValid(_countdownTimer))
            {
                _countdownTimer.Start();
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
            if (_countdownTimer != null && GodotObject.IsInstanceValid(_countdownTimer))
            {
                _countdownTimer.Stop();
            }

            if (_countdownAudioPlayer != null && GodotObject.IsInstanceValid(_countdownAudioPlayer))
            {
                _countdownAudioPlayer.Stop();
            }

            GD.Print("[TimedBombPower] 停止播放炸弹倒计时音效");
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

    private void PlayExplosionVfx()
    {
        try
        {
            if (Owner == null) return;

            VfxCmd.PlayOnCreatureCenter(Owner, "vfx/vfx_bloody_impact");

            var fireVfx = NFireBurningVfx.Create(Owner, 1.5f, goingRight: true);
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
            }

            GD.Print("[TimedBombPower] 播放爆炸特效");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TimedBombPower] 播放爆炸特效失败: {ex.Message}");
        }
    }

    public async Task Explode()
    {
        if (_isExploding) return;
        _isExploding = true;

        try
        {
            StopCountdownSound();
            PlayExplosionSound();
            PlayExplosionVfx();

            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                Owner,
                20m,
                ValueProp.Move,
                null,
                null);

            GD.Print("[TimedBombPower] 炸弹爆炸，造成20点伤害");

            await PowerCmd.Remove(this);
            GD.Print("[TimedBombPower] 移除定时炸弹能力");
        }
        finally
        {
            _isExploding = false;
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
                    await Explode();
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
