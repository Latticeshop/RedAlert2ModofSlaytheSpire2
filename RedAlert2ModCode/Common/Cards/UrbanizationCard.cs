using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class UrbanizationCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.Urbanization;

    public UrbanizationCard() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/urbanization.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.DefenseTower.CreateHoverTip(),
        HoverTipFactory.FromPower<UrbanizationPower>()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DrawCount", Values.Damage)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<UrbanizationPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[UrbanizationCard] 打出城市化，获得城市化能力");
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        GD.Print("[UrbanizationCard] 卡牌升级 - 添加固有词条");
    }
}
