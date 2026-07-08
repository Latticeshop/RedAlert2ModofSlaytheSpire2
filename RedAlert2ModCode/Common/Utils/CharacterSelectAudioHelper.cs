#nullable enable

using Godot;
using System;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Utils;

public static class CharacterSelectAudioHelper
{
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
        "res://RedAlert2ModResources/audio/character_select/TanyaSelectVoice/Itapsea.mp3",
    };

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

    private static readonly List<string> _tanyaDamageVoices = new()
    {
        "res://RedAlert2ModResources/audio/character_select/TanyaDamageVoice/Itapdia.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaDamageVoice/Itapdib.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaDamageVoice/Itapdic.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaDamageVoice/Itapdid.mp3",
        "res://RedAlert2ModResources/audio/character_select/TanyaDamageVoice/Itapdie.mp3",
    };

    private static readonly List<string> _natashaDamageVoices = new()
    {
        "res://RedAlert2ModResources/audio/character_select/NatashaDamageVoice/Icfadia.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaDamageVoice/Icfadib.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaDamageVoice/Icfadic.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaDamageVoice/Icfsdic.mp3",
        "res://RedAlert2ModResources/audio/character_select/NatashaDamageVoice/Icfssea.mp3",
    };

    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
        GD.Print("[CharacterSelectAudioHelper] 创建AudioStreamPlayer");
    }

    public static void PlayAlliesSelectVoice()
    {
        PlayRandomVoice(_alliesSelectVoices, "盟军");
    }

    public static void PlaySovietSelectVoice()
    {
        PlayRandomVoice(_sovietSelectVoices, "苏军");
    }

    public static void PlayTanyaDamageVoice()
    {
        PlayRandomVoice(_tanyaDamageVoices, "盟军受伤");
    }

    public static void PlayNatashaDamageVoice()
    {
        PlayRandomVoice(_natashaDamageVoices, "苏军受伤");
    }

    private static void PlayRandomVoice(List<string> voicePaths, string factionName)
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null)
                return;

            Random random = new();
            string voicePath = voicePaths[random.Next(voicePaths.Count)];

            var soundFile = GD.Load<AudioStream>(voicePath);
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -8;
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