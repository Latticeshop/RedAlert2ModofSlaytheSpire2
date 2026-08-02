using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using STS2RitsuLib.Combat.HealthBars;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class ErasingPower : PowerModel, IHealthBarForecastSource
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/clegicon.png";

    /// <summary>
    /// 血条伤害预览的蓝色（超时空军团兵主题色）。
    /// 同时用作覆盖层染色（OverlaySelfModulate 为 null 时回退到此色）
    /// 和致命时 HP 文字的主题色。
    /// </summary>
    private static readonly Color ForecastColor = new("42A5F5");

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Amount", (int)Amount);
            return locString;
        }
    }

    /// <summary>
    /// 血条伤害预览：以蓝色块显示当前抹除层数对应的血量进度。
    /// 从右侧（当前HP边缘）向左延伸，类似中毒的绿色指示器。
    /// 当层数达到或超过当前HP时，蓝色块填满血条，表示即将/已经斩杀。
    /// </summary>
    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        // 仅对自身拥有者显示
        if (context.Creature != Owner) yield break;
        if (Owner == null || !Owner.IsAlive) yield break;

        int eraseStacks = (int)Amount;
        if (eraseStacks <= 0) yield break;

        int currentHp = (int)Owner.CurrentHp;
        if (currentHp <= 0) yield break;

        // 显示已积累的抹除进度，最多填满当前血条
        int displayAmount = Math.Min(eraseStacks, currentHp);

        // 蓝色，从左侧延伸（像灾厄），敌人在回合开始时检查斩杀
        yield return new HealthBarForecastSegment(
            displayAmount,
            ForecastColor,
            HealthBarForecastGrowthDirection.FromLeft,
            HealthBarForecastOrder.ForSideTurnStart(Owner, Owner.Side)
        );
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;

        await CheckErase(choiceContext, combatState);
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        TransparencyHelper.SetTransparency(Owner);
        await CheckErase(new ThrowingPlayerChoiceContext(), null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        TransparencyHelper.ResetTransparency(oldOwner);
    }

    private async Task CheckErase(PlayerChoiceContext choiceContext, ICombatState? combatState)
    {
        int eraseStacks = (int)Amount;
        if (eraseStacks <= 0) return;

        if (Owner == null || !Owner.IsAlive) return;

        if (eraseStacks > (int)Owner.CurrentHp)
        {
            Flash();
            UnitVoiceHelper.PlayUnitVoice("ChronoLegionnaireKill", "Allied");

            await CreatureCmd.Kill(Owner);
        }
    }
}
