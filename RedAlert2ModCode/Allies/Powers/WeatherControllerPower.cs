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

public sealed class WeatherControllerPower : PowerModel
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
            locString.Add("TurnsRemaining", _turnCounter);
            locString.Add("Block", (int)AlliesCardValues.WeatherController.Block);
            return locString;
        }
    }

    private int _turnCounter = (int)AlliesCardValues.WeatherController.Repeat;
    private bool _initialized = false;

    private int GetInterval()
    {
        var values = AlliesCardValues.WeatherController;
        return IsUpgraded ? values.RepeatUpgraded : values.Repeat;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 立即初始化倒计时
        _initialized = true;
        GD.Print($"[WeatherControllerPower] 能力应用，初始化倒计时: {_turnCounter}");
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        // 如果还未初始化，跳过
        if (!_initialized)
            return;

        _turnCounter--;
        GD.Print($"[WeatherControllerPower] 回合开始，剩余回合: {_turnCounter}");

        if (_turnCounter <= 0)
        {
            // 重置倒计时
            _turnCounter = GetInterval();
            
            // 获取电球数量配置
            var values = AlliesCardValues.WeatherController;
            int orbCount = (int)values.Block;
            
            // 获得电球
            GD.Print($"[WeatherControllerPower] 获得 {orbCount} 个电球");
            for (int i = 0; i < orbCount; i++)
            {
                await OrbCmd.Channel<MegaCrit.Sts2.Core.Models.Orbs.LightningOrb>(new BlockingPlayerChoiceContext(), Owner.Player);
            }
            
            var lightningStormCard = Owner.CombatState.CreateCard(ModelDb.Card<LightningStorm>(), Owner.Player);
            
            if (lightningStormCard != null)
            {
                // 设置为0费
                lightningStormCard.EnergyCost.SetCustomBaseCost(0);
                // 添加虚无和消耗词条
                lightningStormCard.AddKeyword(CardKeyword.Ethereal);
                lightningStormCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print("[WeatherControllerPower] 成功为闪电风暴添加0费、虚无和消耗词条");
                
                await CardPileCmd.AddGeneratedCardToCombat(lightningStormCard, PileType.Hand, Owner.Player);
                GD.Print("[WeatherControllerPower] 成功添加闪电风暴到手牌");
            }
        }
    }
}