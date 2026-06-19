using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 目标锁定能力 - 标记敌人为被攻击目标
/// 被标记的敌人会受到黄蜂舰载机的攻击
/// </summary>
public class TargetLockedPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用目标锁定图标
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/target_locked.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            return locString;
        }
    }
}
