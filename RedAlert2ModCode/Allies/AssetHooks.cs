using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Godot;

namespace RedAlert2ModCode.Allies;

[HarmonyPatch]
internal static class AssetHooks
{
    private const string CombatArtNodeName = "AlliesCombatIllustration";
    private const string DeathArtNodeName = "AlliesDeathIllustration";
    private const string CharacterCombatImagePath = "res://RedAlert2ModResources/images/character/allies_character.png";
    private const string CharacterDeathImagePath = "res://RedAlert2ModResources/images/character/allies_character_die.png";

    public static void Install(Harmony harmony)
    {
        ModInitializer.Logger.Info("=== AssetHooks.Install - 开始安装盟军立绘补丁 ===");

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

    private static bool CharacterMerchantAnimPathPrefix(CharacterModel __instance, ref string __result)
    {
        ModInitializer.Logger.Info($"=== CharacterMerchantAnimPathPrefix 被调用 ===");
        ModInitializer.Logger.Info($"角色类型: {__instance.GetType().FullName}");

        if (__instance is not Allies)
        {
            ModInitializer.Logger.Info("不是盟军角色，跳过");
            return true;
        }

        __result = "res://RedAlert2ModResources/scenes/creature_visuals/allies_shop.tscn";
        ModInitializer.Logger.Info($"盟军角色，设置商店立绘路径为: {__result}");
        return false;
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

    private static bool StartDeathAnimPrefix(NCreature __instance, bool shouldRemove)
    {
        if (__instance.Entity.Player == null)
        {
            return true;
        }

        if (__instance.Entity.Player.Character is not Allies)
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
            ModInitializer.Logger.Error($"无法加载战斗立绘: {CharacterCombatImagePath}");
            return;
        }

        ModInitializer.Logger.Info($"成功加载战斗立绘: {CharacterCombatImagePath}");

        if (visuals.GetNodeOrNull<Sprite2D>("%Visuals") is { } body)
        {
            body.Texture = texture;
            body.Centered = true;
            body.Visible = true;
            body.Position = new Vector2(0f, -145f);
            body.Scale = new Vector2(0.29f, 0.29f);
            ModInitializer.Logger.Info("成功修改现有的Visuals节点");
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
            Scale = new Vector2(0.29f, 0.29f),
            ZIndex = 20
        };
        visuals.AddChild(art);
        ModInitializer.Logger.Info("成功创建新的Sprite2D立绘节点");
    }

    private static void ApplyDeathIllustration(NCreatureVisuals visuals, string imagePath)
    {
        Texture2D? texture = LoadTexture(imagePath);
        if (texture == null)
        {
            ModInitializer.Logger.Error($"无法加载死亡立绘: {imagePath}");
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
            // 上移增加，下移减少
            Position = new Vector2(0f, -85f),
            Scale = new Vector2(0.25f, 0.25f),
            ZIndex = 20
        };
        visuals.AddChild(deathArt);

        ModInitializer.Logger.Info($"成功显示死亡立绘: {imagePath}");
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
            ModInitializer.Logger.Error($"加载纹理失败: {path}, 错误: {ex.Message}");
            return null;
        }
    }
}