using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class GrandCannon : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GrandCannon;

    public GrandCannon() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/gcanicon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT2.CreateHoverTip(),
        ModCardKeywords.DefenseTower.CreateHoverTip()
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("DamageUpgraded", Values.Damage + Values.DamageUpgraded),
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        BuildingSoundHelper.PlayBuildingPlaceSound();

        var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[GrandCannon] 扣除资金 {Values.DollarValue}");
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (play.Target is not Creature target)
        {
            GD.Print("[GrandCannon] 目标无效");
            return;
        }

        await TargetLockedManager.ApplyTargetLocked(target, Owner.Creature, this);
        GD.Print($"[GrandCannon] 为目标 {target.Name} 赋予目标锁定");

        await GrandCannonPower.ApplyGrandCannon(Owner.Creature, base.IsUpgraded);
        GD.Print($"[GrandCannon] 应用巨炮能力 - IsUpgraded={base.IsUpgraded}");

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}
