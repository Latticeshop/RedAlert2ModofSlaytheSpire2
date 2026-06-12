using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 盟军基地车能力 - 用于显示能力图标
/// </summary>
public sealed class AlliedMCVPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用盟军基地车卡牌的图标
    /// 注意：Icon属性使用的是PackedIconPath，所以必须重写这个属性
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";
}