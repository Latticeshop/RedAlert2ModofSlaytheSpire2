#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class NuclearMissileSiloPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public bool IsUpgraded { get; set; } = false;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("TurnsRemaining", _turnCounter);
            return locString;
        }
    }

    private int _turnCounter = (int)SovietCardValues.NuclearMissileSiloCard.Repeat;
    private bool _initialized = false;

    private int GetInterval()
    {
        var values = SovietCardValues.NuclearMissileSiloCard;
        return (int)values.Repeat;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _initialized = true;
        GD.Print($"[NuclearMissileSiloPower] 能力应用，初始化倒计时: {_turnCounter}");
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (!_initialized)
            return;

        _turnCounter--;
        GD.Print($"[NuclearMissileSiloPower] 回合开始，剩余回合: {_turnCounter}");

        if (_turnCounter <= 0)
        {
            _turnCounter = GetInterval();

            var nuclearAttackCard = Owner.CombatState.CreateCard(ModelDb.Card<NuclearAttack>(), Owner.Player);

            if (nuclearAttackCard != null)
            {
                nuclearAttackCard.EnergyCost.SetCustomBaseCost(0);
                nuclearAttackCard.AddKeyword(CardKeyword.Ethereal);
                nuclearAttackCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print("[NuclearMissileSiloPower] 成功为核弹攻击添加0费、虚无和消耗词条");

                PlayNuclearReadySound();

                await CardPileCmd.AddGeneratedCardToCombat(nuclearAttackCard, PileType.Hand, Owner.Player);
                GD.Print("[NuclearMissileSiloPower] 成功添加核弹攻击到手牌");
            }
        }
    }

    private void PlayNuclearReadySound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "NuclearReadySoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_ready.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[NuclearMissileSiloPower] 播放核弹就绪音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearMissileSiloPower] 播放音效失败: {ex.Message}");
        }
    }
}