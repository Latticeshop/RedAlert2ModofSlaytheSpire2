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

public class UpgradeCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.UpgradeCrate;

    public UpgradeCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Unit.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayUpgradeSound();
        AudioHelper.PlayUpgradeCrateSound();

        var handCards = PileType.Hand.GetPile(Owner)?.Cards?.ToList();
        if (handCards == null || handCards.Count == 0)
        {
            GD.Print("[UpgradeCrate] 手牌为空，无需升级");
            return;
        }

        int upgradedCount = 0;
        foreach (var card in handCards)
        {
            if (!card.IsUpgraded && CardUtils.GetUnitTypes().Contains(card.GetType()))
            {
                CardCmd.Upgrade(card);
                upgradedCount++;
                GD.Print($"[UpgradeCrate] 升级卡牌: {card.Title}");
            }
        }

        GD.Print($"[UpgradeCrate] 共升级 {upgradedCount} 张单位卡");
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
