using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using Godot;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 夜莺直升机临时敏捷能力
/// </summary>
public sealed class NightHawkTemporaryDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<NightHawkChopper>();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/shadicon.png";
}