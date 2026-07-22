using Godot;
using System;

namespace RedAlert2ModCode.Common.Utils;

public static class CommonSoundHelper
{
    private static AudioStreamPlayer? _chronoMoveAudioPlayer;

    private static void EnsureChronoMoveAudioPlayer()
    {
        if (_chronoMoveAudioPlayer != null && GodotObject.IsInstanceValid(_chronoMoveAudioPlayer))
            return;

        _chronoMoveAudioPlayer = new AudioStreamPlayer();
        _chronoMoveAudioPlayer.Name = "ChronoMoveAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_chronoMoveAudioPlayer);
    }

    public static void PlayChronoMoveSound()
    {
        try
        {
            EnsureChronoMoveAudioPlayer();
            if (_chronoMoveAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/CommonSFX/chrono_move.wav");
            if (soundFile != null)
            {
                _chronoMoveAudioPlayer.Stream = soundFile;
                _chronoMoveAudioPlayer.VolumeDb = -5;
                _chronoMoveAudioPlayer.Play();
                GD.Print("[CommonSoundHelper] 播放超时空移动音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CommonSoundHelper] 播放超时空移动音效失败: {ex.Message}");
        }
    }
}