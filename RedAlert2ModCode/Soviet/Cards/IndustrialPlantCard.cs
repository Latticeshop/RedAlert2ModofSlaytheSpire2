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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class IndustrialPlantCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.IndustrialPlant;

    public IndustrialPlantCard() : base((int)Values.Cost, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/indpicon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Unit.CreateHoverTip()
    };

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("Discount", Values.MagicNumber)
    };

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
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
        // 播放建筑释放音效
        BuildingSoundHelper.PlayBuildingPlaceSound();
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        // 扣除建筑资金
        var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[IndustrialPlantCard] 扣除建筑资金 {Values.DollarValue}");
        }
        
        var existingPower = Owner.Creature.Powers.OfType<IndustrialPlantPower>().FirstOrDefault();
        if (existingPower != null)
        {
            int newDiscount = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
            if (newDiscount > existingPower.CurrentDiscount)
            {
                existingPower.CurrentDiscount = newDiscount;
                existingPower.IsUpgraded = IsUpgraded;
            }
            await PowerCmd.ModifyAmount(ctx, existingPower, 1m, Owner.Creature, this);
        }
        else
        {
            var industrialPlantPower = await PowerCmd.Apply<IndustrialPlantPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
            if (industrialPlantPower != null)
            {
                industrialPlantPower.CurrentDiscount = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
                industrialPlantPower.IsUpgraded = IsUpgraded;
            }
        }
        
        await Common.Powers.MassProductionPower.RecalculateAllTrainingQueuePrices(Owner.Creature);
	    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Discount"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}