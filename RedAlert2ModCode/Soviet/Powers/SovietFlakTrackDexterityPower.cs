using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 防空履带车临时敏捷能力
/// </summary>
public sealed class SovietFlakTrackDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<FlakTrack>();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/htkicon.png";
}