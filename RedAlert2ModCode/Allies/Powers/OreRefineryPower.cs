using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Cards;

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

    public OreRefineryPower()
    {
        GD.Print($"[OreRefineryPower] 构造函数被调用");
    }

    public override LocString Description
    {
        get
        {
            var values = AlliesCardValues.OreRefinery;
            int bonus = IsUpgraded ? (int)(values.MagicNumber + values.MagicNumberUpgraded) : (int)values.MagicNumber;
            var locString = new LocString("powers", base.Id.Entry + ".smartDescription");
            locString.Add("Bonus", bonus);
            return locString;
        }
    }

    public float GetOreMultiplier()
    {
        var values = AlliesCardValues.OreRefinery;
        int bonus = IsUpgraded ? (int)(values.MagicNumber + values.MagicNumberUpgraded) : (int)values.MagicNumber;
        return 1.0f + bonus / 100.0f;
    }
}