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
}
