using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class IndustrialPlantPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".smartDescription");
            locString.Add("Discount", (int)base.Amount);
            return locString;
        }
    }

    public float GetPriceMultiplier()
    {
        return 1.0f - (int)base.Amount / 100.0f;
    }
}