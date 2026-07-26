using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Common.Utils;

public static class UnitPriceCalculator
{
    public static int CalculateFinalUnitPrice(Creature owner, int originalPrice, int trainingQueueStacks = 1)
    {
        // 新公式：单位最终价格 = max(0, 原始价格 × (1 - 工业工厂折扣) - 大生产层数 × 大生产数值)
        int industrialPlantPrice = ApplyIndustrialPlantDiscount(owner, originalPrice);
        int finalPrice = ApplyMassProductionReduction(owner, industrialPlantPrice);
        
        GD.Print($"[UnitPriceCalculator] 计算完成 - 原始价格={originalPrice}, 工业工厂后={industrialPlantPrice}, 最终价格={finalPrice}");
        
        return finalPrice;
    }

    public static int ApplyMassProductionReduction(Creature owner, int price)
    {
        var massProductionPowers = owner.Powers.OfType<MassProductionPower>().ToList();
        if (massProductionPowers.Count == 0)
        {
            return price;
        }

        int totalReduction = 0;
        
        // 遍历所有大生产能力，分别计算降价并累加
        foreach (var power in massProductionPowers)
        {
            int stacks = (int)power.Amount;
            int reductionPerStack = power.IsUpgraded 
                ? (int)CommonPowerValues.MassProductionPower.StarsUpgraded 
                : (int)CommonPowerValues.MassProductionPower.Stars;
            
            int powerReduction = stacks * reductionPerStack;
            totalReduction += powerReduction;
            
            GD.Print($"[UnitPriceCalculator] 大生产{(power.IsUpgraded ? "(升级)" : "")} - 层数={stacks}, 每层减少={reductionPerStack}, 该能力总减少={powerReduction}");
        }

        GD.Print($"[UnitPriceCalculator] 大生产总减少={totalReduction}");
        
        return Mathf.Max(0, price - totalReduction);
    }

    public static int ApplyIndustrialPlantDiscount(Creature owner, int price)
    {
        var industrialPlantPower = owner.Powers.OfType<IndustrialPlantPower>().FirstOrDefault();
        if (industrialPlantPower == null)
        {
            return price;
        }

        float multiplier = industrialPlantPower.GetPriceMultiplier();
        int discountedPrice = Mathf.FloorToInt(price * multiplier);
        
        GD.Print($"[UnitPriceCalculator] 工业工厂折扣 - 原价={price}, 折扣={industrialPlantPower.CurrentDiscount}%, 折扣后={discountedPrice}");
        
        return discountedPrice;
    }

    public static async System.Threading.Tasks.Task RecalculateAllTrainingQueuePrices(Creature owner)
    {
        GD.Print($"[UnitPriceCalculator] RecalculateAllTrainingQueuePrices 被调用");

        var trainingQueuePowers = owner.Powers.OfType<TrainingQueuePower>().ToList();

        foreach (var trainingPower in trainingQueuePowers)
        {
            int originalPrice = trainingPower.OriginalUnitPrice;

            if (originalPrice == 0)
            {
                originalPrice = trainingPower.UnitPrice;
            }

            int finalPrice = CalculateFinalUnitPrice(owner, originalPrice, (int)trainingPower.Amount);
            trainingPower.UnitPrice = finalPrice;

            GD.Print($"[UnitPriceCalculator] 生产序列 {trainingPower.UnitName}: 原始价格={originalPrice}, 最终价格={finalPrice}");
        }
    }
}