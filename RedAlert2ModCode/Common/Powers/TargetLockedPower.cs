using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace RedAlert2ModCode.Common.Powers;

public class TargetLockedPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/target_locked.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            return locString;
        }
    }
}