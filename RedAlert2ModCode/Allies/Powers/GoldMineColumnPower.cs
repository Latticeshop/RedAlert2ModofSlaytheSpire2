using System.Collections.Generic;
using System.Linq;
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
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 黄金矿柱能力 - 每回合自动增加黄金矿储备
/// 与黄金矿能力配合使用，负责被动增加储备
/// </summary>
public class GoldMineColumnPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.GoldMineColumnPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>
    /// 每回合增加的储备
    /// </summary>
    public int ReservePerTurn { get; set; } = (int)Values.Stars;

    public GoldMineColumnPower()
    {
        GD.Print($"[GoldMineColumnPower] 构造函数被调用 - PerTurn={ReservePerTurn}");
    }

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/gold_mine_column_power.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("PerTurn", ReservePerTurn);
            return locString;
        }
    }

    /// <summary>
    /// 增加每回合产量（叠加效果）
    /// </summary>
    public void IncreasePerTurn()
    {
        ReservePerTurn += (int)Values.Stars;
        GD.Print($"[GoldMineColumnPower] 每回合产量增加 {Values.Stars}，当前每回合产量: {ReservePerTurn}");
    }

    /// <summary>
    /// 回合开始时增加黄金矿储备
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        // 找到黄金矿能力并增加其储备
        var goldMinePower = Owner.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(ReservePerTurn);
            GD.Print($"[GoldMineColumnPower] 回合开始，为黄金矿增加储备 {ReservePerTurn}");
        }
    }
}