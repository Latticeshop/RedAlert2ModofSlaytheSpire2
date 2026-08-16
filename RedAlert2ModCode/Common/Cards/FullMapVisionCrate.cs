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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 全图视野箱子 - 1费(升级保留)技能卡，消耗
/// 本回合免疫[gold]虚弱[/gold]和[gold]脆弱[/gold]（玩家回合开始时移除，能力不可叠层）
/// </summary>
public class FullMapVisionCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.FullMapVisionCrate;

    public FullMapVisionCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FullMapVisionPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override void OnUpgrade()
    {
        // 1费升级保留，无数值变化
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/CommonSFX/vision_gain.wav");
        GD.Print("[FullMapVisionCrate] 获得全图视野（本回合免疫虚弱和脆弱）");
        await PowerCmd.Apply<FullMapVisionPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
    }
}
