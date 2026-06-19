using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 作战实验室能力
/// 用于标记已解锁高级科技，无实际效果
/// </summary>
public sealed class BattleLabPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>
    /// 本地化描述，显示"已解锁高级科技"
    /// </summary>
    public override LocString Description
    {
        get
        {
            return new LocString("powers", "BATTLE_LAB_POWER.description");
        }
    }
}
