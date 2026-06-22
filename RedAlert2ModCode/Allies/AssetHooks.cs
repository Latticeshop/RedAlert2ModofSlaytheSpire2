using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Godot;

namespace RedAlert2ModCode.Allies;

internal static class AssetHooks
{
    private const string CombatArtNodeName = "AlliesCombatIllustration";
    private const string CharacterCombatImagePath = "res://RedAlert2ModResources/images/charui/allies_character.png";

    public static void Install(Harmony harmony)
    {
        ModInitializer.Logger.Info("=== AssetHooks.Install - 开始安装立绘补丁 ===");

        // 商店场景立绘补丁 - 使用Prefix拦截并返回自定义路径
        var merchantAnimPathGetter = AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.MerchantAnimPath));
        if (merchantAnimPathGetter != null)
        {
            harmony.Patch(
                original: merchantAnimPathGetter,
                prefix: new HarmonyMethod(typeof(AssetHooks), nameof(CharacterMerchantAnimPathPrefix))
            );
            ModInitializer.Logger.Info("AssetHooks.Install - 商店场景立绘补丁已安装");
        }
        else
        {
            ModInitializer.Logger.Error("AssetHooks.Install - 无法找到 MerchantAnimPath 属性的 getter 方法");
        }

        // 战斗场景立绘补丁
        harmony.Patch(
            original: AccessTools.Method(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals)),
            postfix: new HarmonyMethod(typeof(AssetHooks), nameof(CharacterCreateVisualsPostfix))
        );
        ModInitializer.Logger.Info("AssetHooks.Install - 战斗场景立绘补丁已安装");
    }

    private static bool CharacterMerchantAnimPathPrefix(CharacterModel __instance, ref string __result)
    {
        ModInitializer.Logger.Info($"=== CharacterMerchantAnimPathPrefix 被调用 ===");
        ModInitializer.Logger.Info($"角色类型: {__instance.GetType().FullName}");

        if (__instance is not Allies)
        {
            ModInitializer.Logger.Info("不是盟军角色，跳过");
            return true;
        }

        // 返回使用Sprite2D的商店场景，避免Node2D报错
        __result = "res://RedAlert2ModResources/scenes/creature_visuals/allies_shop_simple.tscn";
        ModInitializer.Logger.Info($"盟军角色，设置商店立绘路径为: {__result}");
        return false; // 阻止原始方法执行
    }

    private static void CharacterCreateVisualsPostfix(CharacterModel __instance, Node2D __result)
    {
        ModInitializer.Logger.Info($"=== CharacterCreateVisualsPostfix 被调用 ===");
        ModInitializer.Logger.Info($"角色类型: {__instance.GetType().FullName}");

        if (__instance is not Allies)
        {
            ModInitializer.Logger.Info("不是盟军角色，跳过");
            return;
        }

        ModInitializer.Logger.Info("是盟军角色，应用战斗立绘");
        ApplyCombatIllustration(__result);
    }

    private static void ApplyCombatIllustration(Node2D visuals)
    {
        Texture2D? texture = LoadPortableTexture(CharacterCombatImagePath);
        if (texture == null)
        {
            ModInitializer.Logger.Error($"无法加载战斗立绘: {CharacterCombatImagePath}");
            return;
        }

        ModInitializer.Logger.Info($"成功加载战斗立绘: {CharacterCombatImagePath}");

        // 尝试直接修改现有的Visuals节点
        if (visuals.GetNodeOrNull<Sprite2D>("%Visuals") is { } body)
        {
            body.Texture = texture;
            body.Centered = true;
            body.Visible = true;
            body.Position = new Vector2(0f, -175f);
            body.Scale = new Vector2(0.33f, 0.33f);
            ModInitializer.Logger.Info("成功修改现有的Visuals节点");
            return;
        }

        // 如果没有找到Visuals节点，创建一个新的Sprite2D
        RefreshCombatIllustration(visuals);
    }

    private static void RefreshCombatIllustration(Node2D visuals)
    {
        // 移除旧的立绘节点
        foreach (Node child in visuals.GetChildren())
        {
            if (child.Name == CombatArtNodeName)
            {
                child.QueueFree();
            }
        }

        Texture2D? texture = LoadPortableTexture(CharacterCombatImagePath);
        if (texture == null)
        {
            return;
        }

        // 隐藏现有的战斗视觉效果
        HideExistingCombatVisuals(visuals);

        // 创建新的Sprite2D来显示立绘
        Sprite2D art = new()
        {
            Name = CombatArtNodeName,
            Texture = texture,
            Centered = true,
            Position = new Vector2(0f, -175f),
            Scale = new Vector2(0.33f, 0.33f),
            ZIndex = 20
        };
        visuals.AddChild(art);
        ModInitializer.Logger.Info("成功创建新的Sprite2D立绘节点");
    }

    private static void HideExistingCombatVisuals(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child.Name == CombatArtNodeName)
            {
                continue;
            }

            if (child is Node2D node2D)
            {
                node2D.Hide();
            }

            HideExistingCombatVisuals(child);
        }
    }

    private static Texture2D? LoadPortableTexture(string path)
    {
        try
        {
            return ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Ignore);
        }
        catch (Exception ex)
        {
            ModInitializer.Logger.Error($"加载纹理失败: {path}, 错误: {ex.Message}");
            return null;
        }
    }
}