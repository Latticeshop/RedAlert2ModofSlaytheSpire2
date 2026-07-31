// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.DeckConfig;

/// <summary>
/// Mod配置补丁 - 在游戏流程中应用mod配置
/// </summary>
public static class ModConfigPatches
{
    private static bool _configApplied;
    private static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = new("ModConfigPatches", MegaCrit.Sts2.Core.Logging.LogType.Generic);

    private const string MenuButtonName = "RedAlert2ModConfigButton";
    private const int NativeDuplicateFlags = 14;
    private const string LocTable = "characters";

    private static readonly Type? MainMenuType = FindType("NMainMenu", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu");
    private static readonly Type? MainMenuTextButtonType = FindType("NMainMenuTextButton", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu");
    private static readonly Type? ClickableControlType = FindType("NClickableControl", "MegaCrit.Sts2.Core.Nodes");
    private static readonly Type? SignalNameType = FindSignalNameType();
    private static readonly string? FocusedSignalName = GetSignalName("Focused");
    private static readonly string? UnfocusedSignalName = GetSignalName("Unfocused");
    private static readonly string ReleasedSignalName = GetSignalName("Released") ?? "pressed";
    private static readonly FieldInfo? LastHitButtonField = MainMenuType != null
        ? AccessTools.Field(MainMenuType, "_lastHitButton")
        : null;
    private static readonly FieldInfo? LocStringField = MainMenuTextButtonType != null
        ? AccessTools.Field(MainMenuTextButtonType, "_locString")
        : null;
    private static readonly MethodInfo? ButtonFocusedMethod = MainMenuType != null && MainMenuTextButtonType != null
        ? AccessTools.Method(MainMenuType, "MainMenuButtonFocused", new[] { MainMenuTextButtonType! })
        : null;
    private static readonly MethodInfo? ButtonUnfocusedMethod = MainMenuType != null && MainMenuTextButtonType != null
        ? AccessTools.Method(MainMenuType, "MainMenuButtonUnfocused", new[] { MainMenuTextButtonType! })
        : null;

    private static Type? FindSignalNameType()
    {
        if (ClickableControlType == null) return null;
        return ClickableControlType.GetNestedTypes()
            .FirstOrDefault(t => t.Name == "SignalName");
    }

    private static string? GetSignalName(string fieldName)
    {
        if (SignalNameType == null) return null;
        var field = SignalNameType.GetField(fieldName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field == null) return null;
        var value = field.GetValue(null);
        if (value is string str) return str;
        if (value is StringName sn) return sn.ToString();
        return null;
    }

    private static string L(string key, params object[] args)
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
    /// 安装所有配置补丁
    /// </summary>
    public static void Install(HarmonyLib.Harmony harmony)
    {
        // 补丁1: 拦截初始卡组创建
        try
        {
            var createInitialDeckMethod = AccessTools.Method(typeof(RunState), "CreateInitialDeckCards");
            if (createInitialDeckMethod != null)
            {
                harmony.Patch(
                    original: createInitialDeckMethod,
                    prefix: new HarmonyMethod(typeof(InitialDeckPatch), nameof(InitialDeckPatch.Prefix)),
                    postfix: new HarmonyMethod(typeof(InitialDeckPatch), nameof(InitialDeckPatch.Postfix))
                );
                Logger.Info("[ModConfig] 初始卡组补丁安装成功");
            }
            else
            {
                Logger.Warn("[ModConfig] 找不到 CreateInitialDeckCards 方法");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] 初始卡组补丁安装失败: {ex.Message}");
        }

        // 补丁2: 在主菜单添加入口按钮（参考海克斯符文mod实现方式）
        try
        {
            if (MainMenuType != null)
            {
                var readyMethod = AccessTools.Method(MainMenuType, "_Ready");
                if (readyMethod != null)
                {
                    harmony.Patch(
                        original: readyMethod,
                        postfix: new HarmonyMethod(typeof(MainMenuPatch), nameof(MainMenuPatch.Postfix))
                    );
                    Logger.Info("[ModConfig] 主菜单补丁安装成功");
                }
            }
            else
            {
                Logger.Warn("[ModConfig] 找不到 NMainMenu 类型");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] 主菜单补丁安装失败: {ex.Message}");
        }

        // 补丁3: 拦截Run开始时应用基地车模式
        try
        {
            var startRunMethod = AccessTools.Method(typeof(RunManager), "StartRun");
            if (startRunMethod != null)
            {
                harmony.Patch(
                    original: startRunMethod,
                    postfix: new HarmonyMethod(typeof(RunStartPatch), nameof(RunStartPatch.Postfix))
                );
                Logger.Info("[ModConfig] Run开始补丁安装成功");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] Run开始补丁安装失败: {ex.Message}");
        }

        // 补丁4: 拦截FlagManager.GetPlayerFaction以支持MCV模式国旗事件
        try
        {
            var getFactionMethod = AccessTools.Method(typeof(FlagManager), "GetPlayerFaction");
            if (getFactionMethod != null)
            {
                harmony.Patch(
                    original: getFactionMethod,
                    postfix: new HarmonyMethod(typeof(FactionPatch), nameof(FactionPatch.Postfix))
                );
                Logger.Info("[ModConfig] MCV模式国旗补丁安装成功");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] MCV模式国旗补丁安装失败: {ex.Message}");
        }

        Logger.Info("[ModConfigPatches] 配置补丁安装完成");
    }

    private static Type? FindType(string name, string? ns = null)
    {
        if (ns != null)
        {
            var type = Type.GetType($"{ns}.{name}, sts2");
            if (type != null)
            {
                Logger.Info($"[ModConfig] FindType: Found {ns}.{name} via Assembly.GetType");
                return type;
            }
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetType(name);
                if (type != null)
                {
                    Logger.Info($"[ModConfig] FindType: Found {name} in {asm.GetName().Name} (exact)");
                    return type;
                }
            }
            catch { }

            if (ns != null)
            {
                try
                {
                    var type = asm.GetType($"{ns}.{name}");
                    if (type != null)
                    {
                        Logger.Info($"[ModConfig] FindType: Found {ns}.{name} in {asm.GetName().Name}");
                        return type;
                    }
                }
                catch { }
            }
        }

        // Fallback: search all types by short name
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetTypes().FirstOrDefault(t => t.Name == name);
                if (type != null)
                {
                    Logger.Info($"[ModConfig] FindType: Found {name} via short name in {asm.GetName().Name} ({type.FullName})");
                    return type;
                }
            }
            catch { }
        }

