using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Cards;

public class RandomCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.RandomCrate;

    public RandomCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) {}


    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[0];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IfUpgradedVar(UpgradeDisplay.Normal)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var faction = FlagManager.GetPlayerFaction(Owner);
        var template = CrateHelper.GetRandomCrateCard(faction, excludeRandom: true);

        if (template == null)
        {
            GD.PrintErr("[RandomCrate] 无法获取随机箱子");
            return;
        }

        var card = Owner.Creature.CombatState.CreateCard(template, Owner);

        if (IsUpgraded)
        {
            CardCmd.Upgrade(card);
            GD.Print($"[RandomCrate] 升级箱子: {card.Title}");
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[RandomCrate] 获得箱子: {card.Title}");
    }
}
