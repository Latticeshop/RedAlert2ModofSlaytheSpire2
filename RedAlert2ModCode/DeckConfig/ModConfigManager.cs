// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.GameActions;
using RedAlert2ModCode.UI;

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
    private BaseCarMode _baseCarMode = BaseCarMode.None;
    private bool _luckyCrateMode;

    /// <summary>
    /// 角色ID
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// 自定义初始卡组（卡牌类型名列表），为空则使用默认
    /// </summary>
    public List<string> CustomDeckCardTypes { get; set; } = new();

    /// <summary>
    /// 是否启用自定义初始遗物
    /// </summary>
    public bool EnableCustomRelics { get; set; }

    /// <summary>
    /// 自定义初始遗物（遗物类型名列表），为空则使用默认
    /// </summary>
    public List<string> StartingRelicTypes { get; set; } = new();

    /// <summary>
    /// 是否启用自定义卡组
    /// </summary>
    public bool EnableCustomDeck { get; set; }

    /// <summary>
    /// 基地车开局模式
    /// </summary>
    public BaseCarMode BaseCarMode
    {
        get => _baseCarMode;
        set
        {
            _baseCarMode = value;
            // 基地车模式与幸运方块模式互斥：启用基地车自动关闭幸运方块。
            if (_baseCarMode != BaseCarMode.None)
            {
                _luckyCrateMode = false;
            }
        }
    }

    /// <summary>
    /// 是否启用幸运方块捡箱子模式
    /// </summary>
    public bool LuckyCrateMode
    {
        get => _luckyCrateMode;
        set
        {
            _luckyCrateMode = value;
            // 与基地车模式互斥：启用幸运方块自动把基地车模式设为“无”。
            if (_luckyCrateMode)
            {
                _baseCarMode = BaseCarMode.None;
            }
        }
    }

    /// <summary>
    /// 卡池奖励模式（None=默认排除箱子，AllCrates=全为箱子，AddCrates=加入箱子）
    /// </summary>
    public CratePoolMode CratePoolMode { get; set; } = CratePoolMode.None;

    /// <summary>
    /// 初始金币数量（0 = 使用角色默认值）
    /// </summary>
    public int StartingGold { get; set; }

    /// <summary>
    /// 初始血量上限（0 = 使用角色默认值）
    /// </summary>
    public int MaxHp { get; set; }

    /// <summary>
    /// 深拷贝当前配置（供方案快照使用，避免引用共享导致方案被后续编辑污染）。
    /// </summary>
    public CharacterConfig Clone()
    {
        var clone = new CharacterConfig
        {
            CharacterId = CharacterId,
            CustomDeckCardTypes = new List<string>(CustomDeckCardTypes),
            EnableCustomDeck = EnableCustomDeck,
            StartingRelicTypes = new List<string>(StartingRelicTypes),
            EnableCustomRelics = EnableCustomRelics,
            CratePoolMode = CratePoolMode,
            StartingGold = StartingGold,
            MaxHp = MaxHp,
        };
        // 基地车/幸运方块互斥：直接按源状态回填私有字段，避免 setter 互相清空
        clone.SetBaseCarState(BaseCarMode, LuckyCrateMode);
        return clone;
    }

    /// <summary>
    /// 直接回填互斥状态（仅供 Clone 使用，绕过 setter 互斥逻辑）。
    /// </summary>
    private void SetBaseCarState(BaseCarMode baseCarMode, bool luckyCrateMode)
    {
        _baseCarMode = baseCarMode;
        _luckyCrateMode = luckyCrateMode;
    }
}

/// <summary>
/// 自定义开局配置方案 - 保存全部角色的配置快照
/// </summary>
public class DeckConfigPreset
{
    /// <summary>
    /// 方案名称（默认“方案N”）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 方案内容：全部角色的配置
    /// </summary>
    public Dictionary<string, CharacterConfig> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Mod配置管理器 - 负责配置的保存、加载和读取
/// </summary>
public static class ModConfigManager
{
    private const string ConfigFileName = "RedAlert2ModConfig.json";
    private const string LocTable = "characters";
    /// <summary>
    /// 自定义卡组条目中升级卡牌的标记后缀（如 "Strike:U"）。
    /// </summary>
    public const string UpgradedMarker = ":U";
    private static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = new("RedAlert2Mod", LogType.Generic);

