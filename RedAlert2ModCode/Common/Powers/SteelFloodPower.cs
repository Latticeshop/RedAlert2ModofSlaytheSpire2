using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Common.Powers;

public sealed class SteelFloodPower : PowerModel
{
    private class Data
    {
        public readonly HashSet<CardModel> AutoPlayingCards = new();
        public int InfiniteAutoPlaysThisTurn;
        public bool ShowedCapReachedMessage;
    }

    private const int InfiniteAutoPlayCap = 9;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner.Creature != base.Owner)
            return;

        if (card.Pile?.Type != PileType.Hand)
            return;

        if (card.Rarity != CardRarity.Token)
            return;

        if (card.Keywords.Contains(CardKeyword.Unplayable))
            return;

        Data data = GetInternalData<Data>();

        if (data.AutoPlayingCards.Contains(card))
            return;

        bool shouldAutoPlay = true;

        if (base.Owner.CombatState.HittableEnemies.All(c => c.HpDisplay.IsInfinite()))
        {
            if (data.InfiniteAutoPlaysThisTurn >= InfiniteAutoPlayCap)
            {
                shouldAutoPlay = false;
                if (!data.ShowedCapReachedMessage)
                {
                    GD.Print("[SteelFloodPower] 无限自动出牌上限已达");
                    data.ShowedCapReachedMessage = true;
                }
            }
            data.InfiniteAutoPlaysThisTurn++;
        }
        else
        {
            ResetInfiniteAutoPlayData();
        }

        if (!shouldAutoPlay)
            return;

        GD.Print($"[SteelFloodPower] 单位卡进入手牌，立即自动打出 - {card.Id.Entry}");

        data.AutoPlayingCards.Add(card);
        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
        data.AutoPlayingCards.Remove(card);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            ResetInfiniteAutoPlayData();
        }
        return Task.CompletedTask;
    }

    private void ResetInfiniteAutoPlayData()
    {
        Data data = GetInternalData<Data>();
        data.InfiniteAutoPlaysThisTurn = 0;
        data.ShowedCapReachedMessage = false;
    }
}