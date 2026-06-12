using System.Collections.Concurrent;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

public static class PowerIconManager
{
    // 存储所有 TrainingQueuePower 的图标路径（按哈希码索引）
    private static readonly ConcurrentDictionary<int, string> _iconPathsByHashCode = new ConcurrentDictionary<int, string>();
    
    // 存储所有已知的能力哈希码
    private static readonly HashSet<int> _knownPowerHashCodes = new HashSet<int>();
    
    // 当前活跃的图标路径（用于解决克隆问题）
    private static string? _currentIconPath = null;
    private static int _currentIconHashCode = 0;

    /// <summary>
    /// 获取能力实例的哈希码
    /// </summary>
    public static int GetPowerHashCode(PowerModel power)
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(power);
    }
    
    /// <summary>
    /// 注册一个能力实例的哈希码（DeepCloneFields时调用）
    /// </summary>
    public static void RegisterPowerHashCode(PowerModel power)
    {
        int hashCode = GetPowerHashCode(power);
        _knownPowerHashCodes.Add(hashCode);
        GD.Print($"[PowerIconManager] RegisterPowerHashCode - HashCode={hashCode}, CurrentPath={_currentIconPath}");
        
        // 如果当前有活跃的图标路径，立即设置
        if (_currentIconPath != null)
        {
            _iconPathsByHashCode[hashCode] = _currentIconPath;
            GD.Print($"[PowerIconManager] RegisterPowerHashCode: 立即设置图标路径={_currentIconPath}");
        }
    }

    public static void SetIcon(PowerModel power, string iconPath)
    {
        int hashCode = GetPowerHashCode(power);
        _currentIconHashCode = hashCode;
        _currentIconPath = iconPath;
        _iconPathsByHashCode[hashCode] = iconPath;
        _knownPowerHashCodes.Add(hashCode);
        
        GD.Print($"[PowerIconManager] SetIcon - HashCode={hashCode}, Path={iconPath}");
    }
    
    /// <summary>
    /// 直接设置当前活跃的图标路径（不依赖能力实例）
    /// </summary>
    public static void SetCurrentIconPath(string iconPath)
    {
        _currentIconPath = iconPath;
        GD.Print($"[PowerIconManager] SetCurrentIconPath - Path={iconPath}");
        
        // 同时更新所有已知的哈希码
        foreach (int hashCode in _knownPowerHashCodes)
        {
            _iconPathsByHashCode[hashCode] = iconPath;
        }
    }

    public static string? GetIconPath(PowerModel power)
    {
        int hashCode = GetPowerHashCode(power);
        
        // 直接通过哈希码查找
        if (_iconPathsByHashCode.TryGetValue(hashCode, out string iconPath))
        {
            return iconPath;
        }
        
        // 如果是 TrainingQueuePower，尝试使用当前活跃的图标路径
        if (power is TrainingQueuePower && _currentIconPath != null)
        {
            GD.Print($"[PowerIconManager] GetIconPath: 使用当前活跃图标路径={_currentIconPath}");
            return _currentIconPath;
        }
        
        return null;
    }

    public static Texture2D? GetIcon(PowerModel power)
    {
        string? iconPath = GetIconPath(power);
        if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
        {
            return ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
        }
        return null;
    }
}