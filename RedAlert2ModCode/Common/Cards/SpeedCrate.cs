using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Cards;

public class SpeedCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.SpeedCrate;

    public SpeedCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Dexterity", Values.MagicNumber)
    };

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlaySpeedSound();

        int dexterityAmount = IsUpgraded
            ? Values.MagicNumber + Values.MagicNumberUpgraded
            : Values.MagicNumber;

        GD.Print($"[SpeedCrate] 获得 {dexterityAmount} 敏捷 (永久)");
        await PowerCmd.Apply<DexterityPower>(ctx, Owner.Creature, dexterityAmount, Owner.Creature, this);
    }
}
