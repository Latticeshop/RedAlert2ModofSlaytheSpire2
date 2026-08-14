#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 裂缝产生器 - 盟军防御塔建筑（防御卡，T3 牌组建筑）
/// 3费（升级2）能力卡，uncommon 蓝卡，价格1000
/// 效果：每回合降低全体敌人 1 点力量，若回合结束没有能量则失效（获得裂缝产生器能力）。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class GapGenerator : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GapGenerator;

    public GapGenerator() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/gapicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("StrengthLoss", Values.MagicNumber),
        new EnergyVar(1)
    };

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.DefenseTower.CreateHoverTip(),
        HoverTipFactory.FromPower<EnergyNextTurnPower>(),
        HoverTipFactory.FromPower<BlackCurtainPower>()
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            // 检查是否拥有MCV能力（建造厂）
            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[GapGenerator] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        // 扣除资金
        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-Values.DollarValue);
            GD.Print($"[GapGenerator] 扣除资金 {Values.DollarValue}");
        }

        // 获得裂缝产生器能力（可叠层，每层能力 1 层黑幕）
        await PowerCmd.Apply<GapGeneratorPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[GapGenerator] 已获得裂缝产生器能力");
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(Values.CostUpgraded);
    }
}
