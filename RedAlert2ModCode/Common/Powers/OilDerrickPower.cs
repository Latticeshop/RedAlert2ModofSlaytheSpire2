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

public class OilDerrickPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.OilDerrickPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDollarPerTurn { get; set; } = (int)Values.DollarValue;

    public bool IsUpgraded { get; set; } = false;

    public OilDerrickPower()
    {
        GD.Print($"[OilDerrickPower] 构造函数被调用 - DollarPerTurn={CurrentDollarPerTurn}");
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("DollarPerTurn", CurrentDollarPerTurn);
            return locString;
        }
    }

    public static async Task<OilDerrickPower?> ApplyOilDerrick(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[OilDerrickPower] ApplyOilDerrick 被调用 - IsUpgraded={isUpgraded}");

        var newPower = await PowerCmd.Apply<OilDerrickPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (newPower != null)
        {
            newPower.CurrentDollarPerTurn = (int)Values.DollarValue + (isUpgraded ? (int)Values.DollarValueUpgraded : 0);
            newPower.IsUpgraded = isUpgraded;
            GD.Print($"[OilDerrickPower] 创建成功 - DollarPerTurn={newPower.CurrentDollarPerTurn}, IsUpgraded={newPower.IsUpgraded}");
        }
        return newPower;
    }

    public static async Task ApplyOilDerricks(Creature owner, int count, bool isUpgraded = false)
    {
        GD.Print($"[OilDerrickPower] ApplyOilDerricks 被调用 - Count={count}, IsUpgraded={isUpgraded}");

        int targetDollarPerTurn = (int)Values.DollarValue + (isUpgraded ? (int)Values.DollarValueUpgraded : 0);

        var existingOilDerrickPower = owner.Powers
            .OfType<OilDerrickPower>()
            .FirstOrDefault(p => p.CurrentDollarPerTurn == targetDollarPerTurn);

        if (existingOilDerrickPower != null)
        {
            GD.Print($"[OilDerrickPower] 发现相同资金产出的能力(${targetDollarPerTurn})，增加层数 - 当前层数={existingOilDerrickPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingOilDerrickPower, (decimal)count, owner, null);
            GD.Print($"[OilDerrickPower] 层数增加完成 - 新层数={existingOilDerrickPower.Amount}");
        }
        else
        {
            GD.Print($"[OilDerrickPower] 未发现相同资金产出的能力(${targetDollarPerTurn})，创建新能力 - 初始层数={count}");
            var newPower = await PowerCmd.Apply<OilDerrickPower>(new ThrowingPlayerChoiceContext(), owner, (decimal)count, owner, null);
            if (newPower != null)
            {
                newPower.CurrentDollarPerTurn = targetDollarPerTurn;
                newPower.IsUpgraded = isUpgraded;
                GD.Print($"[OilDerrickPower] 创建成功 - DollarPerTurn={newPower.CurrentDollarPerTurn}, IsUpgraded={newPower.IsUpgraded}, Amount={newPower.Amount}");
            }
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        int stacks = (int)base.Amount;
        GD.Print($"[OilDerrickPower] 回合开始触发 - 层数={stacks}, DollarPerTurn={CurrentDollarPerTurn}");

        int totalDollar = CurrentDollarPerTurn * stacks;
        GD.Print($"[OilDerrickPower] 计算总资金 - {CurrentDollarPerTurn} x {stacks} = {totalDollar}");

        var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(totalDollar);
            GD.Print($"[OilDerrickPower] 油井产出资金 {totalDollar}，当前总资金 {dollarPower.DollarValue}");
        }
        else
        {
            GD.PrintErr($"[OilDerrickPower] 未找到 DollarPower，资金无法添加");
        }
    }
}