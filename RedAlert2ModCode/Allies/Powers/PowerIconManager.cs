using System.Collections.Concurrent;
using Godot;

namespace RedAlert2ModCode.Allies.Powers;

public static class PowerIconManager
{
    // 存储能力实例到图标路径的映射
    private static readonly ConcurrentDictionary<object, string> _powerIconPaths = new ConcurrentDictionary<object, string>();
    
    // 存储能力实例到图标的映射
    private static readonly ConcurrentDictionary<object, Texture2D> _powerIcons = new ConcurrentDictionary<object, Texture2D>();

    public static void SetIcon(object powerInstance, string iconPath)
    {
        _powerIconPaths[powerInstance] = iconPath;
        
        // 预加载图标
        if (ResourceLoader.Exists(iconPath))
        {
            Texture2D icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
            _powerIcons[powerInstance] = icon;
        }
    }

    public static Texture2D? GetIcon(object powerInstance)
    {
        if (_powerIcons.TryGetValue(powerInstance, out Texture2D icon))
        {
            return icon;
        }
        
        if (_powerIconPaths.TryGetValue(powerInstance, out string iconPath))
        {
            if (ResourceLoader.Exists(iconPath))
            {
                icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
                _powerIcons[powerInstance] = icon;
                return icon;
            }
        }
        
        return null;
    }

    public static string? GetIconPath(object powerInstance)
    {
        if (_powerIconPaths.TryGetValue(powerInstance, out string iconPath))
        {
            return iconPath;
        }
        return null;
    }
}