using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 箱子敏捷能力 - 速度箱子使用
/// </summary>
public sealed class CrateTemporaryDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SpeedCrate>();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";
}
