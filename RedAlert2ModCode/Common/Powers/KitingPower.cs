using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public sealed class KitingPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Block) };

    private static HashSet<System.Type>? _unitCardTypes;

    private static HashSet<System.Type> GetUnitCardTypes()
    {
        if (_unitCardTypes == null)
            _unitCardTypes = CardUtils.GetUnitTypes();
        return _unitCardTypes;
    }

    private bool IsUnitCard(CardModel card)
    {
        // 仅本mod的"单位"卡牌触发走A效果，排除箱子、技能、建筑等非单位卡
        return GetUnitCardTypes().Contains(card.GetType());
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == base.Owner.Player && IsUnitCard(cardPlay.Card))
        {
            GD.Print($"[KitingPower] 打出单位卡 {cardPlay.Card.Id.Entry}，获得 {Amount} 点格挡");
            await CreatureCmd.GainBlock(base.Owner, Amount, ValueProp.Unpowered, null);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}