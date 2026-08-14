using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 黑幕能力 - 全体敌人失去力量（可叠层，叠层效果为失去力量叠加，如 3 层黑幕 = 全体敌人降低 3 点力量）。
/// 由裂缝产生器在回合结束时刷新：旧黑幕失效、新黑幕无缝衔接；若回合结束没有能量则中断。
/// </summary>
public sealed class BlackCurtainPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.BlackCurtainPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/black_curtain.png";

    public override LocString Title => new LocString("powers", "BLACK_CURTAIN_POWER.title");

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", "BLACK_CURTAIN_POWER.description");
            locString.Add("StrengthLoss", Values.MagicNumber);
            return locString;
        }
    }

    /// <summary>
    /// 黑幕结束时，对全体敌人的力量效果也结束（恢复被降低的力量）。
    /// </summary>
    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);

        if (oldOwner?.CombatState == null || base.Amount <= 0) return;
        var enemies = oldOwner.CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, base.Amount, oldOwner, null);
        }
        GD.Print($"[BlackCurtainPower] 黑幕结束，全体敌人恢复 {base.Amount} 点力量");
    }

    /// <summary>
    /// 施加黑幕：先移除旧黑幕（旧黑幕失效，AfterRemoved 恢复已降低的力量），
    /// 再给全体敌人降低 amount 点力量，并叠加 amount 层黑幕。
    /// </summary>
    public static async Task ApplyBlackCurtain(Creature owner, int amount)
    {
        if (owner == null) return;

        // 旧黑幕失效
        var existing = owner.Powers.OfType<BlackCurtainPower>().FirstOrDefault();
        if (existing != null)
        {
            await PowerCmd.Remove(existing);
        }
        if (amount <= 0) return;

        // 全体敌人降低力量
        var enemies = owner.CombatState?.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies == null || enemies.Count == 0) return;
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, -amount, owner, null);
        }

        // 新黑幕
        await PowerCmd.Apply<BlackCurtainPower>(new ThrowingPlayerChoiceContext(), owner, amount, owner, null);
        GD.Print($"[BlackCurtainPower] 全体敌人降低 {amount} 点力量，黑幕 {amount} 层");
    }
}
