using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class OreRefineryPower : PowerModel
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

    private int _currentBonus;
    public int CurrentBonus
    {
        get => _currentBonus;
        set
        {
            _currentBonus = value;
            DynamicVars["Bonus"].BaseValue = value;
        }
    }

    public OreRefineryPower()
    {
        GD.Print($"[OreRefineryPower] 构造函数被调用");
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new IntVar("Bonus", 0) };

    public float GetOreMultiplier()
    {
        return 1.0f + _currentBonus / 100.0f;
    }
}