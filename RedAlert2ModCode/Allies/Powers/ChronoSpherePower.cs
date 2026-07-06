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
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class ChronoSpherePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>
    /// 是否升级
    /// </summary>
    private bool _isUpgraded;
    public bool IsUpgraded
    {
        get => _isUpgraded;
        set
        {
            if (value != _isUpgraded)
            {
                _isUpgraded = value;
                if (value && _initialized)
                {
                    _turnCounter = GetInterval();
                }
            }
        }
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int displayInterval = IsUpgraded ? (int)(AlliesCardValues.ChronoSphere.Repeat + AlliesCardValues.ChronoSphere.RepeatUpgraded) : _turnCounter;
            locString.Add("TurnsRemaining", displayInterval);
            string chronoWarpName = IsUpgraded ? "超时空传送+" : "超时空传送";
            locString.Add("ChronoWarpName", chronoWarpName);
            return locString;
        }
    }

    private int _turnCounter = (int)AlliesCardValues.ChronoSphere.Repeat;
    private bool _initialized = false;

    private int GetInterval()
    {
        var values = AlliesCardValues.ChronoSphere;
        return IsUpgraded ? values.RepeatUpgraded : values.Repeat;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 立即初始化倒计时
        _initialized = true;
        GD.Print($"[ChronoSpherePower] 能力应用，初始化倒计时: {_turnCounter}");
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (!_initialized)
            return;

        _turnCounter--;
        GD.Print($"[ChronoSpherePower] 回合开始，剩余回合: {_turnCounter}");

        if (_turnCounter <= 0)
        {
            _turnCounter = GetInterval();
            
            var chronoWarpCard = Owner.CombatState.CreateCard(ModelDb.Card<ChronoWarp>(), Owner.Player);
            
            if (chronoWarpCard != null)
            {
                if (IsUpgraded)
                {
                    CardCmd.Upgrade(chronoWarpCard);
                    GD.Print("[ChronoSpherePower] 超时空传送已升级");
                }
                
                chronoWarpCard.EnergyCost.SetCustomBaseCost(0);
                chronoWarpCard.AddKeyword(CardKeyword.Ethereal);
                chronoWarpCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print("[ChronoSpherePower] 成功为超时空传送添加0费、虚无和消耗词条");

                PlayChronoReadySound();
                
                await CardPileCmd.AddGeneratedCardToCombat(chronoWarpCard, PileType.Hand, Owner.Player);
                GD.Print("[ChronoSpherePower] 成功添加超时空传送到手牌");
            }
        }
    }

    private void PlayChronoReadySound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "ChronoReadySoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/AlliedUnits/ChronoWarp/chrono_ready.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[ChronoSpherePower] 播放超时空传送就绪音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ChronoSpherePower] 播放音效失败: {ex.Message}");
        }
    }
}
