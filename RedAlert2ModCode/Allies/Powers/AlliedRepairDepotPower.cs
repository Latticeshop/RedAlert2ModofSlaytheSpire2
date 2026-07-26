using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 盟军维修厂能力 - 无效果，用于标识已部署维修厂
/// 提供给"出售"卡牌半价出售，以及重工检测后添加MCV选项
/// </summary>
public sealed class AlliedRepairDepotPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new LocString("powers", Id.Entry + ".title");

    public override LocString Description => new LocString("powers", Id.Entry + ".description");

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/fixicon.png";
}
