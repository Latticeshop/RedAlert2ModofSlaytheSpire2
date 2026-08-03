// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.DeckConfig;

/// <summary>
/// 基地车开局模式阵营选项
/// </summary>
public enum BaseCarMode
{
    None,       // 无
    Allied,     // 盟军
    Soviet,     // 苏军
    Yuri        // 尤里
}

/// <summary>
/// 卡池奖励模式（控制箱子卡是否/如何进入角色的卡牌奖励范围）
/// </summary>
public enum CratePoolMode
{
    None,       // 默认卡池（排除箱子）
    AllCrates,  // 卡池奖励全为箱子
    AddCrates   // 卡池奖励加入箱子
}

/// <summary>
/// 角色配置数据
/// </summary>
public class CharacterConfig
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// 自定义初始卡组（卡牌类型名列表），为空则使用默认
    /// </summary>
    public List<string> CustomDeckCardTypes { get; set; } = new();

    /// <summary>
    /// 是否启用自定义卡组
    /// </summary>
    public bool EnableCustomDeck { get; set; }

    /// <summary>
    /// 基地车开局模式
    /// </summary>
    public BaseCarMode BaseCarMode { get; set; } = BaseCarMode.None;

    /// <summary>
    /// 是否启用幸运方块捡箱子模式
    /// </summary>
    public bool LuckyCrateMode { get; set; }

    /// <summary>
    /// 卡池奖励模式（None=默认排除箱子，AllCrates=全为箱子，AddCrates=加入箱子）
    /// </summary>
    public CratePoolMode CratePoolMode { get; set; } = CratePoolMode.None;
}

/// <summary>
/// Mod配置管理器 - 负责配置的保存、加载和读取
/// </summary>
public static class ModConfigManager
{
    private const string ConfigFileName = "RedAlert2ModConfig.json";
    private static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = new("RedAlert2Mod", LogType.Generic);

    private static Dictionary<string, CharacterConfig>? _configs;
    private static bool _initialized;

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static string ConfigPath { get; private set; } = string.Empty;

    /// <summary>
    /// 初始化配置管理器
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;

        _initialized = true;
        ConfigPath = GetConfigPath();
        Load();

        Logger.Info($"[ModConfigManager] 初始化完成，配置文件: {ConfigPath}");
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public static void Load()
    {
        _configs = new Dictionary<string, CharacterConfig>(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (!File.Exists(ConfigPath))
            {
                Logger.Info($"[ModConfigManager] 配置文件不存在，创建默认配置: {ConfigPath}");
                Save();
                return;
            }

            string json = File.ReadAllText(ConfigPath);
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });

            if (doc.RootElement.TryGetProperty("characters", out JsonElement chars))
            {
                foreach (var prop in chars.EnumerateObject())
                {
                    var config = ParseCharacterConfig(prop.Name, prop.Value);
                    _configs[config.CharacterId] = config;
                }
            }

