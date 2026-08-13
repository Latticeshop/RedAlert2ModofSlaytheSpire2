using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 间谍卫星能力 - 免疫[gold]虚弱[/gold]与[gold]脆弱[/gold]
/// 可叠层（Counter），但叠层无叠加效果：只要拥有该能力即永久免疫。
/// 通过 TryModifyPowerAmountReceived 阻止获得虚弱/脆弱，获得能力时清除自身已有的虚弱/脆弱。
/// </summary>
public sealed class SpySatellitePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用间谍卫星卡牌的图标（注意：PowerModel.Icon 非 virtual，需配合 PowerIconPatch）
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/asaticon.png";

    public override LocString Title => new LocString("powers", "SPY_SATELLITE_POWER.title");

    public override LocString Description => new LocString("powers", "SPY_SATELLITE_POWER.description");

    /// <summary>
    /// 阻止自身获得原版虚弱（Weak，伤害减少）与脆弱（Frail，格挡减少）。
    /// </summary>
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? _, out decimal modifiedAmount)
    {
        if (target != base.Owner)
        {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower is not WeakPower && canonicalPower is not FrailPower)
        {
            modifiedAmount = amount;
            return false;
        }

        modifiedAmount = default;
        return true;
    }

    /// <summary>
    /// 获得能力时清除自身已有的虚弱/脆弱。
    /// </summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);

        if (Owner == null) return;
        var existing = Owner.Powers.Where(p => p is WeakPower or FrailPower).ToList();
        foreach (var power in existing)
        {
            await PowerCmd.Remove(power);
            GD.Print($"[SpySatellitePower] 清除已有 {power.GetType().Name}");
        }
    }
}
