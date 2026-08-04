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
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using RedAlert2ModCode.Common.Relics;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.DeckConfig;

/// <summary>
/// Mod配置补丁 - 在游戏流程中应用mod配置
/// </summary>
public static class ModConfigPatches
{
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
        // 注意：游戏没有 RunState.CreateInitialDeckCards 方法（旧代码打到不存在的目标，覆盖从未生效）。
        // 真实初始牌组在 Player.CreateForNewRun -> PopulateStartingInventory() 中依次生成 牌组/遗物/药水。
        // 必须 Patch 整个 PopulateStartingInventory（而非仅 PopulateStartingDeck）：
        //   在 PopulateStartingInventory 的 Postfix 中替换牌组、追加基地车、补授刀乐遗物，
        //   此时所有 PopulateStarting* 已执行完毕，不会触发 "Relics have already been populated" 冲突。
        try
        {
            var populateInventoryMethod = AccessTools.Method(typeof(Player), "PopulateStartingInventory");
            if (populateInventoryMethod != null)
            {
                harmony.Patch(
                    original: populateInventoryMethod,
                    postfix: new HarmonyMethod(typeof(InitialDeckPatch), nameof(InitialDeckPatch.Postfix))
                );
                Logger.Info("[ModConfig] 初始卡组补丁安装成功 (Player.PopulateStartingInventory)");
            }
            else
            {
                Logger.Warn("[ModConfig] 找不到 Player.PopulateStartingInventory 方法");
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

        // 补丁3: 拦截多人开局设置（RunManager.StartRun 在当前版本不存在，改用真实存在的 SetUpNewMultiplayer），
        // 缓存玩家列表并广播本机配置，供主机按 NetId 应用
        try
        {
            var setupMultiplayerMethod = AccessTools.Method(typeof(RunManager), "SetUpNewMultiplayer");
            if (setupMultiplayerMethod != null)
            {
                harmony.Patch(
                    original: setupMultiplayerMethod,
                    postfix: new HarmonyMethod(typeof(RunStartPatch), nameof(RunStartPatch.Postfix))
                );
                Logger.Info("[ModConfig] 多人开局设置补丁安装成功 (RunManager.SetUpNewMultiplayer)");
            }
            else
            {
                Logger.Warn("[ModConfig] 找不到 RunManager.SetUpNewMultiplayer");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] 多人开局设置补丁安装失败: {ex.Message}");
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

        // 补丁5: 卡池奖励模式 - 直接给奖励候选注入箱子卡（不修改角色池，原版角色也生效）
        try
        {
            var getPossibleCardsMethod = AccessTools.Method(typeof(CardCreationOptions), "GetPossibleCards");
            if (getPossibleCardsMethod != null)
            {
                harmony.Patch(
                    original: getPossibleCardsMethod,
                    postfix: new HarmonyMethod(typeof(CardRewardCratePatch), nameof(CardRewardCratePatch.Postfix))
                );
                Logger.Info("[ModConfig] 卡池奖励模式补丁安装成功 (CardCreationOptions.GetPossibleCards)");
            }
            else
            {
                Logger.Warn("[ModConfig] 找不到 CardCreationOptions.GetPossibleCards");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[ModConfig] 卡池奖励模式补丁安装失败: {ex.Message}");
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
        public static void Postfix(Player __instance)
        {
            try
            {
                if (__instance?.Character == null) return;

                string? characterId = __instance.Character?.Id?.Entry;
                if (string.IsNullOrEmpty(characterId)) return;

                // 多人模式按玩家 NetId 取配置，保证每个玩家独立应用自己的配置
                var config = ModConfigManager.GetCharacterConfig(characterId, __instance.NetId);

                // 幸运方块 / 自定义卡组：清空并重建初始牌组
                List<CardModel>? replacement = null;
                if (config.LuckyCrateMode)
                {
                    replacement = CreateLuckyCrateDeck();
                    // 幸运方块 + 自定义初始卡组可叠加：自定义卡牌追加在箱子卡之后
                    if (config.EnableCustomDeck && config.CustomDeckCardTypes.Count > 0)
                    {
                        replacement.AddRange(CreateCustomDeck(config));
                    }
                    Logger.Info($"[ModConfig] 已应用幸运方块模式（含自定义卡组），角色: {characterId}");
                }
                else if (config.EnableCustomDeck && config.CustomDeckCardTypes.Count > 0)
                {
                    var customDeck = CreateCustomDeck(config);
                    if (customDeck.Count > 0)
                    {
                        replacement = customDeck;
                        Logger.Info($"[ModConfig] 已应用自定义卡组，角色: {characterId}, 卡牌数: {customDeck.Count}");
                    }
                }

                if (replacement != null)
                {
                    // Player.PopulateStartingInventory 已用默认牌组填充过 Deck，这里清空后重建
                    __instance.Deck.Clear(silent: true);
                    foreach (CardModel card in replacement)
                    {
                        card.FloorAddedToDeck = 1;
                        __instance.Deck.AddInternal(card, -1, true);
                    }
                    Logger.Info($"[ModConfig] 初始卡组已覆盖，角色: {characterId}, 卡牌数: {replacement.Count}");
                }

                // 基地车模式 - 在初始卡组中追加基地车 + 补授刀乐遗物
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
            if (string.IsNullOrEmpty(typeName)) return null;

            // 1) 尝试全名（含命名空间）
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = asm.GetType(typeName);
                    if (type != null) return type;
                }
                catch { }
            }

            // 2) 常用命名空间（本mod + 原版卡牌）
            string[] namespaces =
            {
                "RedAlert2ModCode.Allies.Cards", "RedAlert2ModCode.Soviet.Cards", "RedAlert2ModCode.Common.Cards",
                "MegaCrit.Sts2.Core.Models.Cards",
            };
            foreach (var ns in namespaces)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var type = asm.GetType($"{ns}.{typeName}");
                        if (type != null) return type;
                    }
                    catch { }
                }
            }

            // 3) 短名扫描（兼容原版卡牌，如 Wound）
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = asm.GetTypes().FirstOrDefault(t => t.Name == typeName);
                    if (type != null) return type;
                }
                catch { }
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
            // 随机箱子×5、回血箱子×1、士兵/车辆/海军/空军箱子各×1（共10张）
            var crateList = new[]
            {
                ("RandomCrate", 5),
                ("HealCrate", 1),
                ("SoldierCrate", 1),
                ("VehicleCrate", 1),
                ("NavyCrate", 1),
                ("AirForceCrate", 1),
            };

            foreach (var (name, count) in crateList)
            {
                try
                {
                    var cardType = FindCardType(name);
                    if (cardType != null)
                    {
                        var cardModel = GetCardModel(cardType);
                        if (cardModel != null)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                deck.Add(cardModel.ToMutable());
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn($"[ModConfig] 找不到箱子卡类型: {name}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[ModConfig] 创建箱子卡失败 {name}: {ex.Message}");
                }
            }

            return deck;
        }

        private static void ApplyBaseCarMode(Player player, CharacterConfig config)
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

                string characterId = player.Character?.Id?.Entry ?? string.Empty;

                // 同阵营的本mod角色起始卡组已含对应基地车，不重复添加（等同于无效果）
                if (IsSameFactionAsMcv(characterId, config.BaseCarMode))
                {
                    Logger.Info($"[ModConfig] 基地车模式: 角色 {characterId} 已有对应阵营基地车，跳过添加");
                }
                else
                {
                    var mcvType = FindCardType(mcvName);
                    if (mcvType != null)
                    {
                        var mcvCard = GetCardModel(mcvType);
                        if (mcvCard != null)
                        {
                            var mcvInstance = mcvCard.ToMutable();
                            mcvInstance.FloorAddedToDeck = 1;
                            player.Deck.AddInternal(mcvInstance, -1, true);
                            Logger.Info($"[ModConfig] 基地车模式: 已添加 {mcvName} 到卡组");
                        }
                    }
                    else
                    {
                        Logger.Warn($"[ModConfig] 找不到基地车类型: {mcvName}");
                    }
                }

                // 补授刀乐遗物（本mod角色已自带，不重复授予）
                if (!player.Relics.Any(r => r is DollarRelic))
                {
                    var relic = ModelDb.Relic<DollarRelic>().ToMutable();
                    relic.FloorAddedToDeck = 1;
                    try { SaveManager.Instance.MarkRelicAsSeen(relic); } catch { }
                    player.AddRelicInternal(relic, -1, true);
                    Logger.Info($"[ModConfig] 基地车模式: 已补授刀乐遗物给 {characterId}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[ModConfig] ApplyBaseCarMode 失败: {ex.Message}");
            }
        }

