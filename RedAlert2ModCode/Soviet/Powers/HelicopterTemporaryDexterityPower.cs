using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 武装直升机临时敏捷能力（参考夜莺直升机临时敏捷）
/// </summary>
public sealed class HelicopterTemporaryDexterityPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<HelicopterFlight>();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/schpicon.png";
}
