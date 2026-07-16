using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class OreRefineryCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.OreRefinery;

    public OreRefineryCard() : base((int)Values.Cost, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/gorepicon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.TechLevelT3.CreateHoverTip()
    };

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("Bonus", Values.MagicNumber)
    };

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            if (!Owner.Creature.Powers.Any(p => p is BattleLabPower))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        var existingPower = Owner.Creature.Powers.OfType<OreRefineryPower>().FirstOrDefault();
        if (existingPower != null)
        {
            int newBonus = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
            if (newBonus > existingPower.CurrentBonus)
            {
                existingPower.CurrentBonus = newBonus;
                existingPower.IsUpgraded = IsUpgraded;
            }
            await PowerCmd.ModifyAmount(ctx, existingPower, 1m, Owner.Creature, this);
        }
        else
        {
            var oreRefineryPower = await PowerCmd.Apply<OreRefineryPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
            if (oreRefineryPower != null)
            {
                oreRefineryPower.CurrentBonus = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
                oreRefineryPower.IsUpgraded = IsUpgraded;
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Bonus"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}