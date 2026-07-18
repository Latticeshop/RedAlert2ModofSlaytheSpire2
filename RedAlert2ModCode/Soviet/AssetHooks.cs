using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Godot;

namespace RedAlert2ModCode.Soviet;

[HarmonyPatch]
internal static class AssetHooks
{
    private const string CombatArtNodeName = "SovietCombatIllustration";
    private const string DeathArtNodeName = "SovietDeathIllustration";
    private const string CharacterCombatImagePath = "res://RedAlert2ModResources/images/character/soviet_character.png";
    private const string CharacterDeathImagePath = "res://RedAlert2ModResources/images/character/soviet_character_die.png";

    public static void Install(Harmony harmony)
    {
        ModInitializer.Logger.Info("=== AssetHooks.Install - 开始安装苏军立绘补丁 ===");

        harmony.Patch(
            original: AccessTools.Method(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals)),
            postfix: new HarmonyMethod(typeof(AssetHooks), nameof(CharacterCreateVisualsPostfix))
        );
        ModInitializer.Logger.Info("AssetHooks.Install - 战斗场景立绘补丁已安装");

        harmony.Patch(
            original: AccessTools.Method(typeof(NCreature), nameof(NCreature.StartDeathAnim)),
            prefix: new HarmonyMethod(typeof(AssetHooks), nameof(StartDeathAnimPrefix))
        );
        ModInitializer.Logger.Info("AssetHooks.Install - 死亡立绘补丁已安装");
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

    private static bool StartDeathAnimPrefix(NCreature __instance, bool shouldRemove)
    {
        if (__instance.Entity.Player == null)
        {
            return true;
        }

        if (__instance.Entity.Player.Character.GetType().Name != "Soviet")
        {
            return true;
        }

        ApplyDeathIllustration(__instance.Visuals, CharacterDeathImagePath);
        return true;
    }

    private static void ApplyCombatIllustration(Node2D visuals)
    {
        Texture2D? texture = LoadTexture(CharacterCombatImagePath);
        if (texture == null)
        {
            ModInitializer.Logger.Error($"无法加载苏军战斗立绘: {CharacterCombatImagePath}");
            return;
        }

        ModInitializer.Logger.Info($"成功加载苏军战斗立绘: {CharacterCombatImagePath}");

        if (visuals.GetNodeOrNull<Sprite2D>("%Visuals") is { } body)
        {
            body.Texture = texture;
            body.Centered = true;
            body.Visible = true;
            body.Position = new Vector2(0f, -145f);
            body.Scale = new Vector2(0.25f, 0.25f);
            ModInitializer.Logger.Info("成功修改苏军现有的Visuals节点");
            return;
        }

        RefreshCombatIllustration(visuals);
    }

    private static void RefreshCombatIllustration(Node2D visuals)
    {
        foreach (Node child in visuals.GetChildren())
        {
            if (child.Name == CombatArtNodeName)
            {
                child.QueueFree();
            }
        }

        Texture2D? texture = LoadTexture(CharacterCombatImagePath);
        if (texture == null)
        {
            return;
        }

        HideExistingCombatVisuals(visuals);

        Sprite2D art = new()
        {
            Name = CombatArtNodeName,
            Texture = texture,
            Centered = true,
            Position = new Vector2(0f, -145f),
            Scale = new Vector2(0.25f, 0.25f),
            ZIndex = 20
        };
        visuals.AddChild(art);
        ModInitializer.Logger.Info("成功创建苏军新的Sprite2D立绘节点");
    }

    private static void ApplyDeathIllustration(NCreatureVisuals visuals, string imagePath)
    {
        Texture2D? texture = LoadTexture(imagePath);
        if (texture == null)
        {
            ModInitializer.Logger.Error($"无法加载苏军死亡立绘: {imagePath}");
            return;
        }

        foreach (Node child in visuals.GetChildren())
        {
            if (child.Name == DeathArtNodeName)
            {
                child.QueueFree();
            }
        }

        HideExistingVisuals(visuals);

        Sprite2D deathArt = new()
        {
            Name = DeathArtNodeName,
            Texture = texture,
            Centered = true,
            Position = new Vector2(0f, -55f),
            Scale = new Vector2(0.25f, 0.25f),
            ZIndex = 20
        };
        visuals.AddChild(deathArt);

        ModInitializer.Logger.Info($"成功显示苏军死亡立绘: {imagePath}");
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

    private static void HideExistingVisuals(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child.Name == DeathArtNodeName)
            {
                continue;
            }

            if (child is Node2D node2D)
            {
                node2D.Hide();
            }

            HideExistingVisuals(child);
        }
    }

    private static Texture2D? LoadTexture(string path)
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