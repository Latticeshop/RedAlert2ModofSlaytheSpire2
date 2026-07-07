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
        int massProductionReducedPrice = ApplyMassProductionReduction(owner, originalPrice, trainingQueueStacks);
        int finalPrice = ApplyIndustrialPlantDiscount(owner, massProductionReducedPrice);
        
        GD.Print($"[UnitPriceCalculator] 计算完成 - 原始价格={originalPrice}, 生产序列层数={trainingQueueStacks}, 大生产后={massProductionReducedPrice}, 最终价格={finalPrice}");
        
        return finalPrice;
    }

    public static int ApplyMassProductionReduction(Creature owner, int originalPrice, int trainingQueueStacks = 1)
    {
        var massProductionPower = owner.Powers.OfType<MassProductionPower>().FirstOrDefault();
        if (massProductionPower == null)
        {
            return originalPrice;
        }

        int totalReduction;
        int massProductionStacks = (int)massProductionPower.Amount;
        int massProductionValue = (int)CommonPowerValues.MassProductionPower.Stars;

        if (massProductionPower.IsUpgraded)
        {
            totalReduction = massProductionStacks * trainingQueueStacks * massProductionValue;
            GD.Print($"[UnitPriceCalculator] 大生产(升级) - 大生产层数={massProductionStacks}, 生产序列层数={trainingQueueStacks}, 每层减少={massProductionValue}, 总减少={totalReduction}");
        }
        else
        {
            totalReduction = massProductionStacks * massProductionValue;
            GD.Print($"[UnitPriceCalculator] 大生产(未升级) - 大生产层数={massProductionStacks}, 每层减少={massProductionValue}, 总减少={totalReduction}");
        }

        return Mathf.Max(0, originalPrice - totalReduction);
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