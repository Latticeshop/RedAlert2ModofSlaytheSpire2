using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 减速 - 独立debuff能力
/// 每打出一张攻击牌，受到的伤害增加10%（回合开始时重置）。
/// 与原版迟缓的区别：仅攻击牌触发（原版为所有牌）。
/// </summary>
public sealed class DecelerationPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Deceleration.png";

    public override int DisplayAmount => base.DynamicVars["SlowAmount"].IntValue * 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("SlowAmount", 0m)
    };

    /// <summary>
    /// 每打出一张攻击牌时：增加减速层数（伤害+10%）
    /// </summary>
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == null) return Task.CompletedTask;
        if (cardPlay.Card.Type != CardType.Attack) return Task.CompletedTask;

        base.DynamicVars["SlowAmount"].BaseValue++;
        InvokeDisplayAmountChanged();
        GD.Print($"[DecelerationPower] 攻击牌触发减速，当前层数: {base.DynamicVars["SlowAmount"].BaseValue}（伤害+{base.DynamicVars["SlowAmount"].BaseValue * 10}%）");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 减速效果：增加受到的攻击伤害（每层+10%）
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != base.Owner)
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;

        decimal slowAmount = base.DynamicVars["SlowAmount"].BaseValue;
        if (slowAmount <= 0)
            return 1m;

        decimal multiplier = 1m + 0.1m * slowAmount;
        GD.Print($"[DecelerationPower] 减速增伤: {slowAmount}层 → ×{multiplier}");
        return multiplier;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    /// <summary>敌人回合开始时移除减速debuff</summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (base.Owner == null) return;
        if (base.DynamicVars["SlowAmount"].BaseValue <= 0) return;

        GD.Print("[DecelerationPower] 敌人回合开始，移除减速debuff");
        await PowerCmd.Remove(this);
    }
}
