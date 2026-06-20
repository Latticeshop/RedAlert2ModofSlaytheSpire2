using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies.Powers;
using Godot;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 绝地战备系统 - 存储各种绝地战备能力
/// 当飞机类卡牌或能力造成伤害时，检查是否有绝地战备能力，有则消耗一层替换攻击效果
/// </summary>
public static class DesperateMeasures
{
    /// <summary>
    /// 绝地战备能力类型列表
    /// 所有继承自 IDesperateMeasurePower 接口的能力都会被识别为绝地战备
    /// </summary>
    private static readonly List<Type> DesperateMeasurePowerTypes = new()
    {
        typeof(Eagle500kgPower)
    };

    /// <summary>
    /// 检查玩家是否有任何绝地战备能力
    /// </summary>
    /// <param name="player">玩家生物</param>
    /// <returns>是否存在绝地战备能力</returns>
    public static bool HasDesperateMeasure(Creature player)
    {
        return player.Powers.Any(p => IsDesperateMeasurePower(p));
    }

    /// <summary>
    /// 获取玩家的第一个绝地战备能力
    /// </summary>
    /// <param name="player">玩家生物</param>
    /// <returns>第一个绝地战备能力，如果没有则返回null</returns>
    public static PowerModel? GetFirstDesperateMeasure(Creature player)
    {
        return player.Powers.FirstOrDefault(p => IsDesperateMeasurePower(p));
    }

    /// <summary>
    /// 获取玩家的所有绝地战备能力
    /// </summary>
    /// <param name="player">玩家生物</param>
    /// <returns>所有绝地战备能力列表</returns>
    public static List<PowerModel> GetAllDesperateMeasures(Creature player)
    {
        return player.Powers.Where(p => IsDesperateMeasurePower(p)).ToList();
    }

    /// <summary>
    /// 检查能力是否为绝地战备类型
    /// </summary>
    /// <param name="power">能力</param>
    /// <returns>是否为绝地战备能力</returns>
    public static bool IsDesperateMeasurePower(PowerModel power)
    {
        if (power == null)
        {
            GD.Print("[DesperateMeasures] IsDesperateMeasurePower: power is null");
            return false;
        }
        
        Type powerType = power.GetType();
        GD.Print($"[DesperateMeasures] IsDesperateMeasurePower - 检查能力类型: {powerType.FullName}");
        
        // 使用 IsAssignableFrom 进行更宽松的类型检查
        // 这可以处理能力被包装或代理的情况
        foreach (Type type in DesperateMeasurePowerTypes)
        {
            bool isAssignable = type.IsAssignableFrom(powerType);
            GD.Print($"[DesperateMeasures]   检查类型 {type.Name} 是否可分配: {isAssignable}");
            if (isAssignable)
            {
                GD.Print($"[DesperateMeasures]   ✓ 找到匹配的绝地战备能力类型");
                return true;
            }
        }
        
        GD.Print("[DesperateMeasures] ✗ 未找到匹配的绝地战备能力类型");
        return false;
    }

    /// <summary>
    /// 尝试执行绝地战备攻击（消耗一层）
    /// 用于飞机类卡牌和能力在造成伤害前调用
    /// </summary>
    /// <param name="player">玩家生物</param>
    /// <param name="target">攻击目标</param>
    /// <param name="ctx">玩家选择上下文</param>
    /// <returns>是否成功执行绝地战备攻击</returns>
    public static async Task<bool> TryExecuteDesperateMeasureAttack(Creature player, Creature target, PlayerChoiceContext ctx)
    {
        GD.Print($"[DesperateMeasures] TryExecuteDesperateMeasureAttack 被调用 - Player={player?.Name}, Target={target?.Name}");
        
        // 检查玩家是否为空
        if (player == null)
        {
            GD.PrintErr("[DesperateMeasures] 玩家为空，无法执行绝地战备攻击");
            return false;
        }
        
        // 检查是否有绝地战备能力
        var desperateMeasure = GetFirstDesperateMeasure(player);
        GD.Print($"[DesperateMeasures] 找到绝地战备能力: {desperateMeasure != null}");
        
        if (desperateMeasure != null && desperateMeasure is IDesperateMeasurePower dmPower)
        {
            GD.Print($"[DesperateMeasures] 发现绝地战备能力: {desperateMeasure.GetType().Name}, 层数: {desperateMeasure.Amount}");
            
            // 检查目标是否有效
            if (target != null && target.IsAlive)
            {
                bool success = await dmPower.ExecuteDesperateMeasureAttack(target, ctx);
                if (success)
                {
                    GD.Print("[DesperateMeasures] 绝地战备攻击成功");
                    return true;
                }
                else
                {
                    GD.Print("[DesperateMeasures] 绝地战备攻击失败");
                    return false;
                }
            }
            else
            {
                GD.Print("[DesperateMeasures] 目标无效或已死亡");
                return false;
            }
        }
        else
        {
            GD.Print("[DesperateMeasures] 没有找到有效的绝地战备能力");
            return false;
        }
    }

    /// <summary>
    /// 注册新的绝地战备能力类型
    /// </summary>
    /// <param name="powerType">能力类型</param>
    public static void RegisterDesperateMeasure(Type powerType)
    {
        if (!DesperateMeasurePowerTypes.Contains(powerType))
        {
            DesperateMeasurePowerTypes.Add(powerType);
        }
    }
}

/// <summary>
/// 绝地战备能力接口
/// 所有绝地战备能力都应该实现此接口
/// </summary>
public interface IDesperateMeasurePower
{
    /// <summary>
    /// 执行绝地战备攻击效果
    /// 替换飞机类卡牌或能力的普通攻击
    /// </summary>
    /// <param name="target">攻击目标</param>
    /// <param name="ctx">玩家选择上下文</param>
    /// <returns>是否成功执行</returns>
    Task<bool> ExecuteDesperateMeasureAttack(Creature target, MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext ctx);
}