    /// <summary>
    /// 读取 characters 本地化表中的文案（供卡牌库/遗物库等配置模块 UI 使用）。
    /// </summary>
    public static string L(string key, params object[] args)
    {
        try
        {
            string text = new LocString(LocTable, key).GetRawText();
            if (args.Length > 0)
                text = string.Format(text, args);
            return text;
        }
        catch
        {
            return key;
        }
    }

    /// <summary>
    /// 编码卡组条目：升级卡附加 ":U" 后缀，与未升级同名卡区分（各自独立叠加）。
    /// </summary>
    public static string EncodeCardType(string typeName, bool upgraded)
    {
        return upgraded ? typeName + UpgradedMarker : typeName;
    }

    /// <summary>
    /// 解码卡组条目：去掉升级标记，返回真实卡牌类型名。
    /// </summary>
    public static string DecodeCardType(string entry, out bool upgraded)
    {
        upgraded = false;
        if (!string.IsNullOrEmpty(entry) && entry.EndsWith(UpgradedMarker, StringComparison.Ordinal))
        {
            upgraded = true;
            return entry.Substring(0, entry.Length - UpgradedMarker.Length);
        }
        return entry ?? string.Empty;
    }

    private static Dictionary<string, CharacterConfig>? _configs;
    // 动态方案槽位列表：高亮槽 = 当前方案（工作配置同步回该槽），
    // 末尾始终保持一个 null 空槽用于“保存即新建”，数量不设上限。
    private static readonly List<DeckConfigPreset?> _presets = new();
    // 当前方案槽位索引（切换时高亮跟随，不覆盖任何槽内容）
    private static int _activePresetIndex = 0;
    private static bool _initialized;
    // 多人模式下按玩家 NetId 同步过来的配置（优先级高于本机按角色保存的配置）
    private static readonly Dictionary<ulong, CharacterConfig> _remoteConfigs = new();
    // 当前局玩家列表（由 RunStartPatch 在开局时缓存，供配置广播定位本地玩家）
    private static IReadOnlyList<Player>? _currentPlayers;
    // 是否处于多人联机会话（由大厅初始化补丁设置；多人时开局牌组改为同步后统一应用）
    private static bool _isMultiplayerSession;
    // “强制全部应用房主配置”开关（由房主在大厅切换并广播到各端）
    private static bool _forceHostConfigEnabled;
    // 房主整套配置的临时副本（本局生效，不写入各端配置存储）
    private static Dictionary<string, CharacterConfig>? _forcedHostConfigs;

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
        _presets.Clear();
        _activePresetIndex = 0;

        try
        {
            if (!File.Exists(ConfigPath))
            {
                Logger.Info($"[ModConfigManager] 配置文件不存在，创建默认配置: {ConfigPath}");
                RecoverActiveSlot();
                EnsureTrailingEmptySlot();
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

            if (doc.RootElement.TryGetProperty("presets", out JsonElement presetsEl) &&
                presetsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in presetsEl.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        var preset = new DeckConfigPreset();
                        if (item.TryGetProperty("name", out var nameEl))
                        {
                            preset.Name = nameEl.GetString() ?? string.Empty;
                        }
                        if (item.TryGetProperty("characters", out var charsEl))
                        {
                            foreach (var prop in charsEl.EnumerateObject())
                            {
                                var config = ParseCharacterConfig(prop.Name, prop.Value);
                                preset.Characters[config.CharacterId] = config;
                            }
                        }
                        _presets.Add(preset);
                    }
                }
            }

            if (doc.RootElement.TryGetProperty("activePresetIndex", out JsonElement activeEl) &&
                activeEl.ValueKind == JsonValueKind.Number)
            {
                int idx = activeEl.GetInt32();
                if (idx >= 0) _activePresetIndex = idx;
            }

