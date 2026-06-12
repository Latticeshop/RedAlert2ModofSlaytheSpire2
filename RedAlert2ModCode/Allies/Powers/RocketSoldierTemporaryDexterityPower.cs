using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using Godot;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 火箭飞行兵临时敏捷能力
/// </summary>
public sealed class RocketSoldierTemporaryDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<RocketSoldier>();

    /// <summary>
    /// 使用火箭飞行兵卡牌的图标
    /// 注意：Icon属性使用的是PackedIconPath，所以必须重写这个属性
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/jjeticon.png";
}