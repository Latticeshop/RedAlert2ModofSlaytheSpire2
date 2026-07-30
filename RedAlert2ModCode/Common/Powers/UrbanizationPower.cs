using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public sealed class UrbanizationPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.UrbanizationPower;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.None;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/urbanization_power.png";

    public override LocString Title => new LocString("powers", Id.Entry + ".title");

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", Id.Entry + ".description");
            locString.Add("DrawCount", (int)Values.Damage);
            return locString;
        }
    }

    /// <summary>
    /// 从弃牌堆/抽牌堆中抽取建筑/防御塔卡牌（含围墙，因为是从牌堆筛选，不是触发判定）。
    /// 仅在玩家拥有城市化能力（打出 UrbanizationCard）后打出建筑/防御塔牌时触发。
    /// </summary>
    private static async Task TriggerDrawInternal(PlayerChoiceContext choiceContext, Player player)
    {
        int drawCount = (int)Values.Damage;
        int cardsDrawn = 0;

        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);

        var discardPileCards = discardPile.Cards
            .Where(c => c is CardModel cm && CardUtils.IsBuildingOrDefenseTower(cm))
            .ToList();

        GD.Print($"[UrbanizationPower] 弃牌堆中有 {discardPileCards.Count} 张建筑/防御塔牌");

        foreach (var card in discardPileCards)
        {
            if (cardsDrawn >= drawCount) break;
            await CardPileCmd.Add(card, PileType.Hand);
            cardsDrawn++;
            GD.Print($"[UrbanizationPower] 从弃牌堆找到: {card.Id.Entry}");
        }

        if (cardsDrawn < drawCount)
        {
            var drawPileCards = drawPile.Cards
                .Where(c => c is CardModel cm && CardUtils.IsBuildingOrDefenseTower(cm))
                .ToList();

            GD.Print($"[UrbanizationPower] 抽牌堆中有 {drawPileCards.Count} 张建筑/防御塔牌");

            foreach (var card in drawPileCards)
            {
                if (cardsDrawn >= drawCount) break;
                await CardPileCmd.Add(card, PileType.Hand);
                cardsDrawn++;
                GD.Print($"[UrbanizationPower] 从抽牌堆找到: {card.Id.Entry}");
            }
        }

        GD.Print($"[UrbanizationPower] 成功抽取 {cardsDrawn}/{drawCount} 张建筑/防御塔牌");
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // 非围墙建筑/防御塔才触发城市化抽牌（围墙不触发）
        if (!CardUtils.IsNonWallBuildingOrDefenseTower(cardPlay.Card))
            return;

        // 选择面板类建筑卡（重工、兵营、MCV 等）在玩家取消选择时会调用 CardUtils.HandleCardCancellation，
        // 并由其标记本次打出已取消。此处统一检测：取消则跳过城市化抽牌，仅在成功打出时触发。
        // 因此所有建筑/防御塔卡牌都无需在自身 OnPlay 中硬编码触发调用。
        if (CardUtils.WasCardPlayCancelled(cardPlay))
        {
            GD.Print($"[UrbanizationPower] 卡牌 {cardPlay.Card.Id.Entry} 已取消选择，跳过城市化抽牌");
            return;
        }

        GD.Print($"[UrbanizationPower] 打出建筑/防御塔牌 {cardPlay.Card.Id.Entry}");
        await TriggerDrawInternal(choiceContext, base.Owner.Player);
    }
}
