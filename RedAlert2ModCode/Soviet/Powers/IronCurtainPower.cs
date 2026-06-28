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

public sealed class IronCurtainPower : PowerModel
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

    private int _turnCounter;
    private bool _initialized = false;

    private int GetInterval()
    {
        var values = SovietCardValues.IronCurtainCard;
        return IsUpgraded ? (int)values.Repeat + (int)values.RepeatUpgraded : (int)values.Repeat;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _turnCounter = GetInterval();
        _initialized = true;
        GD.Print($"[IronCurtainPower] 能力应用，初始化倒计时: {_turnCounter}");
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (!_initialized)
            return;

        _turnCounter--;
        GD.Print($"[IronCurtainPower] 回合开始，剩余回合: {_turnCounter}");

        if (_turnCounter <= 0)
        {
            _turnCounter = GetInterval();

            var ironCurtainCard = Owner.CombatState.CreateCard(ModelDb.Card<IronCurtain>(), Owner.Player);

            if (ironCurtainCard != null)
            {
                ironCurtainCard.EnergyCost.SetCustomBaseCost(0);
                ironCurtainCard.AddKeyword(CardKeyword.Ethereal);
                ironCurtainCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print("[IronCurtainPower] 成功为铁幕添加0费、虚无和消耗词条");

                PlayIronCurtainReadySound();

                await CardPileCmd.AddGeneratedCardToCombat(ironCurtainCard, PileType.Hand, Owner.Player);
                GD.Print("[IronCurtainPower] 成功添加铁幕到手牌");
            }
        }
    }

    private void PlayIronCurtainReadySound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "IronCurtainReadySoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/IronCurtain/iron_curtain_ready.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[IronCurtainPower] 播放铁幕就绪音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[IronCurtainPower] 播放音效失败: {ex.Message}");
        }
    }
}