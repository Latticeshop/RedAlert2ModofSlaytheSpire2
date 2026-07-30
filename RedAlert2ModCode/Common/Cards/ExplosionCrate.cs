using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Cards;

public class ExplosionCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.ExplosionCrate;

    public ExplosionCrate() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[0];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move)
    };

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayRandomExplosionSound();

        decimal damage = DynamicVars.Damage.BaseValue;
        GD.Print($"[ExplosionCrate] 对自己造成 {damage} 点伤害");

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(Owner.Creature)
            .Execute(ctx);
    }
}
