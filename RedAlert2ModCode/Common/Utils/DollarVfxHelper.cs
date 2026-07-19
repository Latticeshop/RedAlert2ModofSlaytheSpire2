#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Ui;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;
using System;

namespace RedAlert2ModCode.Common.Utils;

public enum DollarVfxType
{
    None,
    Gain,
    Spend
}

public static class DollarVfxHelper
{
    private const string DollarGainSoundPath = "res://RedAlert2ModResources/audio/dollar_gain.wav";
    
    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
    }

    private static void PlayDollarGainSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null)
                return;

            var soundFile = GD.Load<AudioStream>(DollarGainSoundPath);
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print("[DollarVfxHelper] 播放资金增加音效");
            }
            else
            {
                GD.PrintErr($"[DollarVfxHelper] 无法加载音效文件: {DollarGainSoundPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarVfxHelper] 播放音效失败: {ex.Message}");
        }
    }

    public static void PlayVfx(Creature owner, int amount, DollarVfxType vfxType)
    {
        if (TestMode.IsOn || amount <= 0) return;

        try
        {
            var dollarPower = owner.Powers.FirstOrDefault(p => p is RedAlert2ModCode.Common.Powers.DollarPower) as RedAlert2ModCode.Common.Powers.DollarPower;
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(owner);

            switch (vfxType)
            {
                case DollarVfxType.None:
                    GD.Print($"[DollarVfxHelper] 资金变化 {amount}，无动画");
                    break;

                case DollarVfxType.Gain:
                    if (dollarPower != null)
                    {
                        dollarPower.FlashPower();
                        GD.Print($"[DollarVfxHelper] 资金增加 {amount}，闪烁刀乐能力");
                    }
                    if (creatureNode != null)
                    {
                        var buffVfx = NPowerAppliedBuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                        if (buffVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(buffVfx);
                            GD.Print("[DollarVfxHelper] 播放增益动画（绿色粒子）");
                        }
                    }
                    PlayDollarGainSound();
                    break;

                case DollarVfxType.Spend:
                    if (dollarPower != null)
                    {
                        dollarPower.FlashPower();
                        GD.Print($"[DollarVfxHelper] 资金扣除 {amount}，闪烁刀乐能力");
                    }
                    if (creatureNode != null)
                    {
                        var debuffVfx = NPowerAppliedDebuffVfx.Create(creatureNode.PowerAppliedVfxSpawnPosition);
                        if (debuffVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(debuffVfx);
                            GD.Print("[DollarVfxHelper] 播放减益动画（虚弱效果）");
                        }
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarVfxHelper] 播放资金动画失败: {ex.Message}");
        }
    }

    public static void PlayGainVfx(Creature owner, int amount)
    {
        PlayVfx(owner, amount, DollarVfxType.Gain);
    }

    public static void PlaySpendVfx(Creature owner, int amount)
    {
        PlayVfx(owner, amount, DollarVfxType.Spend);
    }
}