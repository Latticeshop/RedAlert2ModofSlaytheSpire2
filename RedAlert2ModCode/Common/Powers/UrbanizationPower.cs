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
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;

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

    private static HashSet<System.Type> _allBuildingTypes;
    private static HashSet<System.Type> _triggerBuildingTypes;

    private static HashSet<System.Type> GetAllBuildingTypes()
    {
        if (_allBuildingTypes != null)
            return _allBuildingTypes;

        var set = new HashSet<System.Type>();
        set.UnionWith(AlliedCardRegistry.GetAllBuildingCardTypes());
        set.UnionWith(AlliedCardRegistry.GetAllDefenseTowerTypes());
        set.UnionWith(SovietCardRegistry.GetAllBuildingCardTypes());
        set.UnionWith(SovietCardRegistry.GetAllDefenseTowerTypes());

        _allBuildingTypes = set;
        return set;
    }

    private static HashSet<System.Type> GetTriggerBuildingTypes()
    {
        if (_triggerBuildingTypes != null)
            return _triggerBuildingTypes;

        var set = new HashSet<System.Type>(GetAllBuildingTypes());
        set.Remove(typeof(AlliedWallCard));
        set.Remove(typeof(FortifiedWall));
        set.Remove(typeof(SovietWallCard));
        set.Remove(typeof(SovietFortifiedWall));

        _triggerBuildingTypes = set;
        return set;
    }

    private bool IsBuildingOrDefenseTower(CardModel card)
    {
        var cardType = card.GetType();
        return GetTriggerBuildingTypes().Contains(cardType);
    }

    private static readonly System.Reflection.PropertyInfo _extraHoverTipsProp =
        typeof(CardModel).GetProperty("ExtraHoverTips",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

    private bool HasCancellablePlay(CardModel card)
    {
        if (card is ICancellableCardPlay)
            return true;

        var prop = _extraHoverTipsProp;
        if (prop == null)
            return false;

        var tips = prop.GetValue(card) as IEnumerable<IHoverTip>;
        if (tips == null)
            return false;

        string prodQueueTitle = ModCardKeywords.ProductionQueue.Title.GetRawText();
        string techTreeTitle = ModCardKeywords.BuildingTechTree.Title.GetRawText();

        foreach (var tip in tips)
        {
            if (tip is HoverTip hoverTip)
            {
                var titleProp = typeof(HoverTip).GetProperty("Title");
                if (titleProp == null) continue;
                string title = titleProp.GetValue(hoverTip) as string;
                if (title == prodQueueTitle || title == techTreeTitle)
                    return true;
            }
        }
        return false;
    }

    private static async Task TriggerDrawInternal(PlayerChoiceContext choiceContext, Player player)
    {
        int drawCount = (int)Values.Damage;
        int cardsDrawn = 0;

        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);

        var discardPileCards = discardPile.Cards
            .Where(c => c is CardModel cm && IsBuildingOrDefenseTowerStatic(cm))
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
                .Where(c => c is CardModel cm && IsBuildingOrDefenseTowerStatic(cm))
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

    private static bool IsBuildingOrDefenseTowerStatic(CardModel card)
    {
        var cardType = card.GetType();
        var types = GetAllBuildingTypes();
        return types.Contains(cardType);
    }

    public static async Task TriggerOnSuccessfulPlay(PlayerChoiceContext choiceContext, Player player, CardModel card)
    {
        var power = player.Creature.Powers.OfType<UrbanizationPower>().FirstOrDefault();
        if (power == null)
        {
            GD.Print("[UrbanizationPower] 玩家没有城市化能力，跳过触发");
            return;
        }

        GD.Print($"[UrbanizationPower] 成功打出建筑/防御塔牌 {card.Id.Entry}，触发城市化抽牌");
        await TriggerDrawInternal(choiceContext, player);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        if (!IsBuildingOrDefenseTower(cardPlay.Card))
            return;

        if (HasCancellablePlay(cardPlay.Card))
        {
            GD.Print($"[UrbanizationPower] 卡牌 {cardPlay.Card.Id.Entry} 有选择面板，跳过AfterCardPlayed触发（将在成功路径触发）");
            return;
        }

        GD.Print($"[UrbanizationPower] 打出建筑/防御塔牌 {cardPlay.Card.Id.Entry}");
        await TriggerDrawInternal(choiceContext, base.Owner.Player);
    }
}
