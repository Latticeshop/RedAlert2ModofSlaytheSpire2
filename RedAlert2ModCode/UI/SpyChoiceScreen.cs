#nullable enable

using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class SpyChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private bool _choiceLocked;
    private FactionType _faction = FactionType.Allied;

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

    private SpyChoiceScreen(FactionType faction = FactionType.Allied)
    {
        _faction = faction;
        Name = nameof(SpyChoiceScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<int?> ShowSelection(Player player, FactionType faction = FactionType.Allied)
    {
        var screen = new SpyChoiceScreen(faction);
        NOverlayStack.Instance?.Push(screen);

        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }

        return await screen._completionSource.Task;
    }

    public static async Task<int?> ShowSelectionWithSync(Player player, FactionType faction = FactionType.Allied)
    {
        return await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
        {
            return await ShowSelection(player, faction);
        });
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
            CustomMinimumSize = new Vector2(800f, 350f)
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        center.AddChild(panel);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 30);
        margin.AddThemeConstantOverride("margin_right", 30);
        margin.AddThemeConstantOverride("margin_top", 30);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        VBoxContainer root = new() { Alignment = BoxContainer.AlignmentMode.Center };
        root.AddThemeConstantOverride("separation", 20);
        margin.AddChild(root);

        Label title = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.spy.title")),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        HBoxContainer choicesRow = new()
        {
            Name = "ChoicesRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        choicesRow.AddThemeConstantOverride("separation", 30);
        root.AddChild(choicesRow);

        choicesRow.AddChild(CreateChoiceButton(0, "ui.spy.attack_title", "ui.spy.attack_desc"));
        choicesRow.AddChild(CreateChoiceButton(1, "ui.spy.deploy_title", "ui.spy.deploy_desc"));
    }

    private Button CreateChoiceButton(int index, string titleKey, string descKey)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{index}",
            CustomMinimumSize = new Vector2(300f, 200f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(GetButtonColor()));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(GetButtonHoverColor()));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(GetSecondaryColor()));

        MarginContainer contentMargin = new();
        contentMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddThemeConstantOverride("margin_left", 15);
        contentMargin.AddThemeConstantOverride("margin_right", 15);
        contentMargin.AddThemeConstantOverride("margin_top", 15);
        contentMargin.AddThemeConstantOverride("margin_bottom", 15);
        button.AddChild(contentMargin);

        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 8);
        contentMargin.AddChild(content);

        Label titleLabel = new Label()
        {
            Text = GetLocStringText(new LocString("card_keywords", titleKey)),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", GetPrimaryColor());
        content.AddChild(titleLabel);

        Label descLabel = new Label()
        {
            Text = GetLocStringText(new LocString("card_keywords", descKey)),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        content.AddChild(descLabel);

        button.Pressed += () => OnChoiceSelected(index);

        return button;
    }

    private Color GetPrimaryColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.9f, 0.4f, 0.4f),
            FactionType.Yuri => new Color(0.8f, 0.4f, 1f),
            _ => new Color(0.4f, 0.6f, 0.9f)
        };
    }

    private Color GetSecondaryColor()
    {
        return new Color(0.08f, 0.1f, 0.14f, 0.92f);
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

    private Color GetButtonColor()
    {
        return new Color(0.1f, 0.15f, 0.2f, 0.8f);
    }

    private Color GetButtonHoverColor()
    {
        return new Color(0.15f, 0.2f, 0.28f, 0.9f);
    }

    private void OnChoiceSelected(int index)
    {
        if (_choiceLocked)
            return;

        _choiceLocked = true;
        _completionSource.SetResult(index);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = GetSecondaryColor();
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
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = GetBorderColor();
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
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