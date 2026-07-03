using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// F2A - 钢铁洪流：公共技能卡，获得钢铁洪流能力，手牌中的单位卡将自动打出
/// </summary>
public class F2A : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.F2A;

    public F2A() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/steel_flood.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.SteelFlood.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<SteelFloodPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
