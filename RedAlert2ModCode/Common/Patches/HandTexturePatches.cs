using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Godot;

namespace RedAlert2ModCode.Common.Patches;

[HarmonyPatch]
internal static class HandTexturePatches
{
    private const string AlliesHandPathPrefix = "res://RedAlert2ModResources/images/ui/hands/multiplayer_hand_allies_";
    private const string SovietHandPathPrefix = "res://RedAlert2ModResources/images/ui/hands/multiplayer_hand_soviet_";
    
    public static void Install(Harmony harmony)
    {
        ModInitializer.Logger.Info("=== HandTexturePatches.Install - 开始安装手臂图片补丁 ===");

        // 手臂指向图片
        var armPointingGetter = AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.ArmPointingTexture));
        if (armPointingGetter != null)
        {
            harmony.Patch(
                original: armPointingGetter,
                prefix: new HarmonyMethod(typeof(HandTexturePatches), nameof(ArmPointingTexturePrefix))
            );
            ModInitializer.Logger.Info("HandTexturePatches.Install - 手臂指向图片补丁已安装");
        }
        else
        {
            ModInitializer.Logger.Error("HandTexturePatches.Install - 无法找到 ArmPointingTexture 属性的 getter 方法");
        }

        // 手臂石头图片
        var armRockGetter = AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.ArmRockTexture));
        if (armRockGetter != null)
        {
            harmony.Patch(
                original: armRockGetter,
                prefix: new HarmonyMethod(typeof(HandTexturePatches), nameof(ArmRockTexturePrefix))
            );
            ModInitializer.Logger.Info("HandTexturePatches.Install - 手臂石头图片补丁已安装");
        }
        else
        {
            ModInitializer.Logger.Error("HandTexturePatches.Install - 无法找到 ArmRockTexture 属性的 getter 方法");
        }

        // 手臂纸张图片
        var armPaperGetter = AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.ArmPaperTexture));
        if (armPaperGetter != null)
        {
            harmony.Patch(
                original: armPaperGetter,
                prefix: new HarmonyMethod(typeof(HandTexturePatches), nameof(ArmPaperTexturePrefix))
            );
            ModInitializer.Logger.Info("HandTexturePatches.Install - 手臂纸张图片补丁已安装");
        }
        else
        {
            ModInitializer.Logger.Error("HandTexturePatches.Install - 无法找到 ArmPaperTexture 属性的 getter 方法");
        }

        // 手臂剪刀图片
        var armScissorsGetter = AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.ArmScissorsTexture));
        if (armScissorsGetter != null)
        {
            harmony.Patch(
                original: armScissorsGetter,
                prefix: new HarmonyMethod(typeof(HandTexturePatches), nameof(ArmScissorsTexturePrefix))
            );
            ModInitializer.Logger.Info("HandTexturePatches.Install - 手臂剪刀图片补丁已安装");
        }
        else
        {
            ModInitializer.Logger.Error("HandTexturePatches.Install - 无法找到 ArmScissorsTexture 属性的 getter 方法");
        }
    }

    private static bool ArmPointingTexturePrefix(CharacterModel __instance, ref Texture2D __result)
    {
        return TryGetHandTexture(__instance, "point", ref __result);
    }

    private static bool ArmRockTexturePrefix(CharacterModel __instance, ref Texture2D __result)
    {
        return TryGetHandTexture(__instance, "rock", ref __result);
    }

    private static bool ArmPaperTexturePrefix(CharacterModel __instance, ref Texture2D __result)
    {
        return TryGetHandTexture(__instance, "paper", ref __result);
    }

    private static bool ArmScissorsTexturePrefix(CharacterModel __instance, ref Texture2D __result)
    {
        return TryGetHandTexture(__instance, "scissors", ref __result);
    }

    private static bool TryGetHandTexture(CharacterModel __instance, string handType, ref Texture2D __result)
    {
        string? handPath = null;
        
        // 检查是否是盟军角色
        if (__instance.GetType().Name == "Allies")
        {
            handPath = $"{AlliesHandPathPrefix}{handType}.png";
            ModInitializer.Logger.Info($"发现盟军角色，尝试加载手臂图片: {handPath}");
        }
        // 检查是否是苏军角色
        else if (__instance.GetType().Name == "Soviet")
        {
            handPath = $"{SovietHandPathPrefix}{handType}.png";
            ModInitializer.Logger.Info($"发现苏军角色，尝试加载手臂图片: {handPath}");
        }
        
        // 如果是mod角色，尝试加载自定义手臂图片
        if (handPath != null)
        {
            try
            {
                Texture2D? texture = ResourceLoader.Load<Texture2D>(handPath, null, ResourceLoader.CacheMode.Ignore);
                if (texture != null)
                {
                    __result = texture;
                    ModInitializer.Logger.Info($"成功加载手臂图片: {handPath}");
                    return false; // 跳过原始实现
                }
                else
                {
                    ModInitializer.Logger.Warn($"无法加载手臂图片，使用默认图片: {handPath}");
                }
            }
            catch (Exception ex)
            {
                ModInitializer.Logger.Error($"加载手臂图片失败: {handPath}, 错误: {ex.Message}");
            }
        }
        
        // 返回true，使用原始实现
        return true;
    }
}
