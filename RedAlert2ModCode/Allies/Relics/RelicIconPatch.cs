using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Relics;
using RedAlert2ModCode.Soviet.Relics;

namespace RedAlert2ModCode.Allies.Relics;

/// <summary>
/// Harmony补丁：拦截RelicModel.Icon属性，为自定义遗物提供正确的图标
/// </summary>
[HarmonyPatch]
public static class RelicIconPatch
{
    /// <summary>
    /// 自定义遗物类型到图标路径的映射
    /// </summary>
    private static readonly Dictionary<Type, string> _customIconPaths = new()
    {
        { typeof(DollarRelic), "res://RedAlert2ModResources/images/relics/dollar_relic.png" },
        { typeof(DollarAncientRelic), "res://RedAlert2ModResources/images/relics/doller-ancient.png" },
        { typeof(LibyaRelic), "res://RedAlert2ModResources/images/relics/flags/libya.png" },
    };

    /// <summary>
    /// 缓存已加载的图标
    /// </summary>
    private static readonly Dictionary<Type, Texture2D> _iconCache = new();

    /// <summary>
    /// 拦截RelicModel.Icon属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Icon), MethodType.Getter)]
    public static bool IconPrefix(RelicModel __instance, ref Texture2D __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (!_iconCache.TryGetValue(type, out Texture2D icon))
            {
                if (ResourceLoader.Exists(iconPath))
                {
                    icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
                    _iconCache[type] = icon;
                }
            }
            
            if (icon != null)
            {
                __result = icon;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 拦截RelicModel.PackedIconPath属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.PackedIconPath), MethodType.Getter)]
    public static bool PackedIconPathPrefix(RelicModel __instance, ref string __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            __result = iconPath;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 拦截RelicModel.BigIcon属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.BigIcon), MethodType.Getter)]
    public static bool BigIconPrefix(RelicModel __instance, ref Texture2D __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (!_iconCache.TryGetValue(type, out Texture2D icon))
            {
                if (ResourceLoader.Exists(iconPath))
                {
                    icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
                    _iconCache[type] = icon;
                }
            }
            
            if (icon != null)
            {
                __result = icon;
                return false;
            }
        }

        return true;
    }
}