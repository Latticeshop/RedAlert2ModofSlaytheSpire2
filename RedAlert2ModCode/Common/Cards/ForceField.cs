#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
namespace RedAlert2ModCode.Common.Cards;

public sealed class ForceField : CardModel
{
    public ForceField() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/forcicon.png";

            public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Retain
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.IntangiblePower>()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("EnergyLoss", (int)CommonPowerValues.ForceFieldPower.Damage)
    };

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        base.DynamicVars["EnergyLoss"].BaseValue = (int)CommonPowerValues.ForceFieldPower.Damage;
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[ForceField] OnPlay 被调用");

        UnitVoiceHelper.PlayUnitVoice("ForceShieldOn", "Allied");
        UnitVoiceHelper.PlayUnitVoice("PowerOutage", "Allied");

        await PowerCmd.Apply<ForceFieldPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);

        await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.IntangiblePower>(ctx, Owner.Creature, 1m, Owner.Creature, this);

        GD.Print("[ForceField] 已获得力场护盾能力和无实体效果");
    }
}