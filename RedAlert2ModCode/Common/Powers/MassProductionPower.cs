using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class MassProductionPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.MassProductionPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为 Instanced 允许相同类型但不同数值的能力独立存在
    /// 参考 IronCurtainPower 和 WeatherControllerPower 的实现
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public bool IsUpgraded { get; set; } = false;

    /// <summary>
    /// 当前每层减少的价格（升级后150，未升级100）
    /// </summary>
    public int PriceReductionPerStack => IsUpgraded ? (int)Values.StarsUpgraded : (int)Values.Stars;

    /// <summary>
    /// 总价格减少量
    /// </summary>
    public int TotalPriceReduction => (int)Amount * PriceReductionPerStack;

    public override LocString Description
    {
        get
        {
            string key = base.Id.Entry + ".description";
            if (IsUpgraded)
            {
                key += "_upgraded";
            }
            
            var locString = new LocString("powers", key);
            locString.Add("Reduction", TotalPriceReduction);
            locString.Add("ReductionPerStack", PriceReductionPerStack);
            return locString;
        }
    }

    public static async Task<MassProductionPower?> ApplyMassProduction(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[MassProductionPower] ApplyMassProduction 被调用 - isUpgraded={isUpgraded}");

        // 计算当前要添加的能力数值（未升级100，升级150）
        int targetReductionPerStack = isUpgraded ? (int)Values.StarsUpgraded : (int)Values.Stars;
        
        GD.Print($"[MassProductionPower] 目标数值={targetReductionPerStack}, 当前拥有的大生产能力数量={owner.Powers.OfType<MassProductionPower>().Count()}");

        // 按数值查找现有能力：若存在数值相同的则叠加层数，否则创建新的能力
        var existingPower = owner.Powers
            .OfType<MassProductionPower>()
            .FirstOrDefault(p => p.PriceReductionPerStack == targetReductionPerStack);

        if (existingPower != null)
        {
            // 存在相同数值的能力，增加层数
            GD.Print($"[MassProductionPower] 发现相同数值的能力，增加层数 - 当前层数={existingPower.Amount}, 每层减少={existingPower.PriceReductionPerStack}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[MassProductionPower] 层数增加完成 - 新层数={existingPower.Amount}");
        }
        else
        {
            // 不存在相同数值的能力，创建新能力
            GD.Print($"[MassProductionPower] 未发现相同数值的能力，创建新能力 - 每层减少={targetReductionPerStack}");
            var newPower = await PowerCmd.Apply<MassProductionPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
            if (newPower != null)
            {
                newPower.IsUpgraded = isUpgraded;
                GD.Print($"[MassProductionPower] 创建成功 - 层数={newPower.Amount}, IsUpgraded={newPower.IsUpgraded}, 每层减少={newPower.PriceReductionPerStack}");
            }
        }
        
        await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
        
        await RecalculateAllTrainingQueuePrices(owner);
        
        GD.Print($"[MassProductionPower] 重新计算所有生产序列价格完成");
        
        // 返回最新的对应数值的能力
        return owner.Powers.OfType<MassProductionPower>().FirstOrDefault(p => p.PriceReductionPerStack == targetReductionPerStack);
    }

    public static async Task RecalculateAllTrainingQueuePrices(Creature owner)
    {
        GD.Print($"[MassProductionPower] RecalculateAllTrainingQueuePrices 被调用");
        await UnitPriceCalculator.RecalculateAllTrainingQueuePrices(owner);
    }

    private static int GetOriginalUnitPrice(TrainingQueuePower trainingPower)
    {
        if (trainingPower.OriginalUnitPrice > 0)
        {
            return trainingPower.OriginalUnitPrice;
        }
        
        var massProductionPower = trainingPower.Owner?.Powers.OfType<MassProductionPower>().FirstOrDefault();
        
        if (massProductionPower != null)
        {
            return trainingPower.UnitPrice + massProductionPower.TotalPriceReduction;
        }
        
        return trainingPower.UnitPrice;
    }

    public static int CalculateUnitPrice(Creature owner, int originalPrice)
    {
        return UnitPriceCalculator.ApplyMassProductionReduction(owner, originalPrice);
    }
}