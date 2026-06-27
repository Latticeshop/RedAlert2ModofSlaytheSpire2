using Godot;
using MegaCrit.Sts2.Core.Runs;
using System.Linq;
using System.Reflection;

namespace RedAlert2ModCode.Common.Utils;

public enum FactionType
{
    Allied,
    Soviet,
    Yuri
}

public static class FactionHelper
{
    public static FactionType GetCurrentFaction()
    {
        if (RunManager.Instance == null)
        {
            GD.Print("[FactionHelper] RunManager.Instance 为空，返回默认阵营(盟军)");
            return FactionType.Allied;
        }

        RunState? currentRun = null;
        
        PropertyInfo? currentRunProperty = typeof(RunManager).GetProperty("CurrentRun");
        if (currentRunProperty != null)
        {
            currentRun = currentRunProperty.GetValue(RunManager.Instance) as RunState;
            GD.Print($"[FactionHelper] 通过属性获取CurrentRun: {currentRun != null}");
        }
        
        if (currentRun == null)
        {
            FieldInfo? currentRunField = typeof(RunManager).GetField("_currentRun", BindingFlags.Instance | BindingFlags.NonPublic);
            if (currentRunField != null)
            {
                currentRun = currentRunField.GetValue(RunManager.Instance) as RunState;
                GD.Print($"[FactionHelper] 通过字段获取_currentRun: {currentRun != null}");
            }
        }

        if (currentRun == null)
        {
            GD.Print("[FactionHelper] CurrentRun 为空，返回默认阵营(盟军)");
            return FactionType.Allied;
        }

        var localPlayer = currentRun.Players.FirstOrDefault();
        if (localPlayer == null)
        {
            GD.Print("[FactionHelper] localPlayer 为空，返回默认阵营(盟军)");
            return FactionType.Allied;
        }

        string characterTypeName = localPlayer.Character?.GetType().Name ?? string.Empty;
        GD.Print($"[FactionHelper] 角色类型名称: {characterTypeName}");
        
        if (characterTypeName == "Soviet")
        {
            GD.Print("[FactionHelper] 检测到苏军阵营，返回红色");
            return FactionType.Soviet;
        }

        GD.Print("[FactionHelper] 检测到盟军阵营，返回蓝色");
        return FactionType.Allied;
    }

    public static Color GetFactionPrimaryColor()
    {
        return GetCurrentFaction() switch
        {
            FactionType.Soviet => new Color(0.8f, 0.2f, 0.2f),
            FactionType.Yuri => new Color(0.6f, 0.2f, 0.8f),
            _ => new Color(0.3f, 0.5f, 0.8f)
        };
    }

    public static Color GetFactionSecondaryColor()
    {
        return GetCurrentFaction() switch
        {
            FactionType.Soviet => new Color(0.6f, 0.15f, 0.15f),
            FactionType.Yuri => new Color(0.45f, 0.15f, 0.6f),
            _ => new Color(0.15f, 0.22f, 0.35f)
        };
    }

    public static Color GetFactionBorderColor()
    {
        return GetCurrentFaction() switch
        {
            FactionType.Soviet => new Color(0.9f, 0.4f, 0.4f),
            FactionType.Yuri => new Color(0.8f, 0.4f, 1f),
            _ => new Color(0.4f, 0.6f, 0.9f)
        };
    }

    public static Color GetFactionButtonColor()
    {
        return GetCurrentFaction() switch
        {
            FactionType.Soviet => new Color(0.15f, 0.08f, 0.08f),
            FactionType.Yuri => new Color(0.15f, 0.08f, 0.2f),
            _ => new Color(0.12f, 0.18f, 0.28f)
        };
    }

    public static Color GetFactionButtonHoverColor()
    {
        return GetCurrentFaction() switch
        {
            FactionType.Soviet => new Color(0.25f, 0.12f, 0.12f),
            FactionType.Yuri => new Color(0.25f, 0.12f, 0.35f),
            _ => new Color(0.18f, 0.26f, 0.4f)
        };
    }
}