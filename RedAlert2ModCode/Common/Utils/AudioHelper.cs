using Godot;
using System;

namespace RedAlert2ModCode.Common.Utils;

public static class AudioHelper
{
    private static AudioStreamPlayer? _effectPlayer;

    private static void EnsureEffectPlayer()
    {
        if (_effectPlayer != null && GodotObject.IsInstanceValid(_effectPlayer))
            return;

        _effectPlayer = new AudioStreamPlayer();
        _effectPlayer.Name = "EffectSoundPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_effectPlayer);
    }

    public static void PlayTeslaTrooperChargeSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/charge.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放磁暴步兵充能音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayTeslaCoilChargeSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/SovietUnits/TeslaCoil/charge.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放磁暴线圈蓄力音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayTeslaCoilAttackSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/SovietUnits/TeslaCoil/attack.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放磁暴线圈攻击音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayPrismTowerChargeSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/AlliedUnits/PrismTower/charge.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放光棱塔蓄力音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayPrismTowerAttackSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/AlliedUnits/PrismTower/attack.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放光棱塔攻击音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayMineRaidSound()
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/CommonSFX/mine_raid.mp3";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放扰矿音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlaySupportCheer()
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/CommonSFX/cheer.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放支援欢呼音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放音效失败: {ex.Message}");
        }
    }

    private static readonly string[] _explosionFiles = new[]
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

    public static void PlayRandomExplosionSound()
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            int randomIndex = (int)GD.RandRange(0, _explosionFiles.Length - 1);
            string soundPath = _explosionFiles[randomIndex];

            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放随机爆炸音效: {soundPath}");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载爆炸音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放爆炸音效失败: {ex.Message}");
        }
    }

    public static void PlaySealC4Voice()
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaexa-c4.mp3";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print("[AudioHelper] 播放海豹突击队C4语音");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载C4语音: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放C4语音失败: {ex.Message}");
        }
    }

    public static void PlayHornetMissileSound(object target)
    {
        try
        {
            EnsureEffectPlayer();
            if (_effectPlayer == null) return;

            string soundPath = "res://RedAlert2ModResources/audio/AlliedUnits/IFV/missile.wav";
            var sound = GD.Load<AudioStream>(soundPath);
            if (sound != null)
            {
                if (_effectPlayer.Playing) _effectPlayer.Stop();
                _effectPlayer.Stream = sound;
                _effectPlayer.VolumeDb = -5;
                _effectPlayer.Play();
                GD.Print($"[AudioHelper] 播放黄蜂导弹音效");
            }
            else
            {
                GD.PrintErr($"[AudioHelper] 无法加载音效: {soundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AudioHelper] 播放黄蜂导弹音效失败: {ex.Message}");
        }
    }
}
