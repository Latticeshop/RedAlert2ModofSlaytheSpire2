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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
namespace RedAlert2ModCode.Common.Cards;

public class MassProductionCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.MassProduction;

    public MassProductionCard() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

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

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/MassProduction.png";

            protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reduction", (int)Values.Stars)
    };

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy((int)Values.CostUpgraded);
        // 升级后每层减少的价格从100增加到150
        DynamicVars["Reduction"].UpgradeValueBy((int)Values.StarsUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        await MassProductionPower.ApplyMassProduction(Owner.Creature, IsUpgraded);
    }
}