            Logger.Info($"[ModConfigManager] 加载配置成功，共 {_configs.Count} 个角色配置");
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfigManager] 加载配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public static void Save()
    {
        if (_configs == null) _configs = new Dictionary<string, CharacterConfig>();

        try
        {
            var config = new Dictionary<string, object>
            {
                ["_readme"] = "红警2 Mod 配置文件。请勿手动修改。",
                ["version"] = "1.0",
                ["characters"] = _configs.ToDictionary(
                    kv => kv.Key,
                    kv =>                    new
                    {
                        customDeckCardTypes = kv.Value.CustomDeckCardTypes,
                        enableCustomDeck = kv.Value.EnableCustomDeck,
                        baseCarMode = kv.Value.BaseCarMode.ToString(),
                        luckyCrateMode = kv.Value.LuckyCrateMode,
                        cratePoolMode = kv.Value.CratePoolMode.ToString()
                    }
                )
            };

            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(ConfigPath, json);
            Logger.Info($"[ModConfigManager] 配置已保存: {ConfigPath}");
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfigManager] 保存配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定角色的配置
    /// </summary>
    public static CharacterConfig GetCharacterConfig(string characterId)
    {
        if (_configs == null) Load();

        if (_configs!.TryGetValue(characterId, out var config))
        {
            return config;
        }

        // 创建默认配置
        var newConfig = new CharacterConfig { CharacterId = characterId };
        _configs[characterId] = newConfig;
        return newConfig;
    }

    /// <summary>
    /// 获取所有角色配置
    /// </summary>
    public static Dictionary<string, CharacterConfig> GetAllConfigs()
    {
        if (_configs == null) Load();
        return new Dictionary<string, CharacterConfig>(_configs!, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 更新角色配置
    /// </summary>
    public static void UpdateCharacterConfig(CharacterConfig config)
    {
        if (_configs == null) Load();
        _configs![config.CharacterId] = config;
        Save();
    }

    /// <summary>
    /// 重置指定角色为默认配置
    /// </summary>
    public static void ResetCharacterConfig(string characterId)
    {
        if (_configs == null) Load();
        _configs![characterId] = new CharacterConfig { CharacterId = characterId };
        Save();
    }

    /// <summary>
    /// 获取角色的自定义卡组卡牌类型列表
    /// </summary>
    public static List<string> GetCustomDeckCardTypes(string characterId)
    {
        var config = GetCharacterConfig(characterId);
        return config.EnableCustomDeck ? config.CustomDeckCardTypes : new List<string>();
    }

    /// <summary>
    /// 是否启用基地车模式
    /// </summary>
    public static BaseCarMode GetBaseCarMode(string characterId)
    {
        return GetCharacterConfig(characterId).BaseCarMode;
    }

    /// <summary>
    /// 是否启用幸运方块模式
    /// </summary>
    public static bool IsLuckyCrateModeEnabled(string characterId)
    {
        return GetCharacterConfig(characterId).LuckyCrateMode;
    }

    private static CharacterConfig ParseCharacterConfig(string characterId, JsonElement element)
    {
        var config = new CharacterConfig { CharacterId = characterId };

        if (element.TryGetProperty("enableCustomDeck", out var enableCustomDeck))
        {
            config.EnableCustomDeck = enableCustomDeck.GetBoolean();
        }

        if (element.TryGetProperty("customDeckCardTypes", out var cardTypes))
        {
            var list = new List<string>();
            foreach (var item in cardTypes.EnumerateArray())
            {
                string? val = item.GetString();
                if (!string.IsNullOrEmpty(val))
                    list.Add(val);
            }
            config.CustomDeckCardTypes = list;
        }

        if (element.TryGetProperty("baseCarMode", out var baseCarMode))
        {
            string modeStr = baseCarMode.GetString() ?? "None";
            if (Enum.TryParse<BaseCarMode>(modeStr, true, out var mode))
            {
                config.BaseCarMode = mode;
            }
        }

        if (element.TryGetProperty("luckyCrateMode", out var luckyCrate))
        {
            config.LuckyCrateMode = luckyCrate.GetBoolean();
        }

        if (element.TryGetProperty("cratePoolMode", out var cratePoolMode))
        {
            string modeStr = cratePoolMode.GetString() ?? "None";
            if (Enum.TryParse<CratePoolMode>(modeStr, true, out var mode))
            {
                config.CratePoolMode = mode;
            }
        }

        return config;
    }

    private static string GetConfigPath()
    {
        try
        {
            string? location = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(location))
            {
                string? dir = Path.GetDirectoryName(location);
                if (!string.IsNullOrEmpty(dir))
                {
                    string modDir = Path.Combine(dir, "RedAlert2Mod");
                    if (Directory.Exists(modDir))
                    {
                        return Path.Combine(modDir, ConfigFileName);
                    }
                    return Path.Combine(dir, ConfigFileName);
                }
            }
        }
        catch { }

        try
        {
            string? exeDir = Path.GetDirectoryName(OS.GetExecutablePath());
            if (!string.IsNullOrEmpty(exeDir))
                return Path.Combine(exeDir, "mods", "RedAlert2Mod", ConfigFileName);
        }
        catch { }

        return ConfigFileName;
    }
}
