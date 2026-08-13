using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 控制中心能力 - 标记盟军重工已解锁遥控坦克
/// 可叠层（Counter），无实际效果，仅给盟军重工检查能力来展示遥控坦克选项。
/// </summary>
public sealed class ControlCenterPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用控制中心卡牌的图标（注意：PowerModel.Icon 非 virtual，需配合 PowerIconPatch）
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/rbccicon.png";

    public override LocString Title => new LocString("powers", "CONTROL_CENTER_POWER.title");

    public override LocString Description => new LocString("powers", "CONTROL_CENTER_POWER.description");
}
