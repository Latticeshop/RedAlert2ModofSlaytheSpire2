using Godot;

namespace RedAlert2ModCode.Common.Utils;

public interface IBuildingSoundProvider
{
    void PlayBuildingPlaceSound();
    
    void PlayBuildingSellSound();
}

public static class BuildingSoundHelper
{
    private const string BuildingPlaceSoundPath = "res://RedAlert2ModResources/audio/CommonSFX/building_place.wav";
    private const string BuildingSellSoundPath = "res://RedAlert2ModResources/audio/CommonSFX/sell_building.wav";
    
    private static AudioStreamPlayer? _placeAudioPlayer;
    private static AudioStreamPlayer? _sellAudioPlayer;

    private static void EnsurePlaceAudioPlayer()
    {
        if (_placeAudioPlayer != null && GodotObject.IsInstanceValid(_placeAudioPlayer))
            return;

        _placeAudioPlayer = new AudioStreamPlayer();
        _placeAudioPlayer.Name = "BuildingPlaceSoundPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        if (root != null)
        {
            root.Root.AddChild(_placeAudioPlayer);
            GD.Print("[BuildingSoundHelper] 创建建筑建造音效播放器");
        }
    }

    private static void EnsureSellAudioPlayer()
    {
        if (_sellAudioPlayer != null && GodotObject.IsInstanceValid(_sellAudioPlayer))
            return;

        _sellAudioPlayer = new AudioStreamPlayer();
        _sellAudioPlayer.Name = "BuildingSellSoundPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        if (root != null)
        {
            root.Root.AddChild(_sellAudioPlayer);
            GD.Print("[BuildingSoundHelper] 创建建筑出售音效播放器");
        }
    }

    public static void PlayBuildingPlaceSound()
    {
        try
        {
            EnsurePlaceAudioPlayer();
            if (_placeAudioPlayer == null)
            {
                GD.PrintErr("[BuildingSoundHelper] 建造音效播放器未初始化");
                return;
            }

            var soundFile = GD.Load<AudioStream>(BuildingPlaceSoundPath);
            if (soundFile != null)
            {
                if (_placeAudioPlayer.Playing)
                {
                    _placeAudioPlayer.Stop();
                }
                
                _placeAudioPlayer.Stream = soundFile;
                _placeAudioPlayer.VolumeDb = -5;
                _placeAudioPlayer.Play();
                GD.Print("[BuildingSoundHelper] 播放建筑建造音效");
            }
            else
            {
                GD.PrintErr($"[BuildingSoundHelper] 无法加载音效文件: {BuildingPlaceSoundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BuildingSoundHelper] 播放建造音效失败: {ex.Message}");
        }
    }

    public static void PlayBuildingSellSound()
    {
        try
        {
            EnsureSellAudioPlayer();
            if (_sellAudioPlayer == null)
            {
                GD.PrintErr("[BuildingSoundHelper] 出售音效播放器未初始化");
                return;
            }

            var soundFile = GD.Load<AudioStream>(BuildingSellSoundPath);
            if (soundFile != null)
            {
                if (_sellAudioPlayer.Playing)
                {
                    _sellAudioPlayer.Stop();
                }
                
                _sellAudioPlayer.Stream = soundFile;
                _sellAudioPlayer.VolumeDb = -5;
                _sellAudioPlayer.Play();
                GD.Print("[BuildingSoundHelper] 播放建筑出售音效");
            }
            else
            {
                GD.PrintErr($"[BuildingSoundHelper] 无法加载音效文件: {BuildingSellSoundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BuildingSoundHelper] 播放出售音效失败: {ex.Message}");
        }
    }
}