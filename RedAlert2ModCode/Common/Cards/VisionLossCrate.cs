using System.Collections.Generic;
using System.Threading.Tasks;
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

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 视野丢失箱子 - 0费token，消耗，只能通过随机箱子获得
/// 获得2(升级1)层虚弱
/// </summary>
public class VisionLossCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.VisionLossCrate;

    public VisionLossCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>()
    };

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("WeakNumber", Values.MagicNumber)
    };

    protected override void OnUpgrade()
    {
        DynamicVars["WeakNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int weakStacks = DynamicVars["WeakNumber"].IntValue;
        GD.Print($"[VisionLossCrate] 获得 {weakStacks} 层虚弱");
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, weakStacks, Owner.Creature, this);
    }
}
