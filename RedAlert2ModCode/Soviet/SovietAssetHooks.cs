using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Godot;

namespace RedAlert2ModCode.Soviet;

internal static class SovietAssetHooks
{
    private const string CombatArtNodeName = "SovietCombatIllustration";
    private const string CharacterCombatImagePath = "res://RedAlert2ModResources/images/character/soviet_character.png";

    public static void Install(Harmony harmony)
    {
        ModInitializer.Logger.Info("=== SovietAssetHooks.Install - 开始安装苏军立绘补丁 ===");

        // 注意：不再对 MerchantAnimPath 进行补丁，因为 PlaceholderCharacterModel 已经通过 CustomMerchantAnimPath 正确设置了路径
        // 移除这个补丁可以避免 BaseLib 的场景类型注册冲突

        // 战斗场景立绘补丁 - 仅用于确保立绘正确显示
        harmony.Patch(
            original: AccessTools.Method(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals)),
            postfix: new HarmonyMethod(typeof(SovietAssetHooks), nameof(CharacterCreateVisualsPostfix))
        );
        ModInitializer.Logger.Info("SovietAssetHooks.Install - 苏军战斗场景立绘补丁已安装");
    }

    private static void CharacterCreateVisualsPostfix(CharacterModel __instance, Node2D __result)
    {
        ModInitializer.Logger.Info($"=== SovietCharacterCreateVisualsPostfix 被调用 ===");
        ModInitializer.Logger.Info($"角色类型: {__instance.GetType().FullName}");

        if (__instance is not Soviet)
        {
            ModInitializer.Logger.Info("不是苏军角色，跳过");
            return;
        }

        ModInitializer.Logger.Info("是苏军角色，应用战斗立绘");
        ApplyCombatIllustration(__result);
    }

    private static void ApplyCombatIllustration(Node2D visuals)
    {
        Texture2D? texture = LoadPortableTexture(CharacterCombatImagePath);
        if (texture == null)
        {
            ModInitializer.Logger.Error($"无法加载苏军战斗立绘: {CharacterCombatImagePath}");
            return;
        }

        ModInitializer.Logger.Info($"成功加载苏军战斗立绘: {CharacterCombatImagePath}");

        // 尝试直接修改现有的Visuals节点
        if (visuals.GetNodeOrNull<Sprite2D>("%Visuals") is { } body)
        {
            body.Texture = texture;
            body.Centered = true;
            body.Visible = true;
            body.Position = new Vector2(0f, -175f);
            body.Scale = new Vector2(0.33f, 0.33f);
            ModInitializer.Logger.Info("成功修改苏军现有的Visuals节点");
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
        ModInitializer.Logger.Info("成功创建苏军新的Sprite2D立绘节点");
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
            ModInitializer.Logger.Error($"加载苏军纹理失败: {path}, 错误: {ex.Message}");
            return null;
        }
    }
}