        private static bool IsSameFactionAsMcv(string characterId, BaseCarMode mode)
        {
            if (mode == BaseCarMode.Allied && characterId.Equals("Allies", StringComparison.OrdinalIgnoreCase))
                return true;
            if (mode == BaseCarMode.Soviet && characterId.Equals("Soviet", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
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
            // NClickableControl.RefreshFocus 发出的 Focused/Unfocused 信号带 1 个参数（控件本身），
            // 必须用 Callable.From<T>(...) 匹配参数个数，否则报 "Expected 0 argument(s), received 1"。
            if (ButtonFocusedMethod != null && FocusedSignalName != null)
            {
                ((GodotObject)configButton).Connect(
                    FocusedSignalName,
                    Callable.From<GodotObject>(_ =>
                    {
                        ButtonFocusedMethod.Invoke(mainMenu, new[] { configButton });
                    }));
            }

            if (ButtonUnfocusedMethod != null && UnfocusedSignalName != null)
            {
                ((GodotObject)configButton).Connect(
                    UnfocusedSignalName,
                    Callable.From<GodotObject>(_ =>
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
        public static void Postfix(RunState state)
        {
            try
            {
                ModConfigManager.SetRunPlayers(state?.Players);
                // 多人开局：把本机本地玩家的配置广播给主机，供主机按 NetId 应用
                ModConfigManager.BroadcastAllLocalConfigs();
                Logger.Info("[ModConfig] RunStartPatch: 开局设置完成，配置已广播");
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

                var config = ModConfigManager.GetCharacterConfig(characterId, player.NetId);

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

    /// <summary>
    /// 卡池奖励模式补丁：直接给卡牌奖励候选注入箱子卡。
    ///   AllCrates → 仅战斗结束卡牌奖励（Encounter 来源）替换为纯箱子卡，商店/事件保持默认池；
    ///   AddCrates → 在卡牌奖励候选中混入箱子卡（不修改角色池，原版角色也生效）。
    /// </summary>
    public static class CardRewardCratePatch
    {
        public static void Postfix(CardCreationOptions __instance, Player player, ref IEnumerable<CardModel> __result)
        {
            try
            {
                if (__result == null || player?.Character == null) return;

                string? characterId = player.Character?.Id?.Entry;
                if (string.IsNullOrEmpty(characterId)) return;

                var config = ModConfigManager.GetCharacterConfig(characterId, player.NetId);
                if (config.CratePoolMode == CratePoolMode.None) return;

                var crateCards = CratePoolHelper.GetAllCrateCards().ToList();
                if (crateCards.Count == 0) return;

                if (config.CratePoolMode == CratePoolMode.AllCrates)
                {
                    // 仅战斗结束卡牌奖励生效（Encounter 来源），商店/事件等使用默认角色卡池
                    if (__instance.Source != CardCreationSource.Encounter) return;
                    __result = crateCards;
                    Logger.Info($"[ModConfig] 奖励模式: 战斗卡牌奖励仅箱子（角色 {characterId}）");
                }
                else // AddCrates：奖励候选混入箱子卡
                {
                    __result = __result.Concat(crateCards).Distinct();
                    Logger.Info($"[ModConfig] 奖励模式: 卡牌奖励加入箱子（角色 {characterId}）");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ModConfig] CardRewardCratePatch 失败: {ex.Message}");
            }
        }
    }
}
