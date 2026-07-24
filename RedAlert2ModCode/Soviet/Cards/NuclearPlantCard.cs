#nullable enable

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
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class NuclearPlantCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.NuclearPlant;

    public NuclearPlantCard() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nrcticon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Energy", (int)Values.MagicNumber),
        new IntVar("Damage", (int)Values.Damage),
        new IntVar("Poison", (int)Values.Repeat),
        new IntVar("DollarNumber", (int)Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Building.CreateHoverTip()
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(SovietBattleLabPower).Name))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        BuildingSoundHelper.PlayBuildingPlaceSound();

        var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[NuclearPlantCard] 扣除资金 {Values.DollarValue}");
        }

        await NuclearReactorCorePower.ApplyNuclearReactorCore(Owner!.Creature, IsUpgraded);

        GD.Print($"[NuclearPlantCard] 核电站已建造，升级状态: {IsUpgraded}");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
        DynamicVars["Damage"].UpgradeValueBy((int)Values.DamageUpgraded);
    }
}
