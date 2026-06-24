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
    // 盟军角色选择语音路径列表
    private static readonly List<string> _alliesSelectVoices = new()
    {
        "res://RedAlert2ModResources/audio/character_select/itanata.mp3",
        "res://RedAlert2ModResources/audio/character_select/itanatb.mp3",
        "res://RedAlert2ModResources/audio/character_select/itanatc.mp3",
        "res://RedAlert2ModResources/audio/character_select/itansec.mp3",
        "res://RedAlert2ModResources/audio/character_select/itapatb.mp3",
        "res://RedAlert2ModResources/audio/character_select/itapcra.mp3",
        "res://RedAlert2ModResources/audio/character_select/itapcrd.mp3",
        "res://RedAlert2ModResources/audio/character_select/itapmoa.mp3",
        "res://RedAlert2ModResources/audio/character_select/itapsed.mp3",
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
    /// 播放盟军角色选择语音（随机选择一个）
    /// </summary>
    public static void PlayAlliesSelectVoice()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null)
                return;

            // 随机选择一个语音
            Random random = new();
            string voicePath = _alliesSelectVoices[random.Next(_alliesSelectVoices.Count)];

            // 加载音效文件
            var soundFile = GD.Load<AudioStream>(voicePath);
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -8; // 设置音量（dB）
                _audioPlayer.Play();
                GD.Print($"[CharacterSelectAudioHelper] 播放盟军角色选择语音: {voicePath}");
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
