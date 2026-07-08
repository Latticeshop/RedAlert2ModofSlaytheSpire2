using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Allies;

namespace RedAlert2ModCode.Allies.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class DeathVisualPatch
{
    private const string DeathArtNodeName = "DeathIllustration";
    private const string AlliesDeathImagePath = "res://RedAlert2ModResources/images/character/allies_character_die.png";
    private const string SovietDeathImagePath = "res://RedAlert2ModResources/images/character/soviet_character_die.png";

    public static bool Prefix(NCreature __instance, bool shouldRemove)
    {
        if (__instance.Entity.Player == null)
        {
            return true;
        }

        if (__instance.Entity.Player.Character is not Allies && __instance.Entity.Player.Character.GetType().Name != "Soviet")
        {
            return true;
        }

        string deathImagePath = __instance.Entity.Player.Character is Allies ? AlliesDeathImagePath : SovietDeathImagePath;
        ApplyDeathIllustration(__instance.Visuals, deathImagePath);

        return true;
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
            Position = new Vector2(0f, -85f),
            Scale = new Vector2(0.33f, 0.33f),
            ZIndex = 20
        };
        visuals.AddChild(deathArt);
        
        ModInitializer.Logger.Info($"成功显示死亡立绘: {imagePath}");
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