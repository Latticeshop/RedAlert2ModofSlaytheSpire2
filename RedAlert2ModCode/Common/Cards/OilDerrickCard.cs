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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public sealed class OilDerrickCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.OilDerrick;

    public OilDerrickCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

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
        if (dollarPower != null)
        {
            dollarPower.AddDollar((int)Values.DollarValue);
            GD.Print($"[OilDerrickCard] 立即获得资金 {Values.DollarValue}");
        }

        await OilDerrickPower.ApplyOilDerricks(Owner.Creature, 1, IsUpgraded);
    }
}