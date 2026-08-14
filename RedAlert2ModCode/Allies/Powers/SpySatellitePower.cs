using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 间谍卫星能力 - 免疫[gold]虚弱[/gold]与[gold]脆弱[/gold]
/// 可叠层（Counter），但叠层无叠加效果：只要拥有该能力即永久免疫。
/// 未升级只免疫虚弱；升级后同时免疫虚弱与脆弱。升级/未升级独立叠加（Instanced）。
/// 通过 TryModifyPowerAmountReceived 阻止获得虚弱/脆弱，获得能力时清除自身已有的虚弱/脆弱。
/// </summary>
public sealed class SpySatellitePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>升级/未升级分别独立实例与叠层</summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public bool IsUpgraded { get; set; }

    /// <summary>
    /// 使用间谍卫星卡牌的图标（注意：PowerModel.Icon 非 virtual，需配合 PowerIconPatch）
    /// </summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/asaticon.png";

    public override LocString Title => new LocString("powers", "SPY_SATELLITE_POWER.title");

    public override LocString Description => new LocString("powers",
        IsUpgraded ? "SPY_SATELLITE_POWER_UPGRADED.description" : "SPY_SATELLITE_POWER.description");

    /// <summary>
    /// 获得间谍卫星能力：按升级状态独立叠层（未升级与升级分别叠加，互不合并）。
    /// </summary>
    public static async Task<SpySatellitePower?> ApplySpySatellite(Creature owner, bool isUpgraded)
    {
        var existingPower = owner.Powers
            .OfType<SpySatellitePower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);
        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[SpySatellitePower] 叠加到已存在的间谍卫星能力，升级={isUpgraded}, 层数={existingPower.Amount}");
            return existingPower;
        }

        var power = await PowerCmd.Apply<SpySatellitePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.IsUpgraded = isUpgraded;
            GD.Print($"[SpySatellitePower] 创建成功 - IsUpgraded={isUpgraded}, Amount={power.Amount}");
        }
        return power;
    }

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
        // 未升级只免疫虚弱；升级同时免疫虚弱与脆弱
        if (canonicalPower is not WeakPower && !(IsUpgraded && canonicalPower is FrailPower))
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
        // 未升级只清除虚弱；升级同时清除虚弱与脆弱
        var existing = Owner.Powers
            .Where(p => p is WeakPower || (IsUpgraded && p is FrailPower))
            .ToList();
        foreach (var power in existing)
        {
            await PowerCmd.Remove(power);
            GD.Print($"[SpySatellitePower] 清除已有 {power.GetType().Name}");
        }
    }
}
