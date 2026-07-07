#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class Dreadnought : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.Dreadnought;

    public Dreadnought() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/dredicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("DamageUpgraded", Values.Damage + Values.DamageUpgraded),
        new IntVar("V3Count", (int)Values.Repeat)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		HoverTipFactory.FromPower<TargetLockedPower>(),
		HoverTipFactory.FromPower<V3RocketPower>()
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[Dreadnought] 目标不是Creature");
            return;
        }

        await TargetLockedManager.ApplyTargetLocked(target, Owner?.Creature, this);

        int damage = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
        for (int i = 0; i < (int)Values.Repeat; i++)
        {
            await V3RocketPower.ApplyV3Rocket(Owner!.Creature, IsUpgraded, damage);
        }

        await PowerCmd.Apply<DreadnoughtPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        var power = Owner.Creature.Powers.OfType<DreadnoughtPower>().FirstOrDefault();
        if (power != null)
        {
            power.IsUpgraded = IsUpgraded;
        }
        GD.Print($"[Dreadnought] 添加无畏级战舰能力，升级状态: {IsUpgraded}");

        PlayDreadnoughtLaunchSound();
        PlayRandomVoice();
    }

    private static AudioStreamPlayer? _launchAudioPlayer;

    private static void EnsureLaunchAudioPlayer()
    {
        if (_launchAudioPlayer != null && GodotObject.IsInstanceValid(_launchAudioPlayer))
            return;

        _launchAudioPlayer = new AudioStreamPlayer();
        _launchAudioPlayer.Name = "DreadnoughtLaunchAudioPlayer_Card";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_launchAudioPlayer);
    }

    private void PlayDreadnoughtLaunchSound()
    {
        try
        {
            EnsureLaunchAudioPlayer();
            if (_launchAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/dreadnought_launch.mp3");
            if (soundFile != null)
            {
                _launchAudioPlayer.Stream = soundFile;
                _launchAudioPlayer.Play();
                GD.Print("[Dreadnought] 播放无畏级战舰发射音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Dreadnought] 播放发射音效失败: {ex.Message}");
        }
    }

    private void PlayRandomVoice()
    {
        try
        {
            var voiceFiles = new List<string>
            {
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasata.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasatb.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasatc.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasmoa.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasmoc.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasmod.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwassea.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwasseb.mp3",
                "res://RedAlert2ModResources/audio/SovietUnits/Dreadnought/Vwassec.mp3",
            };

            var rng = Owner?.RunState?.Rng?.CombatCardSelection;
            int randomIndex = rng?.NextInt(voiceFiles.Count) ?? (int)GD.RandRange(0, voiceFiles.Count - 1);
            string randomFile = voiceFiles[randomIndex];

            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "DreadnoughtVoicePlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>(randomFile);
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.Play();
                    GD.Print($"[Dreadnought] 播放随机语音: {randomFile}");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Dreadnought] 播放语音失败: {ex.Message}");
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}