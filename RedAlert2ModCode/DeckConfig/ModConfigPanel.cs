// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.DeckConfig;

/// <summary>
/// 红警2模组配置面板
/// </summary>
internal static class ModConfigPanel
{
    private const string LocTable = "characters";
    private const string OverlayName = "RedAlert2ModConfigOverlay";
    private const int OverlayZIndex = 1000;
    private static readonly string[] FeatureIds = { "deck_config" };
    private static string[] FeatureNames => new[] { L("CONFIG_FEATURE_DECK") };
    private static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = new("ModConfigPanel", MegaCrit.Sts2.Core.Logging.LogType.Generic);

    private static Control? _layer;
    private static HBoxContainer? _charIconRow;
    private static HBoxContainer? _subTabBar;
    private static ScrollContainer? _scrollContainer;
    private static VBoxContainer? _contentContainer;
    private static Button[]? _sideBarButtons;
    private static Button[]? _subTabButtons;
    private static Control? _charInfoHeader;
    private static Label? _charNameLabel;
    private static Label? _charFactionLabel;

    private static int _activeFeature;
    private static int _activeSubTab;

    private static string? _selectedCharacterId;
    private static CharacterConfig? _currentConfig;
    private static CardLibraryTab? _cardLibraryTab;

    private static readonly Dictionary<string, Texture2D> _iconCache = new();

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

    internal static bool IsOpen => _layer is { Visible: true };

    internal static void Toggle(Node root)
    {
        if (_layer == null || !GodotObject.IsInstanceValid(_layer))
        {
            Build();
            RemoveExistingOverlay(root);
            root.AddChild(_layer!);
            _layer!.Visible = true;
            return;
        }

        if (_layer.GetParent() == null)
        {
            root.AddChild(_layer);
        }

        _layer.Visible = !_layer.Visible;
    }

    internal static void Show(Node root)
    {
        if (_layer == null || !GodotObject.IsInstanceValid(_layer))
        {
            Build();
        }

        RemoveExistingOverlay(root);

        // 若 _layer 已挂载到场景树，不要重复 AddChild（否则报 "already has a parent"）
        if (_layer!.GetParent() == null)
        {
            root.AddChild(_layer);
        }
        _layer!.Visible = true;
    }

    private static void RemoveExistingOverlay(Node root)
    {
        var existing = root.GetNodeOrNull<Control>(OverlayName);
        // 仅释放旧的、非当前 _layer 的残留覆盖层，避免误删自身导致重复 AddChild 报错
        if (existing != null && GodotObject.IsInstanceValid(existing) && !ReferenceEquals(existing, _layer))
        {
            existing.QueueFree();
        }
        if (_layer != null && !GodotObject.IsInstanceValid(_layer))
        {
            _layer = null;
        }
    }

    internal static void Hide()
    {
        if (_layer != null && GodotObject.IsInstanceValid(_layer))
        {
            _layer.Visible = false;
        }
    }

    internal static void RequestRefresh()
    {
        if (_layer != null && GodotObject.IsInstanceValid(_layer) && _layer.Visible)
        {
            RefreshContent();
        }
    }

