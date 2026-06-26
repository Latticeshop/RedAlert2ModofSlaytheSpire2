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
        var values = AlliesCardValues.ChronoSphere;
        return IsUpgraded ? values.RepeatUpgraded : values.Repeat;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 立即初始化倒计时
        _turnCounter = GetInterval();
        _initialized = true;
        GD.Print($"[ChronoSpherePower] 能力应用，初始化倒计时: {_turnCounter}");
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
        GD.Print($"[ChronoSpherePower] 回合开始，剩余回合: {_turnCounter}");

        if (_turnCounter <= 0)
        {
            // 重置倒计时
            _turnCounter = GetInterval();
            
            var chronoWarpCard = Owner.CombatState.CreateCard(ModelDb.Card<ChronoWarp>(), Owner.Player);
            
            if (chronoWarpCard != null)
            {
                // 设置为0费
                chronoWarpCard.EnergyCost.SetCustomBaseCost(0);
                // 添加虚无和消耗词条
                chronoWarpCard.AddKeyword(CardKeyword.Ethereal);
                chronoWarpCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print("[ChronoSpherePower] 成功为超时空传送添加0费、虚无和消耗词条");
                
                await CardPileCmd.AddGeneratedCardToCombat(chronoWarpCard, PileType.Hand, Owner.Player);
                GD.Print("[ChronoSpherePower] 成功添加超时空传送到手牌");
            }
        }
    }
}
