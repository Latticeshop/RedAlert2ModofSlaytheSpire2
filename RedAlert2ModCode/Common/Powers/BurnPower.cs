using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 灼烧 - 每回合按百分比扣敌怪血量的debuff能力
/// 移植自海克斯符文mod的HextechBurnPower
/// </summary>
public sealed class BurnPower : PowerModel
{
    private const decimal StackDecayPercent = 0.1m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.None;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/BurnPower.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int stacks = (int)Amount;
            int percentHpLoss = Owner != null && Owner.IsAlive
                ? Math.Max(1, (int)Math.Floor(Owner.CurrentHp * stacks / 100m))
                : stacks;
            int hpLoss = Math.Max(stacks, percentHpLoss);
            locString.Add("SmartDamage", hpLoss);
            return locString;
        }
    }

    /// <summary>
    /// 敌人回合开始时结算灼烧（不可格挡）
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner == null || Owner.Side != CombatSide.Enemy || side != Owner.Side)
            return;

        await ResolveBurn(new ThrowingPlayerChoiceContext(), blockable: false);
    }

    /// <summary>
    /// 玩家回合结束时结算灼烧（可格挡）
    /// </summary>
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner == null || Owner.Side != CombatSide.Player || side != Owner.Side)
            return;

        await ResolveBurn(choiceContext, blockable: true);
    }

    /// <summary>
    /// 结算灼烧伤害和衰减
    /// </summary>
    private async Task ResolveBurn(PlayerChoiceContext choiceContext, bool blockable)
    {
        if (Amount <= 0 || Owner == null || !Owner.IsAlive)
            return;

        int stacks = (int)Amount;

        // 伤害 = max(层数, 当前HP * 层数%)
        int percentHpLoss = Math.Max(1, (int)Math.Floor(Owner.CurrentHp * stacks / 100m));
        int hpLoss = Math.Max(stacks, percentHpLoss);

        // 衰减 = 层数 × 10%（最少减1层）
        int stackLoss = Math.Max(1, (int)Math.Ceiling(stacks * StackDecayPercent));

        Flash();

        ValueProp valueProps = ValueProp.Unpowered;
        if (!blockable)
            valueProps |= ValueProp.Unblockable;

        await CreatureCmd.Damage(choiceContext, Owner, hpLoss, valueProps, null, null);

        if (Owner.IsAlive)
        {
            await PowerCmd.Apply<BurnPower>(choiceContext, Owner, -stackLoss, Owner, null);
        }
    }
}
