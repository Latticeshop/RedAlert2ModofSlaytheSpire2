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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 控制中心 - 盟军高科技建筑卡（T2，需要空指部/雷达能力）
/// 0费能力卡，uncommon 蓝卡，价格600
/// 效果：盟军重工解锁遥控坦克（获得控制中心能力，重工生产选项中展示遥控坦克）。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class ControlCenter : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.ControlCenter;

    public ControlCenter() : base((int)Values.Cost, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/rbccicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.TechLevelT2.CreateHoverTip(),
        HoverTipFactory.FromCard<RoboTank>(),
        HoverTipFactory.FromCard<AlliedWarFactory>()
    ];

    /// <summary>
    /// 需要 MCV（建造厂）、足够资金、以及空指部/雷达能力（T2 科技）才可打出。
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            // 检查有“空指部”/“雷达”能力（T2 科技）
            if (!AlliedCardRegistry.HasAirForceCommandPower(Owner.Creature))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[ControlCenter] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        // 扣除资金
        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-Values.DollarValue);
            GD.Print($"[ControlCenter] 扣除资金 {Values.DollarValue}");
        }

        // 获得控制中心能力（可叠层，供盟军重工检查解锁遥控坦克）
        await PowerCmd.Apply<ControlCenterPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
        GD.Print("[ControlCenter] 已获得控制中心能力（盟军重工解锁遥控坦克）");
    }

    protected override void OnUpgrade()
    {
        // 控制中心无升级数值变化
    }
}
