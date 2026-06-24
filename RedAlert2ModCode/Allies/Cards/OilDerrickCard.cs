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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 油井 - 中立建筑卡（不受建造厂限制）
/// 能力卡，1费
/// 效果：立即获得$1000，回合开始时获得$200（升级后$500）资金
/// </summary>
public sealed class OilDerrickCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.OilDerrick;

    public OilDerrickCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/oil_derrick.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("DollarPerTurn", Values.Damage)
    };

    protected override void OnUpgrade()
    {
        // 升级后每回合资金从200增加到500
        DynamicVars["DollarPerTurn"].UpgradeValueBy(Values.DamageUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 播放建筑释放音效
        BuildingSoundHelper.PlayBuildingPlaceSound();
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 立即获得$1000资金
        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar((int)Values.DollarValue);
            GD.Print($"[OilDerrickCard] 立即获得资金 {Values.DollarValue}");
        }

        // 应用油井能力（使用ApplyOilDerricks确保正确叠加）
        await OilDerrickPower.ApplyOilDerricks(Owner.Creature, 1, IsUpgraded);
    }
}