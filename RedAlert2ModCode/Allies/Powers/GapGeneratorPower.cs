using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 裂缝产生器能力 - 可叠层，每层能力获得 1 层黑幕。
/// 自己回合结束时检查是否有能量（>0 即生效）：有则移除旧黑幕并施加新黑幕（无缝衔接），
/// 没有能量则中断（旧黑幕失效且不施新）。
/// </summary>
public sealed class GapGeneratorPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.GapGeneratorPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/gapicon.png";

    public override LocString Title => new LocString("powers", "GAP_GENERATOR_POWER.title");

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", "GAP_GENERATOR_POWER.description");
            locString.Add("StrengthLoss", Values.MagicNumber);
            locString.Add(new EnergyVar(1));
            return locString;
        }
    }

    /// <summary>
    /// 玩家回合结束：检查能量，>0 即刷新黑幕（每层能力 = 1 层黑幕）。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;
        if (Owner == null || !participants.Contains(Owner))
            return;

        int stacks = (int)base.Amount;
        int energy = Owner.Player?.PlayerCombatState?.Energy ?? 0;

        if (energy <= 0)
        {
            // 没有能量：旧黑幕失效（ApplyBlackCurtain 内部会移除），不施新 → 中断
            GD.Print($"[GapGeneratorPower] 回合结束没有能量，裂缝产生器中断（{stacks} 层）");
            await BlackCurtainPower.ApplyBlackCurtain(Owner, 0);
            return;
        }

        // 有能量：旧黑幕失效 + 新黑幕无缝衔接（每层能力 1 层黑幕）
        await BlackCurtainPower.ApplyBlackCurtain(Owner, stacks);
        GD.Print($"[GapGeneratorPower] 回合结束有能量，刷新黑幕 {stacks} 层");
    }
}
