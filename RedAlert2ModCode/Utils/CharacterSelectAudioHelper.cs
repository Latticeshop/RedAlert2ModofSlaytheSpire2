#nullable enable

using Godot;
using System;
using System.Collections.Generic;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 角色选择语音辅助类
/// 用于在选择角色时播放随机语音
/// </summary>
public static class CharacterSelectAudioHelper
{
    // 盟军角色选择语音路径列表（谭雅）
    private static readonly List<string> _alliesSelectVoices = new()
    {
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itanata.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itanatb.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itanatc.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itansec.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itapatb.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itapcra.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itapcrd.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itapmoa.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/itapsed.mp3",
    };

    // 苏军角色选择语音路径列表（娜塔莎）
    private static readonly List<string> _sovietSelectVoices = new()
    {
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiCrea.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiCreb.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiCrec.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiMova.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiMovd.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiMovh.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiSelBatb.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiSeld.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiSelf.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaSelectVoice/RA3 SUNatas VoiSelh.mp3",
    };

    // 静态AudioStreamPlayer用于播放音效
    private static AudioStreamPlayer? _audioPlayer;

    /// <summary>
    /// 确保AudioStreamPlayer存在
    /// </summary>
    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        // 添加到场景树
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
        GD.Print("[CharacterSelectAudioHelper] 创建AudioStreamPlayer");
    }

    /// <summary>
    /// 播放盟军角色选择语音（谭雅，随机选择一个）
    /// </summary>
    public static void PlayAlliesSelectVoice()
    {
        PlayRandomVoice(_alliesSelectVoices, "盟军");
    }

    /// <summary>
    /// 播放苏军角色选择语音（娜塔莎，随机选择一个）
    /// </summary>
    public static void PlaySovietSelectVoice()
    {
        PlayRandomVoice(_sovietSelectVoices, "苏军");
    }

    /// <summary>
    /// 播放随机语音的通用方法
    /// </summary>
    private static void PlayRandomVoice(List<string> voicePaths, string factionName)
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null)
                return;

            // 随机选择一个语音
            Random random = new();
            string voicePath = voicePaths[random.Next(voicePaths.Count)];

            // 加载音效文件
            var soundFile = GD.Load<AudioStream>(voicePath);
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -8; // 设置音量（dB）
                _audioPlayer.Play();
                GD.Print($"[CharacterSelectAudioHelper] 播放{factionName}角色选择语音: {voicePath}");
            }
            else
            {
                GD.PrintErr($"[CharacterSelectAudioHelper] 无法加载语音文件: {voicePath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CharacterSelectAudioHelper] 播放语音失败: {ex.Message}");
        }
    }
}