            // 恢复当前方案槽（越界/指向空槽时自动回退），保证末尾空槽存在
            RecoverActiveSlot();
            // 保证末尾始终有一个空槽（用于“保存即新建”）
            EnsureTrailingEmptySlot();

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
            // 当前方案绑定：工作配置始终同步回高亮槽（名字保留）
            RecoverActiveSlot();
            _presets[_activePresetIndex].Characters = SnapshotConfigs(_configs);
            EnsureTrailingEmptySlot();

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
                        startingRelicTypes = kv.Value.StartingRelicTypes,
                        enableCustomRelics = kv.Value.EnableCustomRelics,
                        baseCarMode = kv.Value.BaseCarMode.ToString(),
                        luckyCrateMode = kv.Value.LuckyCrateMode,
                        cratePoolMode = kv.Value.CratePoolMode.ToString(),
                        startingGold = kv.Value.StartingGold,
                        maxHp = kv.Value.MaxHp
                    }
                ),
                ["presets"] = BuildPresetsJson(),
                ["activePresetIndex"] = _activePresetIndex
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
    public static CharacterConfig GetCharacterConfig(string characterId, ulong? netId = null)
    {
        if (_configs == null) Load();

        // 多人模式：仅远端玩家使用同步过来的配置（本机玩家始终用本机最新配置，
        // 避免局内改配置后广播失败/未同步时读到 _remoteConfigs 里的过期副本）
        if (netId.HasValue && !IsLocalNetId(netId.Value) && _remoteConfigs.TryGetValue(netId.Value, out var remoteConfig))
        {
            return remoteConfig;
        }

        if (_configs!.TryGetValue(characterId, out var config))
        {
            return config;
        }

        // 创建默认配置
        var newConfig = new CharacterConfig { CharacterId = characterId };
        _configs[characterId] = newConfig;
        return newConfig;
    }

    private static bool IsLocalNetId(ulong netId)
    {
        try
        {
            return RunManager.Instance?.NetService != null && RunManager.Instance.NetService.NetId == netId;
        }
        catch { return false; }
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
        BroadcastConfig(config);
    }

    /// <summary>
    /// 记录其他玩家（NetId）同步过来的配置，用于多人模式下按玩家独立应用。
    /// </summary>
    public static void SetRemoteCharacterConfig(ulong netId, CharacterConfig config)
    {
        if (config == null) return;
        _remoteConfigs[netId] = config;
        Logger.Info($"[ModConfigManager] 已记录玩家 {netId} 的配置（角色: {config.CharacterId}）");
    }

    /// <summary>
    /// 缓存当前局玩家列表（开局时由 RunStartPatch 调用）。
    /// </summary>
    public static void SetRunPlayers(IReadOnlyList<Player>? players)
    {
        _currentPlayers = players;
    }

    /// <summary>
    /// 是否处于多人联机会话（大厅阶段即置位，离开大厅后复位）。
    /// </summary>
    public static bool IsMultiplayerSession => _isMultiplayerSession;

    /// <summary>
    /// 设置多人联机会话标记（由大厅初始化/清理补丁调用）。
    /// </summary>
    public static void SetMultiplayerSession(bool value)
    {
        _isMultiplayerSession = value;
        if (!value)
        {
            _remoteConfigs.Clear();
            _currentPlayers = null;
            _forceHostConfigEnabled = false;
            _forcedHostConfigs = null;
        }
    }

    /// <summary>
    /// 仅复位多人会话标记，不清空同步配置/强制配置。
    /// 用于开局应用结束后复位 InitialDeckPatch 的跳过标记，
    /// 同时保留局内奖励/阵营补丁所需的各玩家同步配置。
    /// </summary>
    public static void ResetMultiplayerSessionFlag()
    {
        _isMultiplayerSession = false;
    }

    /// <summary>
    /// 清空房主整套配置的临时副本（每次开局前调用，避免上一局残留被误用）。
    /// </summary>
    public static void ClearForcedHostConfigs()
    {
        _forcedHostConfigs = null;
    }

    /// <summary>
    /// 复位强制房主配置开关与临时副本（每次进入新大厅时调用，默认关闭）。
    /// </summary>
    public static void ResetForceConfigState()
    {
        _forceHostConfigEnabled = false;
        _forcedHostConfigs = null;
    }

    /// <summary>
    /// “强制全部应用房主配置”开关状态。
    /// </summary>
    public static bool IsForceHostConfigEnabled => _forceHostConfigEnabled;

    /// <summary>
    /// 设置强制房主配置开关（大厅面板/消息调用；不写入配置文件）。
    /// </summary>
    public static void SetForceHostConfig(bool enabled)
    {
        if (_forceHostConfigEnabled == enabled) return;
        _forceHostConfigEnabled = enabled;
        Logger.Info($"[ModConfigManager] 强制全部应用房主配置: {(enabled ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 保存房主整套配置的本局临时副本（由开局时的同步动作在所有端执行）。
    /// </summary>
    public static void SetForcedHostConfigs(Dictionary<string, CharacterConfig>? configs)
    {
        _forcedHostConfigs = configs == null
            ? null
            : new Dictionary<string, CharacterConfig>(configs, StringComparer.OrdinalIgnoreCase);
        Logger.Info($"[ModConfigManager] 已接收房主整套配置（{_forcedHostConfigs?.Count ?? 0} 个角色）");
    }

    /// <summary>
    /// 强制模式下是否已收到房主整套配置（房主可能一个角色都没配置，即全默认，也算就绪）。
    /// </summary>
    public static bool HasForcedHostConfigs()
    {
        return _forceHostConfigEnabled && _forcedHostConfigs != null;
    }

    /// <summary>
    /// 尝试获取房主对指定角色的强制配置。
    /// </summary>
    public static bool TryGetForcedConfig(string characterId, out CharacterConfig config)
    {
        if (_forcedHostConfigs != null && _forcedHostConfigs.TryGetValue(characterId, out var forced))
        {
            config = forced;
            return true;
        }
        config = null!;
        return false;
    }

    /// <summary>
    /// 获取玩家本局应使用的配置：
    /// 强制房主配置开启时优先使用房主对对应角色的配置（房主未配置的角色强制默认开局）；
    /// 否则本机玩家用本机配置，远端玩家用同步配置。
    /// </summary>
    public static CharacterConfig? GetConfigForPlayer(Player player)
    {
        try
        {
            if (player?.Character == null) return null;
            string? characterId = player.Character?.Id?.Entry;
            if (string.IsNullOrEmpty(characterId)) return null;

            // 强制模式只在多人游戏运行中生效（RunManager.NetService 为 Host/Client）；
            // 单机局即使残留开关也不应用，避免误用上一局房主配置
            if (_forceHostConfigEnabled && MultiplayerSyncHelper.IsMultiplayerGame())
            {
                if (TryGetForcedConfig(characterId, out var forced))
                {
                    return forced;
                }
                // 房主未配置该角色：本局按默认开局（与房主行为一致），不采用玩家自身配置
                return new CharacterConfig { CharacterId = characterId };
            }

            return MultiplayerSyncHelper.IsLocalPlayer(player)
                ? GetCharacterConfig(characterId)
                : GetCharacterConfig(characterId, player.NetId);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 是否已收集到所有远端玩家的配置（本地玩家始终可用本机配置）。
    /// </summary>
    public static bool HasConfigForAllPlayers(IReadOnlyList<Player> players)
    {
        if (players == null) return false;
        foreach (var player in players)
        {
            try
            {
                if (player == null) continue;
                if (MultiplayerSyncHelper.IsLocalPlayer(player)) continue;
                if (!_remoteConfigs.ContainsKey(player.NetId)) return false;
            }
            catch { return false; }
        }
        return true;
    }

    /// <summary>
    /// 单个玩家配置是否就绪（本地玩家始终可用本机配置，远端玩家需等同步到达）。
    /// </summary>
    public static bool HasConfigForPlayer(Player player)
    {
        if (player == null) return false;
        try
        {
            if (MultiplayerSyncHelper.IsLocalPlayer(player)) return true;
            return _remoteConfigs.ContainsKey(player.NetId);
        }
        catch { return false; }
    }

    /// <summary>
    /// 多人开局时：把本机所有本地玩家的配置广播给主机。
    /// </summary>
    public static void BroadcastAllLocalConfigs()
    {
        try
        {
            if (!MultiplayerSyncHelper.IsMultiplayerGame()) return;
            if (_currentPlayers == null) return;
            foreach (var player in _currentPlayers)
            {
                try
                {
                    if (!MultiplayerSyncHelper.IsLocalPlayer(player)) continue;
                    string? characterId = player.Character?.Id?.Entry;
                    if (string.IsNullOrEmpty(characterId)) continue;
                    BroadcastConfig(GetCharacterConfig(characterId));
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfigManager] 广播本地配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将本地配置广播给主机（多人模式下）；不在多人运行环境中时静默跳过。
    /// </summary>
    public static void BroadcastConfig(CharacterConfig config)
    {
        try
        {
            if (config == null) return;
            if (!MultiplayerSyncHelper.IsMultiplayerGame()) return;
            if (_currentPlayers == null) return;

            Player? local = _currentPlayers.FirstOrDefault(p =>
            {
                try { return MultiplayerSyncHelper.IsLocalPlayer(p) && p.Character?.Id?.Entry == config.CharacterId; }
                catch { return false; }
            });
            if (local == null) return;

            RunManager.Instance?.ActionQueueSynchronizer?.RequestEnqueue(new ConfigSyncGameAction(local, config));
            Logger.Info($"[ModConfigManager] 已广播配置（角色: {config.CharacterId}）");
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfigManager] 广播配置失败: {ex.Message}");
        }
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
    /// 当前工作配置最近一次保存/加载的方案槽位（-1 = 未关联任何方案）。
    /// </summary>
    /// <summary>
    /// 当前方案槽位索引（约定恒为 0：当前方案槽始终存在并绑定工作配置）。
    /// </summary>
    /// <summary>
    /// 当前方案槽位索引：高亮跟随当前方案，切换只改高亮、不覆盖任何槽内容。
    /// </summary>
    public static int ActivePresetIndex => _activePresetIndex;

    /// <summary>
    /// 当前槽位总数（含末尾空槽）。
    /// </summary>
    public static int GetPresetCount()
    {
        return _presets.Count;
    }

    /// <summary>
    /// 获取指定槽位的方案（null = 空位）。
    /// </summary>
    public static DeckConfigPreset? GetPreset(int index)
    {
        return index >= 0 && index < _presets.Count ? _presets[index] : null;
    }

    /// <summary>
    /// 指定槽位是否已保存过方案（有角色配置内容）。
    /// </summary>
    public static bool HasPreset(int index)
    {
        var preset = GetPreset(index);
        return preset != null && preset.Characters.Count > 0;
    }

    /// <summary>
    /// 将当前全部角色配置快照保存到指定槽位：
    /// 空槽 = 新建方案（自动追加新的末尾空槽）；已保存槽 = 覆盖内容（保留名字）；当前槽 = 重写快照。
    /// </summary>
    public static void SavePreset(int index)
    {
        if (index < 0 || index >= _presets.Count) return;
        if (_configs == null) Load();

        var snapshot = BuildFullConfigSnapshot();
        if (index == _activePresetIndex)
        {
            // 当前方案槽：重写内容，保留名字
            _presets[index].Characters = snapshot;
            Logger.Info($"[ModConfigManager] 已保存当前方案（{snapshot.Count} 个角色）");
        }
        else
        {
            var existing = _presets[index];
            if (existing == null || existing.Characters.Count == 0)
            {
                // 空槽：新建方案（保留已命名名字；未命名则自动生成“方案N”）
                var newPreset = new DeckConfigPreset
                {
                    Name = existing?.Name ?? GenerateNextPresetName(),
                    Characters = snapshot,
                };
                _presets[index] = newPreset;
                Logger.Info($"[ModConfigManager] 已新建方案（{snapshot.Count} 个角色）");
            }
            else
            {
                existing.Characters = snapshot;
                Logger.Info($"[ModConfigManager] 已保存方案（{snapshot.Count} 个角色）");
            }
        }
        Save();
    }

    /// <summary>
    /// 重命名指定槽位的方案。
    /// </summary>
    public static void RenamePreset(int index, string name)
    {
        if (index < 0 || index >= _presets.Count) return;
        var preset = _presets[index];
        if (preset == null)
        {
            preset = new DeckConfigPreset();
            _presets[index] = preset;
        }
        preset.Name = string.IsNullOrWhiteSpace(name) ? GenerateNextPresetName() : name.Trim();
        Save();
        Logger.Info($"[ModConfigManager] 已重命名方案 {index}: {preset.Name}");
    }

    /// <summary>
    /// 对配置字典做深拷贝快照（供方案绑定同步与加载使用）。
    /// </summary>
    private static Dictionary<string, CharacterConfig> SnapshotConfigs(Dictionary<string, CharacterConfig> source)
    {
        var result = new Dictionary<string, CharacterConfig>(StringComparer.OrdinalIgnoreCase);
        if (source == null) return result;
        foreach (var kv in source)
        {
            result[kv.Key] = kv.Value.Clone();
        }
        return result;
    }

    /// <summary>
    /// 生成 presets 数组的 JSON 载荷：当前槽（index 0）始终序列化（即使为空），
    /// 已保存/已命名槽序列化；未命名空槽不序列化（加载时自动重建）。
    /// </summary>
    private static List<object?> BuildPresetsJson()
    {
        var result = new List<object?>();
        foreach (var preset in _presets)
        {
            if (preset == null) continue;

            var characters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in preset.Characters)
            {
                characters[kv.Key] = new
                {
                    customDeckCardTypes = kv.Value.CustomDeckCardTypes,
                    enableCustomDeck = kv.Value.EnableCustomDeck,
                    startingRelicTypes = kv.Value.StartingRelicTypes,
                    enableCustomRelics = kv.Value.EnableCustomRelics,
                    baseCarMode = kv.Value.BaseCarMode.ToString(),
                    luckyCrateMode = kv.Value.LuckyCrateMode,
                    cratePoolMode = kv.Value.CratePoolMode.ToString(),
                    startingGold = kv.Value.StartingGold,
                    maxHp = kv.Value.MaxHp,
                };
            }

            result.Add(new Dictionary<string, object>
            {
                ["name"] = preset.Name,
                ["characters"] = characters,
            });
        }
        return result;
    }

    /// <summary>
    /// 生成“方案N”默认名（N = 当前已保存槽数量 + 1，不含当前槽与空槽）。
    /// </summary>
    private static string GenerateNextPresetName()
    {
        int savedCount = 0;
        for (int i = 0; i < _presets.Count; i++)
        {
            if (i == _activePresetIndex) continue;
            var p = _presets[i];
            if (p != null && p.Characters.Count > 0) savedCount++;
        }
        return L("CONFIG_PRESET_SLOT", savedCount + 1);
    }

    /// <summary>
    /// 物化全部角色的配置快照（未配置角色自动补默认项）。
    /// </summary>
    private static Dictionary<string, CharacterConfig> BuildFullConfigSnapshot()
    {
        var snapshot = new Dictionary<string, CharacterConfig>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var character in ModelDb.AllCharacters)
            {
                try
                {
                    string id = character.Id.Entry;
                    if (string.IsNullOrEmpty(id)) continue;
                    snapshot[id] = GetCharacterConfig(id).Clone();
                }
                catch { }
            }
        }
        catch { }
        // 兜底：ModelDb 不可用时至少保存当前已配置角色
        if (snapshot.Count == 0 && _configs != null)
        {
            foreach (var kv in _configs)
            {
                snapshot[kv.Key] = kv.Value.Clone();
            }
        }
        return snapshot;
    }

    /// <summary>
    /// 保证槽位列表末尾始终有一个空槽（用于“保存即新建”）。
    /// </summary>
    private static void EnsureTrailingEmptySlot()
    {
        if (_presets.Count == 0)
        {
            _presets.Add(null);
            return;
        }
        int lastIdx = _presets.Count - 1;
        var last = _presets[lastIdx];
        if (last == null) return;
        // 当前方案槽不充当“新建”空槽；末尾有内容的槽也需补空槽
        if (lastIdx == _activePresetIndex || last.Characters.Count > 0)
        {
            _presets.Add(null);
        }
    }

    /// <summary>
    /// 恢复当前方案槽：越界或指向空槽时回退到最近的槽，全部为空则新建当前槽。
    /// </summary>
    private static void RecoverActiveSlot()
    {
        if (_presets.Count == 0)
        {
            _presets.Add(new DeckConfigPreset { Name = L("CONFIG_PRESET_CURRENT_DEFAULT") });
            _activePresetIndex = 0;
            return;
        }
        if (_activePresetIndex >= _presets.Count) _activePresetIndex = _presets.Count - 1;
        if (_activePresetIndex < 0) _activePresetIndex = 0;
        if (_presets[_activePresetIndex] != null) return;

        // 向左找最近的非空槽
        for (int j = _activePresetIndex - 1; j >= 0; j--)
        {
            if (_presets[j] != null) { _activePresetIndex = j; return; }
        }
        for (int j = _activePresetIndex + 1; j < _presets.Count; j++)
        {
            if (_presets[j] != null) { _activePresetIndex = j; return; }
        }
        // 全部为空：把 active 位置的 null 换成当前方案槽
        _presets[_activePresetIndex] = new DeckConfigPreset { Name = L("CONFIG_PRESET_CURRENT_DEFAULT") };
    }

    /// <summary>
    /// 加载指定槽位的方案为当前方案：只移动高亮（active index），不覆盖任何槽内容。
    /// </summary>
    public static bool LoadPreset(int index)
    {
        if (index < 0 || index >= _presets.Count) return false;
        var preset = _presets[index];
        if (preset == null || preset.Characters.Count == 0) return false;

        _configs = SnapshotConfigs(preset.Characters);
        _activePresetIndex = index;
        Save();
        // 多人大厅中切换方案后立即把本机各角色配置重新广播，避免旧配置残留
        BroadcastAllLocalConfigs();
        Logger.Info($"[ModConfigManager] 已切换到方案 {index}（{_configs.Count} 个角色）");
        return true;
    }

    /// <summary>
    /// 删除指定槽位的方案（可删当前槽，删除后高亮自动回退到最近槽；弹窗确认由 UI 负责）。
    /// </summary>
    public static bool DeletePreset(int index)
    {
        if (index < 0 || index >= _presets.Count) return false;
        if (_presets[index] == null) return false;
        _presets.RemoveAt(index);
        RecoverActiveSlot();
        EnsureTrailingEmptySlot();
        Save();
        Logger.Info($"[ModConfigManager] 已删除方案 {index}");
        return true;
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

        if (element.TryGetProperty("startingRelicTypes", out var relicTypes))
        {
            var list = new List<string>();
            foreach (var item in relicTypes.EnumerateArray())
            {
                string? val = item.GetString();
                if (!string.IsNullOrEmpty(val))
                    list.Add(val);
            }
            config.StartingRelicTypes = list;
        }

        if (element.TryGetProperty("enableCustomRelics", out var enableCustomRelics))
        {
            config.EnableCustomRelics = enableCustomRelics.GetBoolean();
        }

        // 先解析幸运方块，再解析基地车：若旧配置两者同时启用，基地车模式优先（并自动关闭幸运方块）。
        if (element.TryGetProperty("luckyCrateMode", out var luckyCrate))
        {
            config.LuckyCrateMode = luckyCrate.GetBoolean();
        }

        if (element.TryGetProperty("baseCarMode", out var baseCarMode))
        {
            string modeStr = baseCarMode.GetString() ?? "None";
            if (Enum.TryParse<BaseCarMode>(modeStr, true, out var mode))
            {
                config.BaseCarMode = mode;
            }
        }

        if (element.TryGetProperty("cratePoolMode", out var cratePoolMode))
        {
            string modeStr = cratePoolMode.GetString() ?? "None";
            if (Enum.TryParse<CratePoolMode>(modeStr, true, out var mode))
            {
                config.CratePoolMode = mode;
            }
        }

        if (element.TryGetProperty("startingGold", out var startingGold))
        {
            config.StartingGold = startingGold.GetInt32();
        }

        if (element.TryGetProperty("maxHp", out var maxHp))
        {
            config.MaxHp = maxHp.GetInt32();
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
