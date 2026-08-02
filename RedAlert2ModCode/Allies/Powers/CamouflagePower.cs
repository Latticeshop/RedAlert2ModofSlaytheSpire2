using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 伪装能力 - 可见buff，内置无实体效果
/// 本回合没有造成任何伤害时，获得伪装（具有无实体效果）
/// 造成伤害时移除伪装
/// 回合开始时移除伪装
/// 拥有伪装时角色获得透明特效（与抹除特效一致），移除时恢复
/// </summary>
public sealed class CamouflagePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<IntangiblePower>()
    };

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            return locString;
        }
    }

    /// <summary>
    /// 能力应用时：设置透明特效（与抹除特效一致）
    /// </summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        TransparencyHelper.SetTransparency(Owner);
        GD.Print("[CamouflagePower] 伪装应用，设置透明特效");
    }

    /// <summary>
    /// 能力移除时：恢复不透明
    /// </summary>
    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        TransparencyHelper.ResetTransparency(oldOwner);
        GD.Print("[CamouflagePower] 伪装移除，恢复不透明");
    }

    /// <summary>
    /// 无实体效果：将生命减少限制为1
    /// </summary>
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!CombatManager.Instance.IsInProgress)
            return amount;
        if (target != base.Owner)
            return amount;
        if (amount < 1m)
            return amount;
        return 1m;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 无实体效果：伤害上限为1
    /// </summary>
    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner)
            return decimal.MaxValue;
        return 1m;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 玩家回合开始时：移除自身
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        GD.Print("[CamouflagePower] 玩家回合开始，移除伪装");
        await PowerCmd.Remove(this);
    }
}
