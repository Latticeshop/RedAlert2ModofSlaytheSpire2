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
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 大生产能力
/// 效果：每有一层大生产能力，其单位价格减少一定金额
/// 升级版本效果更强：效果取升级的，生产序列层数参与计算
/// </summary>
public class MassProductionPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.MassProductionPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 当前每层减少的价格
    /// </summary>
    public int PriceReductionPerStack { get; set; } = (int)Values.Stars;

    /// <summary>
    /// 是否为升级版本的能力
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

    /// <summary>
    /// 获取当前总价格减少量（层数 * 每层减少量）
    /// </summary>
    public int TotalPriceReduction => (int)Amount * PriceReductionPerStack;

    /// <summary>
    /// 本地化描述
    /// 根据升级状态显示不同的描述文本
    /// </summary>
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

    /// <summary>
    /// 应用大生产能力（支持堆叠）
    /// 升级和未升级可以叠加，但效果取升级的
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="isUpgraded">是否是升级版本</param>
    public static async Task<MassProductionPower?> ApplyMassProduction(Creature owner, bool isUpgraded = false)
    {
        GD.Print($"[MassProductionPower] ApplyMassProduction 被调用 - isUpgraded={isUpgraded}");

        // 获取所有大生产能力
        var allPowers = owner.Powers.OfType<MassProductionPower>().ToList();
        
        if (allPowers.Count > 0)
        {
            // 已有大生产能力
            GD.Print($"[MassProductionPower] 发现现有大生产能力，当前数量={allPowers.Count}");
            
            if (isUpgraded)
            {
                // 打出升级大生产：
                // 1. 移除所有未升级的大生产能力（如果有）
                // 2. 将其层数叠加到升级版本上
                
                var upgradedPower = allPowers.FirstOrDefault(p => p.IsUpgraded);
                var unupgradedPowers = allPowers.Where(p => !p.IsUpgraded).ToList();
                
                int totalUnupgradedStacks = unupgradedPowers.Sum(p => (int)p.Amount);
                
                GD.Print($"[MassProductionPower] 升级版本叠加逻辑 - 升级能力存在={upgradedPower != null}, 未升级能力数量={unupgradedPowers.Count}, 未升级总层数={totalUnupgradedStacks}");
                
                // 移除所有未升级的大生产能力
                foreach (var unupgradedPower in unupgradedPowers)
                {
                    owner.RemovePowerInternal(unupgradedPower);
                    GD.Print($"[MassProductionPower] 移除未升级大生产能力");
                }
                
                if (upgradedPower != null)
                {
                    // 升级能力存在，将未升级层数叠加到升级能力上
                    int oldAmount = (int)upgradedPower.Amount;
                    await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), upgradedPower, totalUnupgradedStacks, owner, null);
                    GD.Print($"[MassProductionPower] 升级能力叠加未升级层数 - 旧层数={oldAmount}, 叠加层数={totalUnupgradedStacks}, 新层数={upgradedPower.Amount}");
                }
                else
                {
                    // 升级能力不存在，创建新的升级能力并叠加未升级层数
                    var power = await PowerCmd.Apply<MassProductionPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
                    if (power != null)
                    {
                        power.IsUpgraded = true;
                        power.PriceReductionPerStack = (int)Values.Stars;
                        
                        // 如果有未升级层数需要叠加
                        if (totalUnupgradedStacks > 0)
                        {
                            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), power, totalUnupgradedStacks, owner, null);
                            GD.Print($"[MassProductionPower] 新建升级能力并叠加未升级层数 - 新层数={power.Amount}");
                        }
                    }
                    // 不要提前返回，继续执行后面的动画和价格计算
                }
            }
            else
            {
                // 打出未升级大生产：直接叠加层数到已有能力上
                // 如果有升级能力，叠加到升级能力上；否则叠加到未升级能力上
                var upgradedPower = allPowers.FirstOrDefault(p => p.IsUpgraded);
                var targetPower = upgradedPower ?? allPowers.First();
                
                GD.Print($"[MassProductionPower] 叠加层数到 {(targetPower.IsUpgraded ? "升级" : "未升级")} 能力 - 当前层数={targetPower.Amount}");
                
                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), targetPower, 1m, owner, null);
                GD.Print($"[MassProductionPower] 层数增加完成 - 新层数={targetPower.Amount}");
            }
            
            // 触发叠加动画反馈
            await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
            
            // 重新计算所有生产序列的单位价格
            await RecalculateAllTrainingQueuePrices(owner);
            
            GD.Print($"[MassProductionPower] 重新计算所有生产序列价格完成");
            
            return allPowers.FirstOrDefault(p => p.IsUpgraded) ?? allPowers.First();
        }
        else
        {
            // 没有大生产能力，创建新能力
            GD.Print($"[MassProductionPower] 创建新大生产能力 - isUpgraded={isUpgraded}");
            
            var power = await PowerCmd.Apply<MassProductionPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
            
            if (power != null)
            {
                power.IsUpgraded = isUpgraded;
                power.PriceReductionPerStack = (int)Values.Stars;
                GD.Print($"[MassProductionPower] 创建成功 - 层数={power.Amount}, IsUpgraded={power.IsUpgraded}, 价格减少={power.TotalPriceReduction}");
                
                // 触发动画反馈
                await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
                
                // 计算所有生产序列的单位价格
                await RecalculateAllTrainingQueuePrices(owner);
                
                GD.Print($"[MassProductionPower] 计算所有生产序列价格完成");
            }
            
            return power;
        }
    }

    /// <summary>
    /// 重新计算所有生产序列的单位价格
    /// 根据大生产能力的升级状态使用不同的计算公式
    /// </summary>
    /// <param name="owner">拥有者</param>
    public static async Task RecalculateAllTrainingQueuePrices(Creature owner)
    {
        GD.Print($"[MassProductionPower] RecalculateAllTrainingQueuePrices 被调用");
        
        // 获取大生产能力
        var massProductionPower = owner.Powers.OfType<MassProductionPower>().FirstOrDefault();
        if (massProductionPower == null)
        {
            GD.Print($"[MassProductionPower] 没有大生产能力，跳过计算");
            return;
        }
        
        GD.Print($"[MassProductionPower] 大生产能力 - 层数={massProductionPower.Amount}, IsUpgraded={massProductionPower.IsUpgraded}");
        
        // 获取所有生产序列能力
        var trainingQueuePowers = owner.Powers.OfType<TrainingQueuePower>().ToList();
        
        foreach (var trainingPower in trainingQueuePowers)
        {
            // 获取原始价格（从训练队列存储的原始价格获取）
            int originalPrice = trainingPower.OriginalUnitPrice;
            
            // 如果原始价格为0（旧数据兼容），使用当前价格作为备用
            if (originalPrice == 0)
            {
                originalPrice = trainingPower.UnitPrice;
            }
            
            // 根据大生产是否升级使用不同的计算公式
            int newPrice = CalculateUnitPrice(owner, originalPrice, (int)trainingPower.Amount);
            
            GD.Print($"[MassProductionPower] 生产序列 {trainingPower.UnitName}: 原始价格={originalPrice}, 大生产层数={massProductionPower.Amount}, 生产序列层数={trainingPower.Amount}, IsUpgraded={massProductionPower.IsUpgraded}, 新价格={newPrice}");
            
            // 更新价格
            trainingPower.UnitPrice = newPrice;
        }
    }

    /// <summary>
    /// 获取单位的原始价格
    /// 优先使用训练队列存储的原始价格，兼容旧数据
    /// </summary>
    private static int GetOriginalUnitPrice(TrainingQueuePower trainingPower)
    {
        // 优先使用存储的原始价格
        if (trainingPower.OriginalUnitPrice > 0)
        {
            return trainingPower.OriginalUnitPrice;
        }
        
        // 兼容旧数据：如果没有存储原始价格，检查是否有大生产能力影响
        var massProductionPower = trainingPower.Owner?.Powers.OfType<MassProductionPower>().FirstOrDefault();
        
        if (massProductionPower != null)
        {
            // 如果有大生产能力，当前价格 = 原始价格 - 总减少量
            // 所以原始价格 = 当前价格 + 总减少量
            return trainingPower.UnitPrice + massProductionPower.TotalPriceReduction;
        }
        
        // 如果没有大生产能力，当前价格就是原始价格
        return trainingPower.UnitPrice;
    }

    /// <summary>
    /// 计算单个生产序列的单位价格（考虑大生产效果）
    /// 未升级大生产：最终价格 = max(0, 原始价格 - 大生产层数 × $100)
    /// 升级大生产：最终价格 = max(0, 原始价格 - 大生产层数 × 生产序列层数 × $100)
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="originalPrice">原始单位价格</param>
    /// <param name="trainingQueueStacks">生产序列的层数（默认为1）</param>
    /// <returns>计算后的价格（最低为0）</returns>
    public static int CalculateUnitPrice(Creature owner, int originalPrice, int trainingQueueStacks = 1)
    {
        // 获取大生产能力
        var massProductionPower = owner.Powers.OfType<MassProductionPower>().FirstOrDefault();
        
        if (massProductionPower == null)
        {
            return originalPrice;
        }
        
        int totalReduction;
        
        if (massProductionPower.IsUpgraded)
        {
            // 升级大生产：考虑生产序列层数
            // 公式：原始价格 - 大生产层数 × 生产序列层数 × 每层减少量
            totalReduction = (int)massProductionPower.Amount * trainingQueueStacks * massProductionPower.PriceReductionPerStack;
            GD.Print($"[MassProductionPower] CalculateUnitPrice (升级) - 原始价格={originalPrice}, 大生产层数={massProductionPower.Amount}, 生产序列层数={trainingQueueStacks}, 每层减少={massProductionPower.PriceReductionPerStack}, 总减少={totalReduction}");
        }
        else
        {
            // 未升级大生产：不考虑生产序列层数
            // 公式：原始价格 - 大生产层数 × 每层减少量
            totalReduction = (int)massProductionPower.Amount * massProductionPower.PriceReductionPerStack;
            GD.Print($"[MassProductionPower] CalculateUnitPrice (未升级) - 原始价格={originalPrice}, 大生产层数={massProductionPower.Amount}, 每层减少={massProductionPower.PriceReductionPerStack}, 总减少={totalReduction}");
        }
        
        // 计算价格（最低为0）
        int reducedPrice = originalPrice - totalReduction;
        return Mathf.Max(0, reducedPrice);
    }
}
