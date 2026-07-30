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
using RedAlert2ModCode.Common.Powers;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

public class MoneyCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.MoneyCrate;

    public MoneyCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[0];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Amount", 3500)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
        // GD.RandRange(2000, 5000) 两端闭区间 → NextInt(minInclusive, maxExclusive)
        int amount = Owner.RunState.Rng.CombatCardSelection.NextInt(2000, 5001);
        GD.Print($"[MoneyCrate] 获得资金 ${amount}");

        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(amount);
        }
        else
        {
            var newPower = await PowerCmd.Apply<DollarPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
            if (newPower != null)
            {
                newPower.AddDollar(amount);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
