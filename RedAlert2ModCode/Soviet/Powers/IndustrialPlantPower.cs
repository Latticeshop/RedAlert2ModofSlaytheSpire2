using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class IndustrialPlantPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private bool _isUpgraded;
    public bool IsUpgraded
    {
        get => _isUpgraded;
        set => _isUpgraded = value;
    }

    public IndustrialPlantPower()
    {
        GD.Print($"[IndustrialPlantPower] 构造函数被调用");
    }

    public override LocString Description
    {
        get
        {
            var values = SovietCardValues.IndustrialPlant;
            int discount = IsUpgraded ? (int)(values.MagicNumber + values.MagicNumberUpgraded) : (int)values.MagicNumber;
            var locString = new LocString("powers", base.Id.Entry + ".smartDescription");
            locString.Add("Discount", discount);
            return locString;
        }
    }

    public float GetPriceMultiplier()
    {
        var values = SovietCardValues.IndustrialPlant;
        int discount = IsUpgraded ? (int)(values.MagicNumber + values.MagicNumberUpgraded) : (int)values.MagicNumber;
        return 1.0f - discount / 100.0f;
    }
}