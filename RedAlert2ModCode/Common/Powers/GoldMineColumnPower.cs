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
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class GoldMineColumnPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.GoldMineColumnPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

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

    public void IncreasePerTurn()
    {
        ReservePerTurn += (int)Values.Stars;
        GD.Print($"[GoldMineColumnPower] 每回合产量增加 {Values.Stars}，当前每回合产量: {ReservePerTurn}");
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        var goldMinePower = Owner.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(ReservePerTurn);
            GD.Print($"[GoldMineColumnPower] 回合开始，为黄金矿增加储备 {ReservePerTurn}");
        }
    }
}