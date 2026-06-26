using Godot;

namespace RedAlert2ModCode.Common.Utils;

public static class BuildingSoundHelper
{
    private const string BuildingPlaceSoundPath = "res://RedAlert2ModResources/audio/building_place.wav";
    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Name = "BuildingSoundPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        if (root != null)
        {
            root.Root.AddChild(_audioPlayer);
            GD.Print("[BuildingSoundHelper] 创建建筑音效播放器");
        }
    }

    public static void PlayBuildingPlaceSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null)
            {
                GD.PrintErr("[BuildingSoundHelper] 音效播放器未初始化");
                return;
            }

            var soundFile = GD.Load<AudioStream>(BuildingPlaceSoundPath);
            if (soundFile != null)
            {
                if (_audioPlayer.Playing)
                {
                    _audioPlayer.Stop();
                }
                
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print("[BuildingSoundHelper] 播放建筑释放音效");
            }
            else
            {
                GD.PrintErr($"[BuildingSoundHelper] 无法加载音效文件: {BuildingPlaceSoundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BuildingSoundHelper] 播放音效失败: {ex.Message}");
        }
    }
}