using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Godot;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class SpyPlaneTemporaryDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<RedAlert2ModCode.Soviet.Cards.SpyPlane>();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/spypicon.png";
}