        Logger.Warn($"[ModConfig] FindType: Type {name} not found{(ns != null ? $" in namespace {ns}" : "")}");
        return null;
    }

    /// <summary>
    /// 初始卡组补丁 - 允许自定义初始卡组
    /// </summary>
    public static class InitialDeckPatch
    {
        private static readonly FieldInfo? PlayerField =
            AccessTools.Field(typeof(RunState), "_player");

        public static void Prefix(RunState __instance, ref List<CardModel> __state)
        {
            try
            {
                var player = PlayerField?.GetValue(__instance) as Player;
                if (player?.Character == null) return;

                string? characterId = player.Character?.Id?.Entry;
                if (string.IsNullOrEmpty(characterId)) return;

                var config = ModConfigManager.GetCharacterConfig(characterId);

                // 幸运方块模式
                if (config.LuckyCrateMode)
                {
                    var luckyDeck = CreateLuckyCrateDeck();
                    __state = luckyDeck;
                    Logger.Info($"[ModConfig] 已应用幸运方块模式，角色: {characterId}");
                    return;
                }

                // 自定义卡组
                if (config.EnableCustomDeck && config.CustomDeckCardTypes.Count > 0)
                {
                    var customDeck = CreateCustomDeck(config);
                    if (customDeck.Count > 0)
                    {
                        __state = customDeck;
                        Logger.Info($"[ModConfig] 已应用自定义卡组，角色: {characterId}, 卡牌数: {customDeck.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] InitialDeckPatch.Prefix 失败: {ex.Message}");
            }
        }

        public static void Postfix(RunState __instance)
        {
            try
            {
                var player = PlayerField?.GetValue(__instance) as Player;
                if (player?.Character == null) return;

                string? characterId = player.Character?.Id?.Entry;
                if (string.IsNullOrEmpty(characterId)) return;

                var config = ModConfigManager.GetCharacterConfig(characterId);

                // 基地车模式 - 在初始卡组中添加基地车
                if (config.BaseCarMode != BaseCarMode.None)
                {
                    ApplyBaseCarMode(__instance, config);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] InitialDeckPatch.Postfix 失败: {ex.Message}");
            }
        }

        private static List<CardModel> CreateCustomDeck(CharacterConfig config)
        {
            var deck = new List<CardModel>();
            var cardCounts = new Dictionary<string, int>();

            foreach (var cardTypeName in config.CustomDeckCardTypes)
            {
                cardCounts.TryGetValue(cardTypeName, out int count);
                cardCounts[cardTypeName] = count + 1;
            }

            foreach (var (cardTypeName, count) in cardCounts)
            {
                try
                {
                    var cardType = FindCardType(cardTypeName);
                    if (cardType != null)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var cardModel = GetCardModel(cardType);
                            if (cardModel != null)
                            {
                                deck.Add(cardModel.ToMutable());
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn($"[ModConfig] 找不到卡牌类型: {cardTypeName}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[ModConfig] 创建卡牌失败 {cardTypeName}: {ex.Message}");
                }
            }

            return deck;
        }

        private static Type? FindCardType(string typeName)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "RedAlert2Mod");
            if (asm != null)
            {
                var type = asm.GetType(typeName);
                if (type != null) return type;

                foreach (var ns in new[] { "RedAlert2ModCode.Allies.Cards", "RedAlert2ModCode.Soviet.Cards", "RedAlert2ModCode.Common.Cards" })
                {
                    type = asm.GetType($"{ns}.{typeName}");
                    if (type != null) return type;
                }
            }

            return null;
        }

        private static CardModel? GetCardModel(Type cardType)
        {
            try
            {
                // 使用反射调用泛型方法 ModelDb.Card<T>()
                var cardMethod = typeof(ModelDb).GetMethods()
                    .FirstOrDefault(m => m.Name == "Card" && m.IsGenericMethodDefinition);

                if (cardMethod != null)
                {
                    var genericMethod = cardMethod.MakeGenericMethod(cardType);
                    return genericMethod.Invoke(null, null) as CardModel;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ModConfig] 获取卡牌模型失败: {ex.Message}");
            }
            return null;
        }

        private static List<CardModel> CreateLuckyCrateDeck()
        {
            var deck = new List<CardModel>();
            var crateNames = new[]
            {
                "OreCrate", "MoneyCrate", "HealCrate", "ArmorCrate",
                "StealthCrate", "SpeedCrate", "UpgradeCrate",
                "ExplosionCrate", "SuperWeaponCrate", "VehicleCrate",
                "FirepowerCrate", "RandomCrate",
            };

            foreach (var name in crateNames)
            {
                try
                {
                    var cardType = FindCardType(name);
                    if (cardType != null)
                    {
                        var cardModel = GetCardModel(cardType);
                        if (cardModel != null)
                        {
                            deck.Add(cardModel.ToMutable());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[ModConfig] 创建箱子卡失败 {name}: {ex.Message}");
                }
            }

            return deck;
        }

        private static void ApplyBaseCarMode(RunState runState, CharacterConfig config)
        {
            try
            {
                string mcvName = config.BaseCarMode switch
                {
                    BaseCarMode.Allied => "AlliedMCV",
                    BaseCarMode.Soviet => "SovietMCV",
                    BaseCarMode.Yuri => "YuriMCV",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(mcvName)) return;

                var mcvType = FindCardType(mcvName);
                if (mcvType != null)
                {
                    var mcvCard = GetCardModel(mcvType);
                    if (mcvCard != null)
                    {
                        var mcvInstance = mcvCard.ToMutable();

                        // 使用反射获取 Deck 或相关属性
                        var deckProp = typeof(RunState).GetProperty("Deck");
                        if (deckProp != null)
                        {
                            var deck = deckProp.GetValue(runState) as IList<CardModel>;
                            deck?.Add(mcvInstance);
                            Logger.Info($"[ModConfig] 基地车模式: 已添加 {mcvName} 到卡组");
                        }
                        else
                        {
                            var deckField = typeof(RunState).GetField("_deck", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (deckField != null)
                            {
                                var deck = deckField.GetValue(runState) as IList<CardModel>;
                                deck?.Add(mcvInstance);
                                Logger.Info($"[ModConfig] 基地车模式: 已添加 {mcvName} 到卡组 (通过字段)");
                            }
                        }
                    }
                }
                else
                {
                    Logger.Warn($"[ModConfig] 找不到基地车类型: {mcvName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] ApplyBaseCarMode 失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 主菜单补丁 - 在主菜单添加入口按钮（参考海克斯符文mod实现）
    /// </summary>
    public static class MainMenuPatch
    {
        private const int MaxAttachAttempts = 30;

        public static void Postfix(Node __instance)
        {
            if (MainMenuType == null) return;

            // 检查是否已添加过
            if (__instance.FindChild(MenuButtonName, recursive: true, owned: false) != null)
                return;

            // 使用TaskHelper异步等待UI完全初始化
            TaskHelper.RunSafely(AttachButtonWhenReadyAsync(__instance));
        }

        private static async Task AttachButtonWhenReadyAsync(Node mainMenu)
        {
            for (int attempt = 1; attempt <= MaxAttachAttempts; attempt++)
            {
                if (!GodotObject.IsInstanceValid(mainMenu))
                    return;

                try
                {
                    if (TryAttachButton(mainMenu))
                        return;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[ModConfig] 主菜单按钮安装失败: {ex.Message}");
                    return;
                }

                if (!await AwaitProcessFrameAsync(mainMenu))
                    return;
            }

            Logger.Warn("[ModConfig] 主菜单按钮安装超时");
        }

        private static bool TryAttachButton(Node host)
        {
            // 检查是否已存在
            if (host.FindChild(MenuButtonName, recursive: true, owned: false) is Node existing
                && GodotObject.IsInstanceValid(existing))
                return true;

            if (TryAttachNativeMenuButton(host))
            {
                Logger.Info("[ModConfig] 主菜单配置按钮已添加");
                return true;
            }

            Logger.Warn("[ModConfig] 主菜单按钮安装失败：找不到原生菜单按钮");
            return false;
        }

        private static bool TryAttachNativeMenuButton(Node mainMenu)
        {
            if (MainMenuTextButtonType == null || LocStringField == null)
                return false;

            // 查找主菜单按钮容器
            var buttonHost = mainMenu.GetNodeOrNull<Control>("MainMenuTextButtons");
            if (buttonHost == null)
            {
                // 尝试查找按钮容器
                buttonHost = TryFindButtonContainer(mainMenu);
                if (buttonHost == null) return false;
            }

            // 查找SettingsButton作为模板
            Node? settingsButton = null;
            if (mainMenu.GetNodeOrNull("MainMenuTextButtons/SettingsButton") is Node btn)
            {
                settingsButton = btn;
            }

            // 如果找不到SettingsButton，用容器中第一个按钮作为模板
            if (settingsButton == null && buttonHost != null)
            {
                settingsButton = buttonHost.GetChildren()
                    .FirstOrDefault(c => MainMenuTextButtonType.IsInstanceOfType(c));
            }

            if (settingsButton == null)
            {
                Logger.Warn("[ModConfig] 找不到按钮模板");
                return false;
            }

            // 使用Duplicate复制按钮
            var configBtn = settingsButton.Duplicate(NativeDuplicateFlags);
            ((Node)configBtn).Name = MenuButtonName;
            ((Node)configBtn).UniqueNameInOwner = true;

            // 添加到容器
            buttonHost.AddChild(configBtn);
            buttonHost.MoveChild(configBtn, Math.Min(settingsButton.GetIndex() + 1, buttonHost.GetChildCount() - 1));

            // 配置标签（设置本地化文本）
            ConfigureNativeMenuLabel(configBtn);

            // 配置按钮属性
            ConfigureNativeMenuButton(configBtn, settingsButton);

            // 配置焦点事件
            ConfigureNativeMenuFocus(mainMenu, configBtn);

            // 连接点击事件
            ConnectNativeMenuButton(configBtn);

            return true;
        }

        private static void ConfigureNativeMenuLabel(Node configButton)
        {
            // 清除locString以使用自定义文本
            LocStringField?.SetValue(configButton, null);

            if (configButton.GetChildCount() > 0 && configButton.GetChild(0) is Label label)
            {
                label.Text = L("MAIN_MENU_CONFIG_BUTTON");
                label.PivotOffset = label.Size * 0.5f;
            }

            if (configButton is Control ctrl)
            {
                ctrl.TooltipText = L("MAIN_MENU_CONFIG_BUTTON_TOOLTIP");
            }
        }

        private static void ConfigureNativeMenuButton(Node configButton, Node template)
        {
            if (configButton is Control control && template is Control templateCtrl)
            {
                control.MouseFilter = Control.MouseFilterEnum.Stop;
                control.FocusMode = Control.FocusModeEnum.All;
                control.MouseDefaultCursorShape = templateCtrl.MouseDefaultCursorShape;
                control.SizeFlagsHorizontal = templateCtrl.SizeFlagsHorizontal;
                control.SizeFlagsVertical = templateCtrl.SizeFlagsVertical;
                control.CustomMinimumSize = templateCtrl.CustomMinimumSize;
                control.ZIndex = templateCtrl.ZIndex;
                control.ZAsRelative = templateCtrl.ZAsRelative;
            }
        }

        private static void ConfigureNativeMenuFocus(Node mainMenu, Node configButton)
        {
            if (ButtonFocusedMethod != null && FocusedSignalName != null)
            {
                ((GodotObject)configButton).Connect(
                    FocusedSignalName,
                    Callable.From(() =>
                    {
                        ButtonFocusedMethod.Invoke(mainMenu, new[] { configButton });
                    }));
            }

            if (ButtonUnfocusedMethod != null && UnfocusedSignalName != null)
            {
                ((GodotObject)configButton).Connect(
                    UnfocusedSignalName,
                    Callable.From(() =>
                    {
                        ButtonUnfocusedMethod.Invoke(mainMenu, new[] { configButton });
                    }));
            }
        }

        private static void ConnectNativeMenuButton(Node configButton)
        {
            // Match Hextech pattern exactly: NClickableControl.SignalName.Released + Callable.From<NButton>
            // The Released signal expects an NButton parameter; using a parameterless callable
            // causes Godot to silently skip the callback in this game's signal system.
            try
            {
                ((GodotObject)configButton).Connect(
                    NClickableControl.SignalName.Released,
                    Callable.From<NButton>(_ =>
                    {
                        Logger.Info("[ModConfig] Config button clicked (Released)");

                        if (LastHitButtonField != null)
                        {
                            var mainMenu = FindAncestorByType(configButton, MainMenuType!);
                            if (mainMenu != null)
                            {
                                LastHitButtonField.SetValue(mainMenu, configButton);
                            }
                        }

                        OpenOverlay(configButton);
                    }));
                Logger.Info("[ModConfig] Connected to NClickableControl.SignalName.Released");
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] Failed to connect Released signal: {ex.Message}");
            }
        }

        private static void OpenOverlay(Node source)
        {
            try
            {
                // Match Hextech's ResolveRoot pattern: use the source button's scene tree root
                Node root = source.GetTree()?.Root is Node r ? r : source;
                ModConfigPanel.Show(root);
                Logger.Info("[ModConfig] Panel shown");
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] Failed to show panel: {ex.Message}");
            }
        }

        private static string GetSignalName(string constantName, string fallback)
        {
            try
            {
                if (ClickableControlType != null)
                {
                    var signalNameField = ClickableControlType.GetNestedTypes()
                        .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                        .FirstOrDefault(f => f.Name == constantName);

                    if (signalNameField != null)
                    {
                        var value = signalNameField.GetValue(null);
                        if (value is string str)
                            return str;
                        if (value is StringName sn)
                            return sn.ToString();
                    }
                }
            }
            catch { }

            return fallback;
        }

        private static Control? TryFindButtonContainer(Node host)
        {
            // 递归查找包含多个按钮的容器
            foreach (var child in host.GetChildren())
            {
                if (child is Control ctrl && ctrl.GetChildCount() >= 3)
                {
                    bool hasButtons = ctrl.GetChildren()
                        .Any(c => c.GetType().Name.Contains("Button"));
                    if (hasButtons)
                        return ctrl;
                }
            }

            foreach (var child in host.GetChildren())
            {
                if (child.GetChildCount() > 0)
                {
                    var found = TryFindButtonContainer(child);
                    if (found != null) return found;
                }
            }

            return null;
        }

        private static Node? FindAncestorByType(Node node, Type type)
        {
            var current = node.GetParent();
            while (current != null)
            {
                if (type.IsInstanceOfType(current))
                    return current;
                current = current.GetParent();
            }
            return null;
        }

        private static async Task<bool> AwaitProcessFrameAsync(Node node)
        {
            try
            {
                int frameCount = 0;
                while (frameCount < 60)
                {
                    await Task.Delay(16);
                    if (!GodotObject.IsInstanceValid(node))
                        return false;
                    frameCount++;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Run开始补丁 - 在run开始时应用配置
    /// </summary>
    public static class RunStartPatch
    {
        public static void Postfix(RunState __instance)
        {
            try
            {
                _configApplied = true;
                Logger.Info("[ModConfig] RunStartPatch: 游戏开始，配置已应用");
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] RunStartPatch 失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 阵营补丁 - 支持MCV模式触发国旗事件
    /// 当玩家配置了MCV模式时，即使不是RA2角色也能获得对应阵营的国旗
    /// </summary>
    public static class FactionPatch
    {
        public static void Postfix(Player player, ref FlagManager.Faction __result)
        {
            try
            {
                // 如果已经检测到阵营（RA2角色），不做修改
                if (__result != FlagManager.Faction.None) return;

                if (player?.Character == null) return;

                string? characterId = player.Character?.Id?.Entry;
                if (string.IsNullOrEmpty(characterId)) return;

                var config = ModConfigManager.GetCharacterConfig(characterId);

                if (config.BaseCarMode == BaseCarMode.None) return;

                // 根据MCV模式确定阵营
                __result = config.BaseCarMode switch
                {
                    BaseCarMode.Allied => FlagManager.Faction.Allies,
                    BaseCarMode.Soviet => FlagManager.Faction.Soviet,
                    BaseCarMode.Yuri => FlagManager.Faction.Yuri,
                    _ => FlagManager.Faction.None
                };

                if (__result != FlagManager.Faction.None)
                {
                    Logger.Info($"[ModConfig] FactionPatch: MCV模式将玩家 {characterId} 的阵营设为 {__result}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ModConfig] FactionPatch 失败: {ex.Message}");
            }
        }
    }
}
