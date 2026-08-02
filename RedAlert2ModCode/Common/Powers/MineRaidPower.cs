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
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public sealed class MineRaidPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private bool IsMinerCard(CardModel card)
    {
        return card is RedAlert2ModCode.Allies.Cards.ChronoMiner || card is RedAlert2ModCode.Soviet.Cards.WarMiner;
    }

    private static HashSet<System.Type>? _unitCardTypes;

    private static HashSet<System.Type> GetUnitCardTypes()
    {
        if (_unitCardTypes == null)
            _unitCardTypes = CardUtils.GetUnitTypes();
        return _unitCardTypes;
    }

    private bool IsUnitCard(CardModel card)
    {
        // 仅本mod的"单位"卡牌触发扰矿效果，排除箱子、技能、建筑等非单位卡
        return GetUnitCardTypes().Contains(card.GetType());
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查是否是自己打出的卡牌，是否是单位卡，且不是矿车卡
        if (cardPlay.Card.Owner != base.Owner.Player || !IsUnitCard(cardPlay.Card) || IsMinerCard(cardPlay.Card))
            return;

        GD.Print($"[MineRaidPower] 打出单位卡 {cardPlay.Card.Id.Entry}，触发扰矿效果，层数={Amount}");

        AudioHelper.PlayMineRaidSound();

        int cardsToDraw = (int)Amount;
        int cardsDrawn = 0;

        var drawPile = PileType.Draw.GetPile(base.Owner.Player);
        var discardPile = PileType.Discard.GetPile(base.Owner.Player);

        var discardPileMiners = discardPile.Cards
            .Where(c => IsMinerCard(c))
            .ToList();

        GD.Print($"[MineRaidPower] 弃牌堆中有 {discardPileMiners.Count} 张矿车卡");

        foreach (var card in discardPileMiners)
        {
            if (cardsDrawn >= cardsToDraw) break;
            await CardPileCmd.Add(card, PileType.Hand);
            cardsDrawn++;
            GD.Print($"[MineRaidPower] 从弃牌堆找到矿车卡: {card.Id.Entry}");
        }

        if (cardsDrawn < cardsToDraw)
        {
            var drawPileMiners = drawPile.Cards
                .Where(c => IsMinerCard(c))
                .ToList();

            GD.Print($"[MineRaidPower] 抽牌堆中有 {drawPileMiners.Count} 张矿车卡");

            foreach (var card in drawPileMiners)
            {
                if (cardsDrawn >= cardsToDraw) break;
                await CardPileCmd.Add(card, PileType.Hand);
                cardsDrawn++;
                GD.Print($"[MineRaidPower] 从抽牌堆找到矿车卡: {card.Id.Entry}");
            }
        }

        GD.Print($"[MineRaidPower] 成功抽取 {cardsDrawn} 张矿车卡");

        await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            GD.Print($"[MineRaidPower] 回合结束，移除扰矿能力");
            await PowerCmd.Remove(this);
        }
    }
}