using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public sealed class SteelFloodPower : PowerModel
{
    private class Data
    {
        public readonly HashSet<CardModel> AutoPlayingCards = new();
        public readonly Queue<CardModel> PendingAutoPlayQueue = new();
        public bool IsProcessingAutoPlay;
        public int InfiniteAutoPlaysThisTurn;
        public bool ShowedCapReachedMessage;
        public int AutoPlayAttempts;
    }

    private const int InfiniteAutoPlayCap = 9;
    private const int MaxAutoPlayAttempts = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool IsProcessingAutoPlay => GetInternalData<Data>().IsProcessingAutoPlay;

    protected override object InitInternalData() => new Data();

    /// <summary>
    /// 超时空类卡牌（如超时空矿车）虽然属于单位卡，但需要特殊的目标选择逻辑，
    /// 不应被钢铁洪流自动打出。
    /// </summary>
    private static readonly HashSet<System.Type> ChronoCardTypes = new()
    {
        typeof(RedAlert2ModCode.Allies.Cards.ChronoMiner)
    };

    /// <summary>
    /// 本mod全部单位卡牌类型缓存（从各阵营卡牌注册类聚合获取）。
    /// </summary>
    private static HashSet<System.Type>? _unitCardTypes;

    private static HashSet<System.Type> GetUnitCardTypes()
    {
        if (_unitCardTypes == null)
            _unitCardTypes = CardUtils.GetUnitTypes();
        return _unitCardTypes;
    }

    private bool IsCardValidForAutoPlay(CardModel card)
    {
        // 仅允许本mod的"单位"卡牌（从卡牌注册类获取列表），排除箱子、技能、建筑等非单位卡
        if (!GetUnitCardTypes().Contains(card.GetType()))
            return false;

        if (card.Keywords.Contains(CardKeyword.Unplayable))
            return false;

        if (ChronoCardTypes.Contains(card.GetType()))
            return false;

        return true;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner.Creature != base.Owner)
            return;

        if (card.Pile?.Type != PileType.Hand)
            return;

        Data data = GetInternalData<Data>();

        if (data.AutoPlayingCards.Contains(card))
            return;

        if (!IsCardValidForAutoPlay(card))
            return;

        if (data.IsProcessingAutoPlay)
        {
            GD.Print($"[SteelFloodPower] 正在处理自动出牌，将 {card.Id.Entry} 加入等待队列");
            data.PendingAutoPlayQueue.Enqueue(card);
            return;
        }

        await ProcessAutoPlay(card);
    }

    private async Task ProcessAutoPlay(CardModel card)
    {
        Data data = GetInternalData<Data>();

        data.IsProcessingAutoPlay = true;
        data.AutoPlayingCards.Add(card);
        data.AutoPlayAttempts = 0;

        try
        {
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

            if (shouldAutoPlay)
            {
                GD.Print($"[SteelFloodPower] 自动打出单位卡 - {card.Id.Entry}");
                await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
            }

            while (data.PendingAutoPlayQueue.Count > 0)
            {
                CardModel pendingCard = data.PendingAutoPlayQueue.Dequeue();
                
                if (pendingCard.Pile?.Type != PileType.Hand)
                    continue;
                
                if (!IsCardValidForAutoPlay(pendingCard))
                {
                    GD.Print($"[SteelFloodPower] 等待队列中的卡牌 {pendingCard.Id.Entry} 无效，跳过");
                    continue;
                }

                bool shouldPlayPending = true;

                if (base.Owner.CombatState.HittableEnemies.All(c => c.HpDisplay.IsInfinite()))
                {
                    if (data.InfiniteAutoPlaysThisTurn >= InfiniteAutoPlayCap)
                    {
                        shouldPlayPending = false;
                    }
                    data.InfiniteAutoPlaysThisTurn++;
                }
                else
                {
                    ResetInfiniteAutoPlayData();
                }

                if (shouldPlayPending)
                {
                    GD.Print($"[SteelFloodPower] 处理等待队列中的单位卡 - {pendingCard.Id.Entry}");
                    await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), pendingCard, null);
                }
            }
        }
        finally
        {
            data.AutoPlayingCards.Remove(card);
            data.IsProcessingAutoPlay = false;
        }
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            ResetInfiniteAutoPlayData();
            Data data = GetInternalData<Data>();
            data.PendingAutoPlayQueue.Clear();
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