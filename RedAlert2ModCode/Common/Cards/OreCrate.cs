using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

public class OreCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.OreCrate;

    public OreCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.GoldMine.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var oreCards = GetOreCards();
        if (oreCards.Count == 0)
        {
            GD.PrintErr("[OreCrate] 没有可用的矿区卡");
            return;
        }

        // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
        int index = Owner.RunState.Rng.CombatCardSelection.NextInt(oreCards.Count);
        var card = oreCards[index];
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[OreCrate] 获得矿区卡: {card.Title}");
    }

    private List<CardModel> GetOreCards()
    {
        var oreTypes = new List<System.Type>
        {
            typeof(GoldMineCard),
            typeof(GemMineCard),
            typeof(GoldMineColumnCard),
            typeof(OilDerrickCard),
        };

        var result = new List<CardModel>();
        foreach (var type in oreTypes)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)?.MakeGenericMethod(type);
            var template = (CardModel)method?.Invoke(null, null);
            var card = Owner.Creature.CombatState.CreateCard(template, Owner);
            result.Add(card);
        }

        return result;
    }
}
