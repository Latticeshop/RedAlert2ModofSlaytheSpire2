using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

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

    private int _currentDiscount;
    public int CurrentDiscount
    {
        get => _currentDiscount;
        set
        {
            _currentDiscount = value;
            DynamicVars["Discount"].BaseValue = value;
        }
    }

    public IndustrialPlantPower()
    {
        GD.Print($"[IndustrialPlantPower] 构造函数被调用");
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new IntVar("Discount", 0) };

    public float GetPriceMultiplier()
    {
        return 1.0f - _currentDiscount / 100.0f;
    }
}