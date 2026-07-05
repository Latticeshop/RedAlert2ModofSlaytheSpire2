using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class AlliedAirForceCommandPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new LocString("powers", Id.Entry + ".title");

    public override LocString Description => new LocString("powers", Id.Entry + ".description");

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/heliicon.png";
}