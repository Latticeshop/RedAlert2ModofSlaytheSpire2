using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 火力能力 - 本回合单位卡牌伤害+50%
/// 通过 ModifyDamageMultiplicative 钩子直接修改卡牌伤害值
/// </summary>
public sealed class FirepowerPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => true;

    public override LocString Description => new("powers", base.Id.Entry + ".description");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != base.Owner)
            return 1m;

        if (!props.IsPoweredAttack())
            return 1m;

        if (cardSource == null || !CardUtils.GetUnitTypes().Contains(cardSource.GetType()))
            return 1m;

        GD.Print($"[FirepowerPower] 单位卡 {cardSource.Id.Entry} 伤害+50%");
        return 1.5m;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        GD.Print("[FirepowerPower] 玩家回合开始，移除火力增益");
        await PowerCmd.Remove(this);
    }
}