    private static void Build()
    {
        _layer = new Control
        {
            Name = OverlayName,
            MouseFilter = Control.MouseFilterEnum.Stop,
            ZIndex = OverlayZIndex,
        };
        _layer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var backstop = new ColorRect();
        backstop.Color = new Color(0f, 0f, 0f, 0.6f);
        backstop.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        backstop.MouseFilter = Control.MouseFilterEnum.Stop;
        backstop.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                Hide();
                backstop.GetViewport()?.SetInputAsHandled();
            }
        };
        _layer.AddChild(backstop);

        var panel = new PanelContainer();
        panel.AnchorLeft = 0.04f;
        panel.AnchorRight = 0.96f;
        panel.AnchorTop = 0.04f;
        panel.AnchorBottom = 0.96f;
        panel.GrowHorizontal = Control.GrowDirection.Both;
        panel.GrowVertical = Control.GrowDirection.Both;
        panel.MouseFilter = Control.MouseFilterEnum.Stop;

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.08f, 0.06f, 0.10f, 0.97f);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Color("B89840");
        panelStyle.SetCornerRadiusAll(8);
        panelStyle.SetContentMarginAll(0);
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        _layer.AddChild(panel);

        var rootVBox = new VBoxContainer();
        rootVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        rootVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        rootVBox.AddThemeConstantOverride("separation", 0);
        panel.AddChild(rootVBox);

        var titleMargin = new MarginContainer();
        titleMargin.AddThemeConstantOverride("margin_left", 16);
        titleMargin.AddThemeConstantOverride("margin_right", 16);
        titleMargin.AddThemeConstantOverride("margin_top", 10);
        titleMargin.AddThemeConstantOverride("margin_bottom", 6);
        titleMargin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleMargin.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        rootVBox.AddChild(titleMargin);

        var titleBar = new HBoxContainer();
        titleBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleBar.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        titleBar.AddThemeConstantOverride("separation", 8);
        titleBar.Alignment = BoxContainer.AlignmentMode.Center;
        titleMargin.AddChild(titleBar);

        var titleLabel = new Label();
        titleLabel.Text = L("CONFIG_TITLE");
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", StsColors.gold);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        titleBar.AddChild(titleLabel);

        var closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.CustomMinimumSize = new Vector2(36, 36);
        closeBtn.AddThemeFontSizeOverride("font_size", 16);
        closeBtn.AddThemeColorOverride("font_color", StsColors.cream);
        closeBtn.AddThemeColorOverride("font_hover_color", StsColors.red);
        ApplyFlatStyle(closeBtn);
        closeBtn.Pressed += Hide;
        titleBar.AddChild(closeBtn);

        var titleDivider = new ColorRect();
        titleDivider.CustomMinimumSize = new Vector2(0, 2);
        titleDivider.Color = new Color(0.91f, 0.86f, 0.75f, 0.25f);
        titleDivider.MouseFilter = Control.MouseFilterEnum.Ignore;
        rootVBox.AddChild(titleDivider);

        var mainHBox = new HBoxContainer();
        mainHBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        mainHBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        rootVBox.AddChild(mainHBox);

        BuildSidebar(mainHBox);
        BuildContentArea(mainHBox);

        _activeFeature = 0;
        UpdateSidebarHighlights();

        if (_selectedCharacterId == null)
        {
            var firstChar = GetAllCharacters().FirstOrDefault();
            if (firstChar != null)
            {
                _selectedCharacterId = firstChar.Id.Entry;
            }
        }

        RefreshContent();
    }

    private static void BuildSidebar(HBoxContainer parent)
    {
        var sidebar = new PanelContainer();
        sidebar.CustomMinimumSize = new Vector2(200, 0);
        sidebar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        var sideStyle = new StyleBoxFlat();
        sideStyle.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.95f);
        sideStyle.SetBorderWidthAll(1);
        sideStyle.BorderColor = new Color(0.55f, 0.45f, 0.25f, 0.35f);
        sideStyle.SetCornerRadiusAll(4);
        sidebar.AddThemeStyleboxOverride("panel", sideStyle);
        parent.AddChild(sidebar);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        sidebar.AddChild(margin);

        var list = new VBoxContainer();
        list.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        list.AddThemeConstantOverride("separation", 4);
        margin.AddChild(list);

        var heading = new Label();
        heading.Text = L("CONFIG_SIDEBAR_TITLE");
        heading.AddThemeFontSizeOverride("font_size", 14);
        heading.AddThemeColorOverride("font_color", StsColors.gold);
        list.AddChild(heading);

        var sep = new ColorRect();
        sep.CustomMinimumSize = new Vector2(0, 1);
        sep.Color = new Color(0.55f, 0.45f, 0.25f, 0.4f);
        sep.MouseFilter = Control.MouseFilterEnum.Ignore;
        list.AddChild(sep);

        _sideBarButtons = new Button[FeatureIds.Length];
        for (int i = 0; i < FeatureIds.Length; i++)
        {
            int idx = i;
            var btn = CreateSidebarButton(FeatureNames[i]);
            btn.Pressed += () => SwitchFeature(idx);
            list.AddChild(btn);
            _sideBarButtons[i] = btn;
        }
    }

    private static void BuildContentArea(HBoxContainer parent)
    {
        var contentMargin = new MarginContainer();
        contentMargin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentMargin.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        contentMargin.AddThemeConstantOverride("margin_left", 12);
        contentMargin.AddThemeConstantOverride("margin_right", 12);
        contentMargin.AddThemeConstantOverride("margin_top", 8);
        contentMargin.AddThemeConstantOverride("margin_bottom", 8);
        parent.AddChild(contentMargin);

        var contentVBox = new VBoxContainer();
        contentVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        contentVBox.AddThemeConstantOverride("separation", 8);
        contentMargin.AddChild(contentVBox);

        // Character icon row at the top
        var iconRowContainer = new PanelContainer();
        iconRowContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        var iconRowStyle = new StyleBoxFlat();
        iconRowStyle.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.95f);
        iconRowStyle.SetBorderWidthAll(1);
        iconRowStyle.BorderColor = new Color(0.55f, 0.45f, 0.25f, 0.35f);
        iconRowStyle.SetCornerRadiusAll(4);
        iconRowStyle.SetContentMarginAll(8);
        iconRowContainer.AddThemeStyleboxOverride("panel", iconRowStyle);
        contentVBox.AddChild(iconRowContainer);

        _charIconRow = new HBoxContainer();
        _charIconRow.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _charIconRow.AddThemeConstantOverride("separation", 16);
        _charIconRow.Alignment = BoxContainer.AlignmentMode.Center;
        iconRowContainer.AddChild(_charIconRow);

        var allCharacters = GetAllCharacters();
        if (allCharacters.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = L("CONFIG_NO_CHARACTERS");
            emptyLabel.AddThemeFontSizeOverride("font_size", 14);
            emptyLabel.AddThemeColorOverride("font_color", StsColors.gray);
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _charIconRow.AddChild(emptyLabel);
        }
        else
        {
            foreach (var character in allCharacters)
            {
                _charIconRow.AddChild(CreateCharacterIconButton(character));
            }
        }

        // Character info section (name + faction)
        _charInfoHeader = new VBoxContainer();
        _charInfoHeader.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _charInfoHeader.AddThemeConstantOverride("separation", 2);
        contentVBox.AddChild(_charInfoHeader);

        _charNameLabel = new Label();
        _charNameLabel.AddThemeFontSizeOverride("font_size", 18);
        _charNameLabel.AddThemeColorOverride("font_color", StsColors.gold);
        _charInfoHeader.AddChild(_charNameLabel);

        _charFactionLabel = new Label();
        _charFactionLabel.AddThemeFontSizeOverride("font_size", 13);
        _charFactionLabel.AddThemeColorOverride("font_color", StsColors.cream);
        _charInfoHeader.AddChild(_charFactionLabel);

        // Sub-tabs
        _subTabBar = new HBoxContainer();
        _subTabBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _subTabBar.AddThemeConstantOverride("separation", 6);
        contentVBox.AddChild(_subTabBar);

        string[] subTabNames = { L("CONFIG_SUBTAB_BASE_CAR"), L("CONFIG_SUBTAB_LUCKY") };
        _subTabButtons = new Button[subTabNames.Length];
        for (int i = 0; i < subTabNames.Length; i++)
        {
            int idx = i;
            var btn = CreateSubTabButton(subTabNames[i]);
            btn.Pressed += () => SwitchSubTab(idx);
            _subTabBar.AddChild(btn);
            _subTabButtons[i] = btn;
        }

        // Scroll container for dynamic content
        _scrollContainer = new ScrollContainer();
        _scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        contentVBox.AddChild(_scrollContainer);

        _contentContainer = new VBoxContainer();
        _contentContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _contentContainer.AddThemeConstantOverride("separation", 6);
        _scrollContainer.AddChild(_contentContainer);
    }

    private static Button CreateSidebarButton(string text)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(180, 36);
        btn.AddThemeFontSizeOverride("font_size", 14);
        btn.AddThemeColorOverride("font_color", StsColors.cream);
        btn.AddThemeColorOverride("font_hover_color", StsColors.gold);
        btn.AddThemeColorOverride("font_pressed_color", StsColors.gray);
        ApplyFlatStyle(btn);
        return btn;
    }

    private static Button CreateSubTabButton(string text)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(200, 32);
        btn.AddThemeFontSizeOverride("font_size", 14);
        btn.AddThemeColorOverride("font_color", StsColors.cream);
        btn.AddThemeColorOverride("font_hover_color", StsColors.gold);
        btn.AddThemeColorOverride("font_pressed_color", StsColors.gray);
        ApplyFlatStyle(btn);
        return btn;
    }

    private static void SwitchFeature(int index)
    {
        _activeFeature = index;
        UpdateSidebarHighlights();
        RefreshContent();
    }

    private static void SwitchSubTab(int index)
    {
        _activeSubTab = index;
        UpdateSubTabHighlights();
        RefreshContent();
    }

    private static void UpdateSidebarHighlights()
    {
        if (_sideBarButtons == null) return;
        for (int i = 0; i < _sideBarButtons.Length; i++)
        {
            var btn = _sideBarButtons[i];
            bool active = i == _activeFeature;
            btn.AddThemeColorOverride("font_color", active ? StsColors.gold : StsColors.cream);
            btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
                active ? new Color(0.18f, 0.15f, 0.08f, 0.95f) : new Color(0.12f, 0.10f, 0.15f, 0.85f),
                active ? StsColors.gold : new Color(0.35f, 0.30f, 0.25f, 0.5f)));
        }
    }

    private static void UpdateSubTabHighlights()
    {
        if (_subTabButtons == null) return;
        for (int i = 0; i < _subTabButtons.Length; i++)
        {
            var btn = _subTabButtons[i];
            bool active = i == _activeSubTab;
            btn.AddThemeColorOverride("font_color", active ? StsColors.gold : StsColors.cream);
            btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
                active ? new Color(0.18f, 0.15f, 0.08f, 0.95f) : new Color(0.12f, 0.10f, 0.15f, 0.85f),
                active ? StsColors.gold : new Color(0.35f, 0.30f, 0.25f, 0.5f)));
        }
    }

    private static void RefreshContent()
    {
        if (_contentContainer == null) return;
        ClearChildren(_contentContainer);

        // Update character info header
        UpdateCharacterInfo();

        if (string.IsNullOrEmpty(_selectedCharacterId))
        {
            var warnLabel = new Label();
            warnLabel.Text = L("CONFIG_WARNING_SELECT_CHAR");
            warnLabel.AddThemeFontSizeOverride("font_size", 16);
            warnLabel.AddThemeColorOverride("font_color", StsColors.cream);
            _contentContainer.AddChild(warnLabel);
            return;
        }

        var config = ModConfigManager.GetCharacterConfig(_selectedCharacterId);
        _currentConfig = config;

        // Update icon highlights
        UpdateCharacterIconHighlights();

        switch (_activeSubTab)
        {
            case 0: BuildBaseCarModeContent(_contentContainer, config); break;
            case 1: BuildLuckyCrateContent(_contentContainer, config); break;
        }

        AddDivider(_contentContainer);
        BuildCardPoolSection(_contentContainer, config);
    }

    private static void UpdateCharacterInfo()
    {
        if (_charNameLabel == null || _charFactionLabel == null) return;

        if (string.IsNullOrEmpty(_selectedCharacterId))
        {
            _charNameLabel.Text = L("CONFIG_SELECT_CHAR_PROMPT");
            _charFactionLabel.Text = string.Empty;
            return;
        }

        _charNameLabel.Text = GetCharacterDisplayName(_selectedCharacterId);
        _charFactionLabel.Text = GetCharacterFactionInfo(_selectedCharacterId);
    }

    private static string GetCharacterDisplayName(string characterId)
    {
        try
        {
            var model = ModelDb.AllCharacters.FirstOrDefault(c =>
            {
                try { return c.Id.Entry == characterId; }
                catch { return false; }
            });
            if (model != null)
            {
                try { return model.Title?.GetFormattedText() ?? characterId; }
                catch { return characterId; }
            }
        }
        catch { }
        return characterId;
    }

    private static void UpdateCharacterIconHighlights()
    {
        if (_charIconRow == null) return;

        foreach (var child in _charIconRow.GetChildren())
        {
            if (child is VBoxContainer wrapper && wrapper.HasMeta("char_id"))
            {
                string charId = wrapper.GetMeta("char_id").AsString();
                bool isSelected = charId == _selectedCharacterId;

                foreach (var c in wrapper.GetChildren())
                {
                    if (c is Button btn && btn.CustomMinimumSize.X == 80 && btn.CustomMinimumSize.Y == 80)
                    {
                        var bg = btn.GetThemeStylebox("normal") as StyleBoxFlat;
                        if (bg != null)
                        {
                            bg.BgColor = isSelected
                                ? new Color(0.30f, 0.25f, 0.10f, 0.95f)
                                : new Color(0.15f, 0.12f, 0.18f, 0.9f);
                            bg.BorderColor = isSelected ? StsColors.gold : new Color(0.45f, 0.40f, 0.30f, 0.5f);
                        }
                    }
                    if (c is Label nameLabel)
                    {
                        nameLabel.AddThemeColorOverride("font_color", isSelected ? StsColors.gold : StsColors.cream);
                    }
                }
            }
        }
    }

    private static void BuildBaseCarModeContent(VBoxContainer container, CharacterConfig config)
    {
        var header = CreateSectionHeader(L("CONFIG_BASE_CAR_TITLE"));
        container.AddChild(header);

        var desc = new Label();
        desc.Text = L("CONFIG_BASE_CAR_DESC");
        desc.AddThemeFontSizeOverride("font_size", 13);
        desc.AddThemeColorOverride("font_color", StsColors.gray);
        container.AddChild(desc);

        var currentLabel = new Label();
        currentLabel.Text = L("CONFIG_BASE_CAR_CURRENT", config.BaseCarMode);
        currentLabel.AddThemeFontSizeOverride("font_size", 14);
        currentLabel.AddThemeColorOverride("font_color", StsColors.gold);
        container.AddChild(currentLabel);

        var modeRow = new HBoxContainer();
        modeRow.AddThemeConstantOverride("separation", 8);
        container.AddChild(modeRow);

        BaseCarMode[] modes = { BaseCarMode.None, BaseCarMode.Allied, BaseCarMode.Soviet, BaseCarMode.Yuri };
        string[] modeNames = { L("CONFIG_BASE_CAR_NONE"), L("CONFIG_BASE_CAR_ALLIED"), L("CONFIG_BASE_CAR_SOVIET"), L("CONFIG_BASE_CAR_YURI") };

        for (int i = 0; i < modes.Length; i++)
        {
            var mode = modes[i];
            var btn = CreateToggleButton(config.BaseCarMode == mode, modeNames[i]);
            btn.CustomMinimumSize = new Vector2(100, 32);
            btn.Pressed += () =>
            {
                config.BaseCarMode = mode;
                ModConfigManager.UpdateCharacterConfig(config);
                RefreshContent();
            };
            modeRow.AddChild(btn);
        }
    }

    private static void BuildLuckyCrateContent(VBoxContainer container, CharacterConfig config)
    {
        var header = CreateSectionHeader(L("CONFIG_LUCKY_CRATE_TITLE"));
        container.AddChild(header);

        var desc = new Label();
        desc.Text = L("CONFIG_LUCKY_CRATE_DESC");
        desc.AddThemeFontSizeOverride("font_size", 13);
        desc.AddThemeColorOverride("font_color", StsColors.gray);
        container.AddChild(desc);

        var statusLabel = new Label();
        statusLabel.Text = config.LuckyCrateMode
            ? L("CONFIG_LUCKY_CRATE_STATUS_ENABLED")
            : L("CONFIG_LUCKY_CRATE_STATUS_DISABLED");
        statusLabel.AddThemeFontSizeOverride("font_size", 13);
        statusLabel.AddThemeColorOverride("font_color", config.LuckyCrateMode ? StsColors.gold : StsColors.gray);
        container.AddChild(statusLabel);

        var toggleRow = new HBoxContainer();
        toggleRow.AddThemeConstantOverride("separation", 12);
        container.AddChild(toggleRow);

        var toggleLabel = new Label();
        toggleLabel.Text = L("CONFIG_LUCKY_CRATE_TOGGLE");
        toggleLabel.AddThemeFontSizeOverride("font_size", 15);
        toggleLabel.AddThemeColorOverride("font_color", StsColors.cream);
        toggleRow.AddChild(toggleLabel);

        var luckyBtn = CreateToggleButton(config.LuckyCrateMode,
            config.LuckyCrateMode ? L("CONFIG_ENABLED") : L("CONFIG_DISABLED"));
        luckyBtn.Pressed += () =>
        {
            config.LuckyCrateMode = !config.LuckyCrateMode;
            ModConfigManager.UpdateCharacterConfig(config);
            RefreshContent();
        };
        toggleRow.AddChild(luckyBtn);

        var noticeLabel = new Label();
        noticeLabel.Text = L("CONFIG_NOTICE");
        noticeLabel.AddThemeFontSizeOverride("font_size", 12);
        noticeLabel.AddThemeColorOverride("font_color", StsColors.gold);
        noticeLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        container.AddChild(noticeLabel);

        AddDivider(container);

        // 卡池奖励模式（三选一互斥：默认 / 全为箱子 / 加入箱子）
        var poolHeader = CreateSectionHeader(L("CONFIG_CRATE_POOL_TITLE"));
        container.AddChild(poolHeader);

        var poolDesc = new Label();
        poolDesc.Text = L("CONFIG_CRATE_POOL_DESC");
        poolDesc.AddThemeFontSizeOverride("font_size", 13);
        poolDesc.AddThemeColorOverride("font_color", StsColors.gray);
        poolDesc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        container.AddChild(poolDesc);

        var poolRow = new HBoxContainer();
        poolRow.AddThemeConstantOverride("separation", 8);
        container.AddChild(poolRow);

        (CratePoolMode mode, string label)[] poolModes =
        {
            (CratePoolMode.None, L("CONFIG_CRATE_POOL_NONE")),
            (CratePoolMode.AllCrates, L("CONFIG_CRATE_POOL_ALL")),
            (CratePoolMode.AddCrates, L("CONFIG_CRATE_POOL_ADD")),
        };
        foreach (var (mode, label) in poolModes)
        {
            var btn = CreateToggleButton(config.CratePoolMode == mode, label);
            btn.CustomMinimumSize = new Vector2(130, 32);
            btn.Pressed += () =>
            {
                config.CratePoolMode = mode;
                ModConfigManager.UpdateCharacterConfig(config);
                RefreshContent();
            };
            poolRow.AddChild(btn);
        }
    }

    private static void BuildCardPoolSection(VBoxContainer container, CharacterConfig config)
    {
        var poolHeader = CreateSectionHeader(L("CONFIG_CARD_POOL"));
        container.AddChild(poolHeader);

        var enableRow = new HBoxContainer();
        enableRow.AddThemeConstantOverride("separation", 12);
        container.AddChild(enableRow);

        var enableLabel = new Label();
        enableLabel.Text = L("CONFIG_ENABLE_CUSTOM_DECK");
        enableLabel.AddThemeFontSizeOverride("font_size", 14);
        enableLabel.AddThemeColorOverride("font_color", StsColors.cream);
        enableRow.AddChild(enableLabel);

        var enableBtn = CreateToggleButton(config.EnableCustomDeck,
            config.EnableCustomDeck ? L("CONFIG_ENABLED") : L("CONFIG_DISABLED"));
        enableBtn.Pressed += () =>
        {
            config.EnableCustomDeck = !config.EnableCustomDeck;
            ModConfigManager.UpdateCharacterConfig(config);
            RefreshContent();
        };
        enableRow.AddChild(enableBtn);

        AddDivider(container);

        if (!config.EnableCustomDeck)
        {
            var defaultHeader = CreateSectionHeader(L("CONFIG_DEFAULT_DECK"));
            container.AddChild(defaultHeader);
            ShowDefaultDeckInfo(container, _selectedCharacterId!);
        }
        else
        {
            var customHeader = CreateSectionHeader(L("CONFIG_CUSTOM_DECK"));
            container.AddChild(customHeader);
            ShowCustomDeckEditor(container, config);
        }

        AddDivider(container);

        var buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 12);
        buttonRow.Alignment = BoxContainer.AlignmentMode.Center;
        container.AddChild(buttonRow);

        var resetBtn = CreateActionButton(L("CONFIG_RESET_DEFAULT"), StsColors.cream);
        resetBtn.CustomMinimumSize = new Vector2(120, 36);
        resetBtn.Pressed += () =>
        {
            ShowConfirmDialog(
                L("CONFIG_CONFIRM_RESET_TITLE"),
                L("CONFIG_CONFIRM_RESET_DESC"),
                () =>
                {
                    ModConfigManager.ResetCharacterConfig(_selectedCharacterId!);
                    RefreshContent();
                });
        };
        buttonRow.AddChild(resetBtn);

        var saveBtn = CreateActionButton(L("CONFIG_SAVE"), StsColors.green);
        saveBtn.CustomMinimumSize = new Vector2(120, 36);
        saveBtn.Pressed += () =>
        {
            ShowConfirmDialog(
                L("CONFIG_CONFIRM_SAVE_TITLE"),
                L("CONFIG_CONFIRM_SAVE_DESC"),
                () =>
                {
                    ModConfigManager.Save();
                    ShowNotification(L("CONFIG_SAVED"));
                });
        };
        buttonRow.AddChild(saveBtn);
    }

    private static Control CreateCharacterIconButton(CharacterModel character)
    {
        var wrapper = new VBoxContainer();
        wrapper.CustomMinimumSize = new Vector2(100, 120);
        wrapper.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        wrapper.AddThemeConstantOverride("separation", 4);
        wrapper.Alignment = BoxContainer.AlignmentMode.Center;
        wrapper.SetMeta("char_id", character.Id.Entry);

        bool isSelected = _selectedCharacterId == character.Id.Entry;

        var iconBtn = new Button();
        iconBtn.CustomMinimumSize = new Vector2(80, 80);
        iconBtn.Flat = true;
        iconBtn.MouseFilter = Control.MouseFilterEnum.Stop;

        var iconBg = new StyleBoxFlat();
        iconBg.BgColor = isSelected
            ? new Color(0.30f, 0.25f, 0.10f, 0.95f)
            : new Color(0.15f, 0.12f, 0.18f, 0.9f);
        iconBg.SetBorderWidthAll(2);
        iconBg.BorderColor = isSelected ? StsColors.gold : new Color(0.45f, 0.40f, 0.30f, 0.5f);
        iconBg.SetCornerRadiusAll(8);
        iconBtn.AddThemeStyleboxOverride("normal", iconBg);

        var iconTexture = LoadCharacterIcon(character);
        if (iconTexture != null)
        {
            var textureRect = new TextureRect();
            textureRect.Texture = iconTexture;
            textureRect.AnchorRight = 1;
            textureRect.AnchorBottom = 1;
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            textureRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            iconBtn.AddChild(textureRect);
        }
        else
        {
            var colorRect = new ColorRect();
            colorRect.Color = new Color(0.3f, 0.25f, 0.35f, 0.8f);
            colorRect.AnchorRight = 1;
            colorRect.AnchorBottom = 1;
            colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            iconBtn.AddChild(colorRect);

            try
            {
                var initialLabel = new Label();
                initialLabel.Text = character.Id.Entry.Substring(0, 1).ToUpper();
                initialLabel.AddThemeFontSizeOverride("font_size", 28);
                initialLabel.AddThemeColorOverride("font_color", StsColors.gold);
                initialLabel.HorizontalAlignment = HorizontalAlignment.Center;
                initialLabel.VerticalAlignment = VerticalAlignment.Center;
                initialLabel.AnchorRight = 1;
                initialLabel.AnchorBottom = 1;
                initialLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
                iconBtn.AddChild(initialLabel);
            }
            catch { }
        }

        string charId = character.Id.Entry;
        iconBtn.Pressed += () =>
        {
            _selectedCharacterId = charId;
            RefreshContent();
        };

        wrapper.AddChild(iconBtn);

        var nameLabel = new Label();
        try
        {
            nameLabel.Text = character.Title?.GetFormattedText() ?? character.Id.Entry;
        }
        catch
        {
            nameLabel.Text = character.Id.Entry;
        }
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AddThemeColorOverride("font_color", isSelected ? StsColors.gold : StsColors.cream);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        wrapper.AddChild(nameLabel);

        return wrapper;
    }

    private static Texture2D? LoadCharacterIcon(CharacterModel character)
    {
        string charId = character.Id.Entry;
        if (_iconCache.TryGetValue(charId, out var cached))
        {
            if (GodotObject.IsInstanceValid(cached))
                return cached;
            _iconCache.Remove(charId);
        }

        try
        {
            var tex = character.IconTexture;
            if (tex != null && GodotObject.IsInstanceValid(tex))
            {
                _iconCache[charId] = tex;
                return tex;
            }
        }
        catch { }

        try
        {
            var selectIcon = character.CharacterSelectIcon;
            if (selectIcon != null && GodotObject.IsInstanceValid(selectIcon))
            {
                _iconCache[charId] = selectIcon;
                return selectIcon;
            }
        }
        catch { }

        return null;
    }

    // ============ 数据辅助方法 ============

    private static string GetCharacterFactionInfo(string characterId)
    {
        try
        {
            var faction = FlagManager.Faction.None;
            if (characterId.Equals("Allies", StringComparison.OrdinalIgnoreCase))
                faction = FlagManager.Faction.Allies;
            else if (characterId.Equals("Soviet", StringComparison.OrdinalIgnoreCase))
                faction = FlagManager.Faction.Soviet;
            else if (characterId.Contains("YURI", StringComparison.OrdinalIgnoreCase))
                faction = FlagManager.Faction.Yuri;

            if (faction != FlagManager.Faction.None)
                return L("CONFIG_FACTION", faction) + L("CONFIG_FACTION_DESC_RA2");

            return L("CONFIG_FACTION", faction) + L("CONFIG_FACTION_DESC_NORMAL");
        }
        catch { return string.Empty; }
    }

    private static void ShowDefaultDeckInfo(VBoxContainer container, string characterId)
    {
        try
        {
            var characterModel = ModelDb.AllCharacters.FirstOrDefault(c =>
            {
                try { return c.Id.Entry == characterId; }
                catch { return false; }
            });

            if (characterModel == null)
            {
                var noModelLabel = new Label();
                noModelLabel.Text = L("CONFIG_NO_CHARACTER_MODEL");
                noModelLabel.AddThemeFontSizeOverride("font_size", 14);
                noModelLabel.AddThemeColorOverride("font_color", StsColors.gray);
                container.AddChild(noModelLabel);
                return;
            }

            // 官方属性 CharacterModel.StartingDeck（RitsuLib 已将 StartingDeckEntries 映射到此处）
            List<CardModel> startingCards;
            try
            {
                startingCards = characterModel.StartingDeck?.ToList() ?? new List<CardModel>();
            }
            catch
            {
                startingCards = new List<CardModel>();
            }

            if (startingCards.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = L("CONFIG_DECK_EMPTY");
                emptyLabel.AddThemeFontSizeOverride("font_size", 14);
                emptyLabel.AddThemeColorOverride("font_color", StsColors.gray);
                container.AddChild(emptyLabel);
                return;
            }

            var infoLabel = new Label();
            infoLabel.Text = L("CONFIG_DEFAULT_DECK_INFO", startingCards.Count);
            infoLabel.AddThemeFontSizeOverride("font_size", 14);
            infoLabel.AddThemeColorOverride("font_color", StsColors.gray);
            container.AddChild(infoLabel);

            // 按卡牌标题分组统计数量（直接读取 CardModel.Title，兼容原版与mod卡牌）
            var counts = new Dictionary<string, int>();
            foreach (var card in startingCards)
            {
                string name;
                try { name = card.Title; }
                catch { name = card.GetType().Name; }
                counts.TryGetValue(name, out int n);
                counts[name] = n + 1;
            }

            var deckListLabel = new Label();
            deckListLabel.Text = string.Join("，", counts.Select(kv => $"{kv.Key} × {kv.Value}"));
            deckListLabel.AddThemeFontSizeOverride("font_size", 12);
            deckListLabel.AddThemeColorOverride("font_color", StsColors.gray);
            deckListLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            container.AddChild(deckListLabel);
        }
        catch
        {
            var infoLabel = new Label();
            infoLabel.Text = L("CONFIG_NO_DECK_INFO");
            infoLabel.AddThemeFontSizeOverride("font_size", 14);
            infoLabel.AddThemeColorOverride("font_color", StsColors.gray);
            container.AddChild(infoLabel);
        }
    }

    private static void ShowCustomDeckEditor(VBoxContainer container, CharacterConfig config)
    {
        var currentDeckLabel = new Label();
        currentDeckLabel.Text = L("CONFIG_CUSTOM_DECK_COUNT", config.CustomDeckCardTypes.Count);
        currentDeckLabel.AddThemeFontSizeOverride("font_size", 14);
        currentDeckLabel.AddThemeColorOverride("font_color", StsColors.gold);
        container.AddChild(currentDeckLabel);

        if (config.CustomDeckCardTypes.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = L("CONFIG_CUSTOM_DECK_EMPTY");
            emptyLabel.AddThemeFontSizeOverride("font_size", 14);
            emptyLabel.AddThemeColorOverride("font_color", StsColors.gray);
            container.AddChild(emptyLabel);
        }
        else
        {
            var cardCounts = new Dictionary<string, int>();
            foreach (var cardType in config.CustomDeckCardTypes)
            {
                cardCounts.TryGetValue(cardType, out int count);
                cardCounts[cardType] = count + 1;
            }

            var flowContainer = new VBoxContainer();
            flowContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            flowContainer.AddThemeConstantOverride("separation", 6);
            container.AddChild(flowContainer);

            var row = new HBoxContainer();
            row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddThemeConstantOverride("separation", 8);

            int itemsInRow = 0;
            int maxPerRow = 5;

            foreach (var (cardType, count) in cardCounts.OrderByDescending(kv => kv.Value))
            {
                string displayName = GetCardDisplayName(cardType);
                var cardModel = GetCardModelByTypeName(cardType);

                var cardThumb = new VBoxContainer();
                cardThumb.Alignment = BoxContainer.AlignmentMode.Center;
                cardThumb.AddThemeConstantOverride("separation", 2);
                cardThumb.CustomMinimumSize = new Vector2(110, 160);

                // 用 clip 包裹卡牌，避免卡片自然最小尺寸把格子撑大（与 CardLibraryTab 相同方案）
                var clip = new Control();
                clip.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                clip.CustomMinimumSize = new Vector2(0, 140);
                clip.ClipContents = true;
                clip.MouseFilter = Control.MouseFilterEnum.Ignore;
                cardThumb.AddChild(clip);

                if (cardModel != null)
                {
                    try
                    {
                        var displayCard = cardModel.IsMutable ? cardModel : cardModel.ToMutable();
                        var nCard = NCard.Create(displayCard);
                        if (nCard != null)
                        {
                            nCard.Scale = new Vector2(0.32f, 0.32f);
                            nCard.MouseFilter = Control.MouseFilterEnum.Ignore;
                            clip.AddChild(nCard);

                            NCard capturedCard = nCard;
                            Control capturedClip = clip;
                            nCard.Ready += () =>
                            {
                                if (GodotObject.IsInstanceValid(capturedCard))
                                    capturedCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                                Callable.From(() =>
                                {
                                    if (GodotObject.IsInstanceValid(capturedClip) && GodotObject.IsInstanceValid(capturedCard))
                                        CenterThumb(capturedClip, capturedCard, 0.32f);
                                }).CallDeferred();
                            };
                            clip.Resized += () =>
                            {
                                if (GodotObject.IsInstanceValid(capturedClip) && GodotObject.IsInstanceValid(capturedCard))
                                    CenterThumb(capturedClip, capturedCard, 0.32f);
                            };
                        }
                    }
                    catch { }
                }

                var infoRow = new HBoxContainer();
                infoRow.Alignment = BoxContainer.AlignmentMode.Center;
                infoRow.AddThemeConstantOverride("separation", 3);

                var nameLabel = new Label();
                nameLabel.Text = displayName;
                nameLabel.AddThemeFontSizeOverride("font_size", 10);
                nameLabel.AddThemeColorOverride("font_color", StsColors.cream);
                infoRow.AddChild(nameLabel);

                var countLbl = new Label();
                countLbl.Text = $"×{count}";
                countLbl.AddThemeFontSizeOverride("font_size", 10);
                countLbl.AddThemeColorOverride("font_color", StsColors.gold);
                infoRow.AddChild(countLbl);

                cardThumb.AddChild(infoRow);

                string capturedType = cardType;
                cardThumb.GuiInput += (InputEvent ev) =>
                {
                    if (ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
                    {
                        cardThumb.GetViewport()?.SetInputAsHandled();
                        string? firstMatch = config.CustomDeckCardTypes.FirstOrDefault(t => t == capturedType);
                        if (firstMatch != null)
                        {
                            config.CustomDeckCardTypes.Remove(firstMatch);
                            ModConfigManager.UpdateCharacterConfig(config);
                            RefreshContent();
                        }
                    }
                };

                row.AddChild(cardThumb);
                itemsInRow++;

                if (itemsInRow >= maxPerRow)
                {
                    flowContainer.AddChild(row);
                    row = new HBoxContainer();
                    row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                    row.AddThemeConstantOverride("separation", 8);
                    itemsInRow = 0;
                }
            }

            if (itemsInRow > 0)
            {
                flowContainer.AddChild(row);
            }

            var hintLabel = new Label();
            hintLabel.Text = L("CONFIG_CARD_REMOVE_HINT");
            hintLabel.AddThemeFontSizeOverride("font_size", 11);
            hintLabel.AddThemeColorOverride("font_color", StsColors.gray);
            flowContainer.AddChild(hintLabel);
        }

        AddDivider(container);

        var btnRow = new HBoxContainer();
        btnRow.AddThemeConstantOverride("separation", 10);
        btnRow.Alignment = BoxContainer.AlignmentMode.Center;
        container.AddChild(btnRow);

        var addBtn = CreateActionButton(L("CONFIG_OPEN_CARD_LIBRARY"), StsColors.gold);
        addBtn.CustomMinimumSize = new Vector2(150, 36);
        addBtn.Pressed += () => OpenCardLibrary(config);
        btnRow.AddChild(addBtn);

        if (config.CustomDeckCardTypes.Count > 0)
        {
            var clearBtn = CreateActionButton(L("CONFIG_CLEAR_DECK"), StsColors.red);
            clearBtn.CustomMinimumSize = new Vector2(120, 36);
            clearBtn.Pressed += () =>
            {
                config.CustomDeckCardTypes.Clear();
                ModConfigManager.UpdateCharacterConfig(config);
                RefreshContent();
            };
            btnRow.AddChild(clearBtn);
        }
    }

    /// <summary>
    /// 将卡牌缩略图居中到 clip 内（按绘制后的尺寸 300x422x缩放 计算偏移）。
    /// </summary>
    private static void CenterThumb(Control clip, Control card, float scale)
    {
        if (!GodotObject.IsInstanceValid(clip) || !GodotObject.IsInstanceValid(card)) return;
        float drawnW = 300f * scale;
        float drawnH = 422f * scale;
        card.Position = new Vector2((clip.Size.X - drawnW) / 2f, (clip.Size.Y - drawnH) / 2f);
    }

    /// <summary>
    /// 在所有已加载程序集中按名称查找类型（兼容本mod与原版卡牌，如 Wound）。
    /// </summary>
    private static Type? FindTypeInAllAssemblies(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetType(typeName);
                if (type != null) return type;
            }
            catch { }
        }

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

    private static string GetCardDisplayName(string typeName)
    {
        try
        {
            var type = FindTypeInAllAssemblies(typeName);
            if (type != null)
            {
                var cardModel = GetCardModelByType(type);
                if (cardModel != null)
                {
                    try { return cardModel.Title; } catch { }
                }
            }
        }
        catch { }
        return typeName;
    }

    private static CardModel? GetCardModelByTypeName(string typeName)
    {
        try
        {
            var type = FindTypeInAllAssemblies(typeName);
            if (type != null)
            {
                return GetCardModelByType(type);
            }
        }
        catch { }
        return null;
    }

    private static CardModel? GetCardModelByType(Type cardType)
    {
        try
        {
            var cardMethod = typeof(ModelDb).GetMethods()
                .FirstOrDefault(m => m.Name == "Card" && m.IsGenericMethodDefinition);
            if (cardMethod != null)
            {
                var genericMethod = cardMethod.MakeGenericMethod(cardType);
                return genericMethod.Invoke(null, null) as CardModel;
            }
        }
        catch { }
        return null;
    }

    private static void OpenCardLibrary(CharacterConfig config)
    {
        _cardLibraryTab = new CardLibraryTab(config, () => RefreshContent());
        _cardLibraryTab.Show();
    }

    internal static void AddCardToDeck(CharacterConfig config, string cardTypeName)
    {
        config.CustomDeckCardTypes.Add(cardTypeName);
        ModConfigManager.UpdateCharacterConfig(config);
    }

    private static List<CharacterModel> GetAllCharacters()
    {
        var characters = new List<CharacterModel>();
        try
        {
            foreach (var character in ModelDb.AllCharacters)
            {
                characters.Add(character);
            }
        }
        catch { }
        return characters;
    }

    private static void ShowNotification(string message)
    {
        GD.Print($"[ModConfig] {message}");
    }

    private static void ShowConfirmDialog(string title, string desc, Action onConfirm)
    {
        if (_layer == null || !GodotObject.IsInstanceValid(_layer)) return;

        var dialogMask = new ColorRect();
        dialogMask.Color = new Color(0f, 0f, 0f, 0.5f);
        dialogMask.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        dialogMask.MouseFilter = Control.MouseFilterEnum.Stop;
        _layer.AddChild(dialogMask);

        var dialog = new PanelContainer();
        dialog.AnchorLeft = 0.5f;
        dialog.AnchorRight = 0.5f;
        dialog.AnchorTop = 0.5f;
        dialog.AnchorBottom = 0.5f;
        dialog.OffsetLeft = -160;
        dialog.OffsetRight = 160;
        dialog.OffsetTop = -80;
        dialog.OffsetBottom = 80;
        dialog.GrowHorizontal = Control.GrowDirection.Both;
        dialog.GrowVertical = Control.GrowDirection.Both;
        dialog.MouseFilter = Control.MouseFilterEnum.Stop;

        var dialogStyle = new StyleBoxFlat();
        dialogStyle.BgColor = new Color(0.10f, 0.08f, 0.12f, 0.98f);
        dialogStyle.SetBorderWidthAll(2);
        dialogStyle.BorderColor = StsColors.gold;
        dialogStyle.SetCornerRadiusAll(8);
        dialogStyle.SetContentMarginAll(0);
        dialog.AddThemeStyleboxOverride("panel", dialogStyle);
        _layer.AddChild(dialog);

        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddThemeConstantOverride("separation", 10);
        dialog.AddChild(vbox);

        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.AddThemeColorOverride("font_color", StsColors.gold);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.AddChild(titleLabel);

        var descLabel = new Label();
        descLabel.Text = desc;
        descLabel.AddThemeFontSizeOverride("font_size", 13);
        descLabel.AddThemeColorOverride("font_color", StsColors.cream);
        descLabel.HorizontalAlignment = HorizontalAlignment.Center;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.CustomMinimumSize = new Vector2(280, 0);
        descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.AddChild(descLabel);

        var btnRow = new HBoxContainer();
        btnRow.AddThemeConstantOverride("separation", 16);
        btnRow.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(btnRow);

        void CloseDialog()
        {
            if (GodotObject.IsInstanceValid(dialogMask))
                dialogMask.QueueFree();
            if (GodotObject.IsInstanceValid(dialog))
                dialog.QueueFree();
        }

        var cancelBtn = CreateActionButton(L("CONFIG_CANCEL"), StsColors.cream);
        cancelBtn.CustomMinimumSize = new Vector2(100, 36);
        cancelBtn.Pressed += CloseDialog;
        btnRow.AddChild(cancelBtn);

        var confirmBtn = CreateActionButton(L("CONFIG_CONFIRM"), StsColors.gold);
        confirmBtn.CustomMinimumSize = new Vector2(100, 36);
        confirmBtn.Pressed += () =>
        {
            CloseDialog();
            onConfirm?.Invoke();
        };
        btnRow.AddChild(confirmBtn);
    }

    // ============ UI辅助方法 ============

    private static Label CreateSectionHeader(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", StsColors.gold);
        return label;
    }

    private static Button CreateActionButton(string text, Color? fontColor = null)
    {
        var btn = new Button();
        btn.Text = text;
        btn.AddThemeFontSizeOverride("font_size", 14);
        btn.AddThemeColorOverride("font_color", fontColor ?? StsColors.cream);
        btn.AddThemeColorOverride("font_hover_color", StsColors.gold);
        btn.AddThemeColorOverride("font_pressed_color", StsColors.gray);
        ApplyFlatStyle(btn);
        return btn;
    }

    private static Button CreateToggleButton(bool enabled, string text)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(80, 32);
        btn.AddThemeFontSizeOverride("font_size", 14);
        btn.AddThemeColorOverride("font_hover_color", StsColors.gold);
        btn.AddThemeColorOverride("font_pressed_color", StsColors.gray);
        UpdateToggleStyle(btn, enabled);
        return btn;
    }

    private static void UpdateToggleStyle(Button btn, bool enabled)
    {
        btn.AddThemeColorOverride("font_color", enabled ? StsColors.green : StsColors.cream);
        if (enabled)
        {
            btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
                new Color(0.15f, 0.25f, 0.15f, 0.9f),
                new Color(0.3f, 0.6f, 0.3f, 0.7f)));
        }
        else
        {
            btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
                new Color(0.12f, 0.10f, 0.15f, 0.85f),
                new Color(0.35f, 0.30f, 0.25f, 0.5f)));
        }
    }

    private static void ApplyFlatStyle(Button btn)
    {
        btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
            new Color(0.12f, 0.10f, 0.15f, 0.85f),
            new Color(0.35f, 0.30f, 0.25f, 0.5f)));
        btn.AddThemeStyleboxOverride("hover", CreateStyleBox(
            new Color(0.18f, 0.15f, 0.22f, 0.92f),
            StsColors.gold));
        btn.AddThemeStyleboxOverride("pressed", CreateStyleBox(
            new Color(0.08f, 0.06f, 0.10f, 0.95f),
            new Color("B89840")));
        btn.AddThemeStyleboxOverride("focus", CreateStyleBox(
            new Color(0.18f, 0.15f, 0.22f, 0.92f),
            StsColors.gold));
    }

    private static StyleBoxFlat CreateStyleBox(Color bg, Color border)
    {
        var sb = new StyleBoxFlat();
        sb.BgColor = bg;
        sb.BorderColor = border;
        sb.SetBorderWidthAll(2);
        sb.SetCornerRadiusAll(6);
        sb.SetContentMarginAll(6);
        return sb;
    }

    private static void AddDivider(VBoxContainer container)
    {
        var divider = new ColorRect();
        divider.CustomMinimumSize = new Vector2(0, 1);
        divider.Color = new Color(0.91f, 0.86f, 0.75f, 0.1f);
        divider.MouseFilter = Control.MouseFilterEnum.Ignore;
        container.AddChild(divider);
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}