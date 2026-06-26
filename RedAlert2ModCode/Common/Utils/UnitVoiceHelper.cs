using Godot;
using System;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Utils;

public static class UnitVoiceHelper
{
    private static AudioStreamPlayer? _audioPlayer;
    
    private static readonly Random _random = new();

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Name = "UnitVoicePlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
        GD.Print("[UnitVoiceHelper] 创建单位语音播放器");
    }

    public static void PlayUnitVoice(Type unitType, string faction = "Allied")
    {
        string unitName = unitType.Name;
        if (unitName.EndsWith("Card"))
        {
            unitName = unitName.Substring(0, unitName.Length - 4);
        }
        PlayUnitVoice(unitName, faction);
    }

    public static void PlayUnitVoice(string unitName, string faction = "Allied")
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            List<string> voices = UnitVoiceConfig.GetUnitVoices(unitName, faction);
            
            if (voices == null || voices.Count == 0)
            {
                GD.Print($"[UnitVoiceHelper] 未找到单位 \"{unitName}\" 在阵营 \"{faction}\" 的语音配置");
                return;
            }

            string selectedPath = voices[_random.Next(voices.Count)];
            var sound = GD.Load<AudioStream>(selectedPath);
            if (sound != null)
            {
                if (_audioPlayer.Playing) _audioPlayer.Stop();
                _audioPlayer.Stream = sound;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print($"[UnitVoiceHelper] 播放: {selectedPath}");
            }
            else
            {
                GD.PrintErr($"[UnitVoiceHelper] 无法加载: {selectedPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UnitVoiceHelper] 播放失败: {ex.Message}");
        }
    }

    public static bool HasVoice(string unitName, string faction = "Allied")
    {
        return UnitVoiceConfig.HasUnitVoices(unitName, faction);
    }
}