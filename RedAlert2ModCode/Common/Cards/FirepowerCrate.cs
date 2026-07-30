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
using RedAlert2ModCode.Common.Powers;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Cards;

public class FirepowerCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.FirepowerCrate;

    public FirepowerCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
        ModCardKeywords.Unit.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayFirepowerSound();
        GD.Print("[FirepowerCrate] 获得火力增幅，本回合单位卡伤害+50%");
        await PowerCmd.Apply<FirepowerPower>(ctx, Owner.Creature, 0.5m, Owner.Creature, this);
    }
}
