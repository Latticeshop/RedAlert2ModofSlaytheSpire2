#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class SpyInfiltrateScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private readonly List<(Type PowerType, string Title, string Description, string IconPath)> _buildingOptions;
    private readonly FactionType _faction;
    private bool _choiceLocked;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private string GetLocStringText(object? locStringObj)
    {
        if (locStringObj == null) return string.Empty;
        if (locStringObj is string str) return str;

        System.Reflection.MethodInfo? formatMethod = locStringObj.GetType().GetMethod("GetFormattedText");
        if (formatMethod != null)
        {
            try
            {
                object? result = formatMethod.Invoke(locStringObj, null);
                if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
                {
                    return formattedText;
                }
            }
            catch { }
        }

        System.Reflection.MethodInfo? rawMethod = locStringObj.GetType().GetMethod("GetRawText");
        if (rawMethod != null)
        {
            try
            {
                object? result = rawMethod.Invoke(locStringObj, null);
                if (result is string rawText && !string.IsNullOrEmpty(rawText))
                {
                    return rawText;
                }
            }
            catch { }
        }

        string toString = locStringObj.ToString() ?? string.Empty;
        if (!toString.StartsWith("MegaCrit.Sts2.Core.Localization") && !toString.Contains("LocString"))
        {
            return toString;
        }

        return string.Empty;
    }

    private SpyInfiltrateScreen(List<(Type PowerType, string Title, string Description, string IconPath)> buildingOptions, FactionType faction)
    {
        _buildingOptions = buildingOptions;
        _faction = faction;
        Name = nameof(SpyInfiltrateScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<int?> ShowSelection(List<(Type PowerType, string Title, string Description, string IconPath)> buildingOptions, Player player, FactionType faction = FactionType.Allied)
    {
        var screen = new SpyInfiltrateScreen(buildingOptions, faction);
        NOverlayStack.Instance?.Push(screen);

        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }

        return await screen._completionSource.Task;
    }

    public static async Task<int?> ShowSelectionWithSync(List<(Type PowerType, string Title, string Description, string IconPath)> buildingOptions, Player player, FactionType faction = FactionType.Allied)
    {
        List<(Type PowerType, string Title, string Description, string IconPath)> optionsCopy = new(buildingOptions);

        int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
        {
            return await ShowSelection(optionsCopy, player, faction);
        });

        return selectedIndex;
    }

    public void Close()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(null);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private void BuildUi()
    {
        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0.02f, 0.025f, 0.035f, 0.8f),
            MouseFilter = MouseFilterEnum.Stop
        };
        backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        CenterContainer center = new() { Name = "Center" };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        PanelContainer panel = new()
        {
            Name = "ContentPanel",
            CustomMinimumSize = new Vector2(1100f, 450f)
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        center.AddChild(panel);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 30);
        margin.AddThemeConstantOverride("margin_right", 30);
        margin.AddThemeConstantOverride("margin_top", 40);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        VBoxContainer root = new() { Alignment = BoxContainer.AlignmentMode.Center };
        root.AddThemeConstantOverride("separation", 15);
        margin.AddChild(root);

        Label title = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.spy.infiltrate_title")),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        ScrollContainer scrollContainer = new()
        {
            Name = "CardScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(1000f, 300f),
            MouseFilter = MouseFilterEnum.Pass,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        root.AddChild(scrollContainer);

        HBoxContainer cardsRow = new()
        {
            Name = "CardsRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        cardsRow.AddThemeConstantOverride("separation", 15);
        scrollContainer.AddChild(cardsRow);

        foreach (var (powerType, titleText, descText, iconPath) in _buildingOptions.Select((opt, idx) => (opt.PowerType, opt.Title, opt.Description, opt.IconPath)))
        {
            cardsRow.AddChild(CreateBuildingButton(titleText, descText, iconPath, _buildingOptions.FindIndex(o => o.PowerType == powerType)));
        }

        ScrollDragHelper.EnableDragScroll(scrollContainer);
    }

    private Button CreateBuildingButton(string title, string description, string iconPath, int index)
    {
        Button button = new()
        {
            Name = $"BuildingButton_{index}",
            CustomMinimumSize = new Vector2(240f, 260f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.2f, 0.8f)));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));

        MarginContainer contentMargin = new();
        contentMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddThemeConstantOverride("margin_left", 12);
        contentMargin.AddThemeConstantOverride("margin_right", 12);
        contentMargin.AddThemeConstantOverride("margin_top", 12);
        contentMargin.AddThemeConstantOverride("margin_bottom", 12);
        button.AddChild(contentMargin);

        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 4);
        contentMargin.AddChild(content);

        if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
        {
            TextureRect texture = new()
            {
                Texture = ResourceLoader.Load<Texture2D>(iconPath),
                CustomMinimumSize = new Vector2(120f, 120f),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            content.AddChild(texture);
        }

        Label name = new()
        {
            Text = title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        content.AddChild(name);

        Label desc = new()
        {
            Text = description,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        desc.AddThemeFontSizeOverride("font_size", 13);
        desc.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        content.AddChild(desc);

        button.Pressed += () => OnBuildingSelected(index);

        return button;
    }

    private void OnBuildingSelected(int index)
    {
        if (_choiceLocked) return;

        _choiceLocked = true;
        _completionSource.SetResult(index);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private Color GetBorderColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.9f, 0.4f, 0.4f),
            FactionType.Yuri => new Color(0.8f, 0.4f, 1f),
            _ => new Color(0.4f, 0.6f, 0.9f)
        };
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = new Color(0.08f, 0.1f, 0.14f, 0.92f);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = GetBorderColor();
        return style;
    }

    private StyleBoxFlat CreateCardStyle(Color bgColor)
    {
        StyleBoxFlat style = new();
        style.BgColor = bgColor;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = GetBorderColor();
        return style;
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }

    public override void _ExitTree()
    {
        _completionSource.TrySetCanceled();
        base._ExitTree();
    }
}