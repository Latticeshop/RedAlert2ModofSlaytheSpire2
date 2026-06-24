using Godot;
using System;
using System.Collections.Generic;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 单位语音播放辅助类
/// 集中处理单位语音播放，使用 UnitVoiceConfig 配置的语音文件列表，随机选择播放
/// </summary>
public static class UnitVoiceHelper
{
    // 静态AudioStreamPlayer用于播放音效
    private static AudioStreamPlayer? _audioPlayer;
    
    // 随机数生成器
    private static readonly Random _random = new();

    /// <summary>
    /// 确保AudioStreamPlayer存在
    /// </summary>
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

    /// <summary>
    /// 播放单位语音（根据单位类型名称，默认盟军阵营）
    /// </summary>
    /// <param name="unitType">单位类型（通常是卡牌类名）</param>
    /// <param name="faction">阵营名称（Allied/Soviet/Yuri）</param>
    public static void PlayUnitVoice(Type unitType, string faction = "Allied")
    {
        string unitName = unitType.Name;
        // 移除可能的"Card"后缀
        if (unitName.EndsWith("Card"))
        {
            unitName = unitName.Substring(0, unitName.Length - 4);
        }
        PlayUnitVoice(unitName, faction);
    }

    /// <summary>
    /// 播放单位语音（根据单位名称随机选择一条语音）
    /// </summary>
    /// <param name="unitName">单位名称</param>
    /// <param name="faction">阵营名称（Allied/Soviet/Yuri）</param>
    public static void PlayUnitVoice(string unitName, string faction = "Allied")
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            // 从配置中获取语音列表
            List<string> voices = UnitVoiceConfig.GetUnitVoices(unitName, faction);
            
            if (voices == null || voices.Count == 0)
            {
                GD.Print($"[UnitVoiceHelper] 未找到单位 \"{unitName}\" 在阵营 \"{faction}\" 的语音配置");
                return;
            }

            // 随机选择一条语音
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

    /// <summary>
    /// 检查指定单位是否有语音配置
    /// </summary>
    /// <param name="unitName">单位名称</param>
    /// <param name="faction">阵营名称</param>
    /// <returns>是否有语音配置</returns>
    public static bool HasVoice(string unitName, string faction = "Allied")
    {
        return UnitVoiceConfig.HasUnitVoices(unitName, faction);
    }
}
