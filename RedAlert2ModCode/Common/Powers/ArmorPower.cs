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
/// 装甲能力 - 本回合单位卡牌格挡翻倍
/// 通过 ModifyBlockMultiplicative 钩子直接修改卡牌格挡值
/// </summary>
public sealed class ArmorPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => true;

    public override LocString Description => new("powers", base.Id.Entry + ".description");

    public override decimal ModifyBlockMultiplicative(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (base.Owner != target)
            return 1m;

        if (!props.IsPoweredCardOrMonsterMoveBlock())
            return 1m;

        if (cardSource == null || !CardUtils.GetUnitTypes().Contains(cardSource.GetType()))
            return 1m;

        GD.Print($"[ArmorPower] 单位卡 {cardSource.Id.Entry} 格挡翻倍");
        return 2m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        GD.Print("[ArmorPower] 玩家回合结束，移除装甲增益");
        await PowerCmd.Remove(this);
    }
}
