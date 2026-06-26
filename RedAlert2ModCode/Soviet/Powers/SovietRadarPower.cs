using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class SovietRadarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override LocString Title => new LocString("powers", Id.Entry + ".title");
    
    public override LocString Description => new LocString("powers", Id.Entry + ".description");
}