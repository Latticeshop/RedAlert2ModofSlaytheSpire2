using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Utils;

public static class UnitVoiceHelper
{
    private static readonly Random _random = new();

    public static readonly string[] SovietAaSfxPaths = new string[]
    {
        "res://RedAlert2ModResources/audio/CommonSFX/soviet_aa_sfx/aa_fire_1.wav",
        "res://RedAlert2ModResources/audio/CommonSFX/soviet_aa_sfx/aa_fire_2.wav",
        "res://RedAlert2ModResources/audio/CommonSFX/soviet_aa_sfx/aa_fire_3.wav",
        "res://RedAlert2ModResources/audio/CommonSFX/soviet_aa_sfx/aa_fire_4.wav",
    };

    public static void PlayRandomSound(IEnumerable<string> paths)
    {
        var list = paths.ToList();
        if (list.Count == 0) return;
        string selected = list[_random.Next(list.Count)];
        PlaySound(selected);
    }

    public static void PlaySovietAaSfx()
    {
        PlayRandomSound(SovietAaSfxPaths);
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
            List<string> voices = UnitVoiceConfig.GetUnitVoices(unitName, faction);
            
            if (voices == null || voices.Count == 0)
            {
                GD.Print($"[UnitVoiceHelper] 未找到单位 \"{unitName}\" 在阵营 \"{faction}\" 的语音配置");
                return;
            }

            string selectedPath = voices[_random.Next(voices.Count)];
            PlaySound(selectedPath);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UnitVoiceHelper] 播放失败: {ex.Message}");
        }
    }

    public static void PlaySound(string path)
    {
        var sound = GD.Load<AudioStream>(path);
        if (sound != null)
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = $"UnitVoicePlayer_{Guid.NewGuid()}";
            audioPlayer.Stream = sound;
            audioPlayer.VolumeDb = -5;
            
            var root = Engine.GetMainLoop() as SceneTree;
            root?.Root.AddChild(audioPlayer);
            
            audioPlayer.Play();
            GD.Print($"[UnitVoiceHelper] 播放: {path}");

            audioPlayer.Finished += () =>
            {
                if (GodotObject.IsInstanceValid(audioPlayer))
                {
                    audioPlayer.QueueFree();
                }
            };
        }
        else
        {
            GD.PrintErr($"[UnitVoiceHelper] 无法加载: {path}");
        }
    }

    public static bool HasVoice(string unitName, string faction = "Allied")
    {
        return UnitVoiceConfig.HasUnitVoices(unitName, faction);
    }
}
