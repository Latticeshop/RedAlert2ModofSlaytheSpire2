using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Allies.Cards;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 提前倒矿能力 - debuff
/// 效果：本回合矿车收益为80%
/// 回合结束时自动移除
/// </summary>
public sealed class EarlyMiningPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/early_mining_power.png";
    public new AbstractModel OriginModel => ModelDb.Card<EarlyMining>();

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MiningMultiplierPercent", 80) // 矿车收益倍率：80%
    };

    /// <summary>
    /// 获取矿车收益倍率
    /// </summary>
    public float GetMiningMultiplier()
    {
        return DynamicVars["MiningMultiplierPercent"].IntValue / 100f;
    }

    /// <summary>
    /// 回合开始时移除此debuff（debuff仅维持一回合）
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner?.Side)
        {
            await PowerCmd.Remove(this);
            GD.Print("[EarlyMiningPower] 回合开始，移除提前倒矿debuff");
        }
    }
}