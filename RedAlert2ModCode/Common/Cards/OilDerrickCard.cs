using System.Collections.Generic;
using System.Linq;
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

public class OilDerrickCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.OilDerrick;

    public OilDerrickCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

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

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/oil_derrick.png";

            protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("DollarPerTurn", Values.Damage)
    };

    protected override void OnUpgrade()
    {
        DynamicVars["DollarPerTurn"].UpgradeValueBy(Values.DamageUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        BuildingSoundHelper.PlayBuildingPlaceSound();
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower == null)
        {
            dollarPower = await PowerCmd.Apply<DollarPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, (decimal)Values.DollarValue, Owner.Creature, null);
            GD.Print($"[OilDerrickCard] 未找到DollarPower，已创建并添加资金 {Values.DollarValue}");
        }
        else
        {
            dollarPower.AddDollar((int)Values.DollarValue);
            GD.Print($"[OilDerrickCard] 立即获得资金 {Values.DollarValue}");
        }

        await OilDerrickPower.ApplyOilDerricks(Owner.Creature, 1, IsUpgraded);
    }
}