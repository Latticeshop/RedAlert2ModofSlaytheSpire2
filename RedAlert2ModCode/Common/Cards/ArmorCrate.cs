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

public class ArmorCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.ArmorCrate;

    public ArmorCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ArmorPower>(),
        ModCardKeywords.Unit.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayArmorSound();
        GD.Print("[ArmorCrate] 获得装甲增幅，本回合单位卡格挡翻倍");
        await PowerCmd.Apply<ArmorPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
    }
}
