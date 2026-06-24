using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 苏军基地车能力 - 用于显示能力图标
/// </summary>
public sealed class SovietMCVPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用苏军基地车卡牌的图标
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";
    
    /// <summary>
    /// 能力标题（本地化）
    /// </summary>
    public override LocString Title => new LocString("powers", Id.Entry + ".title");
    
    /// <summary>
    /// 能力描述（本地化）
    /// </summary>
    public override LocString Description => new LocString("powers", Id.Entry + ".description");
}
