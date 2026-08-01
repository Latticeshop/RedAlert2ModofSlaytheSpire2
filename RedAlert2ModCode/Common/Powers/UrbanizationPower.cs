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
    /// 从弃牌堆/抽牌堆中抽取指定类型的卡牌。
    /// 若无可抽则跳过。
    /// </summary>
    /// <param name="targetTypes">要抽取的卡牌类型集合</param>
    /// <param name="targetLabel">日志中显示的目标类型名称</param>
    private static async Task TriggerDrawInternal(PlayerChoiceContext choiceContext, Player player, HashSet<Type> targetTypes, string targetLabel)
    {
        int drawCount = (int)Values.Damage;
        int cardsDrawn = 0;

        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);

        var discardPileCards = discardPile.Cards
            .Where(c => targetTypes.Contains(c.GetType()))
            .ToList();

        GD.Print($"[UrbanizationPower] 弃牌堆中有 {discardPileCards.Count} 张{targetLabel}");

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
                .Where(c => targetTypes.Contains(c.GetType()))
                .ToList();

            GD.Print($"[UrbanizationPower] 抽牌堆中有 {drawPileCards.Count} 张{targetLabel}");

            foreach (var card in drawPileCards)
            {
                if (cardsDrawn >= drawCount) break;
                await CardPileCmd.Add(card, PileType.Hand);
                cardsDrawn++;
                GD.Print($"[UrbanizationPower] 从抽牌堆找到: {card.Id.Entry}");
            }
        }

        GD.Print($"[UrbanizationPower] 成功抽取 {cardsDrawn}/{drawCount} 张{targetLabel}");
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // 城市化交叉抽牌逻辑：
        // - 打出建筑（非围墙、非防御塔）→ 抽防御塔牌（含围墙，围墙归属于防御塔类）
        // - 打出防御塔（非围墙）→ 抽建筑牌（纯建筑，不含围墙和防御塔）
        // - 围墙不触发城市化抽牌

        HashSet<Type>? targetTypes = null;
        string targetLabel = "";

        if (CardUtils.IsNonWallNonDefenseTowerBuilding(cardPlay.Card))
        {
            // 打出建筑 → 抽防御塔（含围墙）
            targetTypes = CardUtils.GetAllDefenseTowerTypesWithWalls();
            targetLabel = "防御塔";
            GD.Print($"[UrbanizationPower] 打出建筑牌 {cardPlay.Card.Id.Entry}，抽取防御塔");
        }
        else if (CardUtils.IsNonWallDefenseTower(cardPlay.Card))
        {
            // 打出防御塔 → 抽建筑（纯建筑）
            targetTypes = CardUtils.GetNonWallNonDefenseTowerBuildingTypes();
            targetLabel = "建筑";
            GD.Print($"[UrbanizationPower] 打出防御塔牌 {cardPlay.Card.Id.Entry}，抽取建筑");
        }

        // 围墙或其他卡牌不触发
        if (targetTypes == null)
            return;

        // 选择面板类建筑卡（重工、兵营、MCV 等）在玩家取消选择时会调用 CardUtils.HandleCardCancellation，
        // 并由其标记本次打出已取消。此处统一检测：取消则跳过城市化抽牌，仅在成功打出时触发。
        if (CardUtils.WasCardPlayCancelled(cardPlay))
        {
            GD.Print($"[UrbanizationPower] 卡牌 {cardPlay.Card.Id.Entry} 已取消选择，跳过城市化抽牌");
            return;
        }

        await TriggerDrawInternal(choiceContext, base.Owner.Player, targetTypes, targetLabel);
    }
}
