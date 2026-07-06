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

    public int PriceReductionPerStack { get; set; } = (int)Values.Stars;

    public bool IsUpgraded { get; set; } = false;

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

        var allPowers = owner.Powers.OfType<MassProductionPower>().ToList();
        
        if (allPowers.Count > 0)
        {
            GD.Print($"[MassProductionPower] 发现现有大生产能力，当前数量={allPowers.Count}");
            
            if (isUpgraded)
            {
                var upgradedPower = allPowers.FirstOrDefault(p => p.IsUpgraded);
                var unupgradedPowers = allPowers.Where(p => !p.IsUpgraded).ToList();
                
                int totalUnupgradedStacks = unupgradedPowers.Sum(p => (int)p.Amount);
                
                GD.Print($"[MassProductionPower] 升级版本叠加逻辑 - 升级能力存在={upgradedPower != null}, 未升级能力数量={unupgradedPowers.Count}, 未升级总层数={totalUnupgradedStacks}");
                
                foreach (var unupgradedPower in unupgradedPowers)
                {
                    owner.RemovePowerInternal(unupgradedPower);
                    GD.Print($"[MassProductionPower] 移除未升级大生产能力");
                }
                
                if (upgradedPower != null)
                {
                    int oldAmount = (int)upgradedPower.Amount;
                    await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), upgradedPower, totalUnupgradedStacks, owner, null);
                    GD.Print($"[MassProductionPower] 升级能力叠加未升级层数 - 旧层数={oldAmount}, 叠加层数={totalUnupgradedStacks}, 新层数={upgradedPower.Amount}");
                }
                else
                {
                    var power = await PowerCmd.Apply<MassProductionPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
                    if (power != null)
                    {
                        power.IsUpgraded = true;
                        power.PriceReductionPerStack = (int)Values.Stars;
                        
                        if (totalUnupgradedStacks > 0)
                        {
                            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), power, totalUnupgradedStacks, owner, null);
                            GD.Print($"[MassProductionPower] 新建升级能力并叠加未升级层数 - 新层数={power.Amount}");
                        }
                    }
                }
            }
            else
            {
                var upgradedPower = allPowers.FirstOrDefault(p => p.IsUpgraded);
                var targetPower = upgradedPower ?? allPowers.First();
                
                GD.Print($"[MassProductionPower] 叠加层数到 {(targetPower.IsUpgraded ? "升级" : "未升级")} 能力 - 当前层数={targetPower.Amount}");
                
                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), targetPower, 1m, owner, null);
                GD.Print($"[MassProductionPower] 层数增加完成 - 新层数={targetPower.Amount}");
            }
            
            await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
            
            await RecalculateAllTrainingQueuePrices(owner);
            
            GD.Print($"[MassProductionPower] 重新计算所有生产序列价格完成");
            
            return allPowers.FirstOrDefault(p => p.IsUpgraded) ?? allPowers.First();
        }
        else
        {
            GD.Print($"[MassProductionPower] 创建新大生产能力 - isUpgraded={isUpgraded}");
            
            var power = await PowerCmd.Apply<MassProductionPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
            
            if (power != null)
            {
                power.IsUpgraded = isUpgraded;
                power.PriceReductionPerStack = (int)Values.Stars;
                GD.Print($"[MassProductionPower] 创建成功 - 层数={power.Amount}, IsUpgraded={power.IsUpgraded}, 价格减少={power.TotalPriceReduction}");
                
                await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
                
                await RecalculateAllTrainingQueuePrices(owner);
                
                GD.Print($"[MassProductionPower] 计算所有生产序列价格完成");
            }
            
            return power;
        }
    }

    public static async Task RecalculateAllTrainingQueuePrices(Creature owner)
    {
        GD.Print($"[MassProductionPower] RecalculateAllTrainingQueuePrices 被调用");
        
        var trainingQueuePowers = owner.Powers.OfType<TrainingQueuePower>().ToList();
        
        foreach (var trainingPower in trainingQueuePowers)
        {
            int originalPrice = trainingPower.OriginalUnitPrice;
            
            if (originalPrice == 0)
            {
                originalPrice = trainingPower.UnitPrice;
            }
            
            int massProductionPrice = CalculateUnitPrice(owner, originalPrice, (int)trainingPower.Amount);
            int finalPrice = TrainingQueuePower.ApplyIndustrialPlantDiscount(owner, massProductionPrice);
            
            GD.Print($"[MassProductionPower] 生产序列 {trainingPower.UnitName}: 原始价格={originalPrice}, 大生产后={massProductionPrice}, 工业工厂后={finalPrice}");
            
            trainingPower.UnitPrice = finalPrice;
        }
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

    public static int CalculateUnitPrice(Creature owner, int originalPrice, int trainingQueueStacks = 1)
    {
        var massProductionPower = owner.Powers.OfType<MassProductionPower>().FirstOrDefault();
        
        if (massProductionPower == null)
        {
            return originalPrice;
        }
        
        int totalReduction;
        
        if (massProductionPower.IsUpgraded)
        {
            totalReduction = (int)massProductionPower.Amount * trainingQueueStacks * massProductionPower.PriceReductionPerStack;
            GD.Print($"[MassProductionPower] CalculateUnitPrice (升级) - 原始价格={originalPrice}, 大生产层数={massProductionPower.Amount}, 生产序列层数={trainingQueueStacks}, 每层减少={massProductionPower.PriceReductionPerStack}, 总减少={totalReduction}");
        }
        else
        {
            totalReduction = (int)massProductionPower.Amount * massProductionPower.PriceReductionPerStack;
            GD.Print($"[MassProductionPower] CalculateUnitPrice (未升级) - 原始价格={originalPrice}, 大生产层数={massProductionPower.Amount}, 每层减少={massProductionPower.PriceReductionPerStack}, 总减少={totalReduction}");
        }
        
        int reducedPrice = originalPrice - totalReduction;
        return Mathf.Max(0, reducedPrice);
    }
}