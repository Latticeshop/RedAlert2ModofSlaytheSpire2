using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class SovietGiantSquidPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.GiantSquid;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sqdicon.png";

    public int CurrentStacks { get; set; } = (int)Values.MagicNumber;

    public bool IsUpgraded { get; set; } = false;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int displayAmount = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : CurrentStacks;
            locString.Add("Count", displayAmount);
            return locString;
        }
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;

        int squidStacks = (int)Amount;
        if (squidStacks <= 0) return;

        if ((int)Owner.CurrentHp <= squidStacks)
        {
            Flash();
            PlaySinkSound();
            
            await CreatureCmd.Kill(Owner);
            return;
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;
        
        int damage = (int)Amount;
        if (damage <= 0) return;

        Flash();
        PlayAttackSound();
        
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner,
            (decimal)damage,
            ValueProp.Move,
            null,
            null);
    }

    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Name = "GiantSquidAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
    }

    private void PlayAttackSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/GiantSquid/attack.mp3");
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SovietGiantSquidPower] 播放攻击音效失败: {ex.Message}");
        }
    }

    private void PlaySinkSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/GiantSquid/sink.wav");
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SovietGiantSquidPower] 播放沉船音效失败: {ex.Message}");
        }
    }
}
