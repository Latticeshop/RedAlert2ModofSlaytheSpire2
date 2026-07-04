using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;

namespace RedAlert2ModCode.UI;

/// <summary>
/// 超时空传送牌堆选择界面
/// </summary>
public sealed partial class ChronoWarpScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private readonly string _prompt;
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
            object? result = rawMethod.Invoke(locStringObj, null);
            if (result is string rawText && !string.IsNullOrEmpty(rawText))
            {
                return rawText;
            }
        }

        string toString = locStringObj.ToString() ?? string.Empty;
        if (!toString.StartsWith("MegaCrit.Sts2.Core.Localization") && !toString.Contains("LocString"))
        {
            return toString;
        }

        return string.Empty;
    }

    public enum PileChoice
    {
        Draw = 0,
        Hand = 1,
        Discard = 2
    }

    private ChronoWarpScreen(string prompt)
    {
        _prompt = prompt;
        Name = nameof(ChronoWarpScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<int?> ShowPileSelection(string prompt, Player player)
    {
        var screen = new ChronoWarpScreen(prompt);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public void Close()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(null);
        NOverlayStack.Instance?.Remove(this);
    }

    public static async Task<int?> ShowPileSelectionWithSync(string prompt, Player player)
    {
        return await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
        {
            return await ShowPileSelection(prompt, player);
        });
    }

    private void BuildUi()
    {
        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0.02f, 0.025f, 0.035f, 0.85f),
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
            Text = _prompt,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        HBoxContainer choicesRow = new()
        {
            Name = "ChoicesRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        choicesRow.AddThemeConstantOverride("separation", 20);
        root.AddChild(choicesRow);

        choicesRow.AddChild(CreatePileButton((int)PileChoice.Draw, GetLocStringText(new LocString("card_keywords", "ui.pile_draw"))));
        choicesRow.AddChild(CreatePileButton((int)PileChoice.Hand, GetLocStringText(new LocString("card_keywords", "ui.pile_hand"))));
        choicesRow.AddChild(CreatePileButton((int)PileChoice.Discard, GetLocStringText(new LocString("card_keywords", "ui.pile_discard"))));
    }

    private Button CreatePileButton(int pileChoice, string label)
    {
        Button button = new()
        {
            Name = $"PileButton_{pileChoice}",
            Text = label,
            CustomMinimumSize = new Vector2(200f, 80f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateButtonStyle(new Color(0.15f, 0.22f, 0.35f)));
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(new Color(0.2f, 0.3f, 0.45f)));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(new Color(0.1f, 0.18f, 0.28f)));
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        button.Pressed += () => OnPileSelected(pileChoice);

        return button;
    }

    private void OnPileSelected(int pileChoice)
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(pileChoice);
        NOverlayStack.Instance?.Remove(this);
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
        style.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        return style;
    }

    private StyleBoxFlat CreateButtonStyle(Color bgColor)
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
        style.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        return style;
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }

    public override void _ExitTree()
    {
        _completionSource.TrySetResult(null);
        base._ExitTree();
    }
}
