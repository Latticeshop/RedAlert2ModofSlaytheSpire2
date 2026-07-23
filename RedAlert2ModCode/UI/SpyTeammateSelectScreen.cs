#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class SpyTeammateSelectScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<Player?> _completionSource = new();
    private readonly List<Player> _teammates;
    private readonly FactionType _faction;
    private bool _choiceLocked;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private SpyTeammateSelectScreen(List<Player> teammates, FactionType faction)
    {
        _teammates = teammates;
        _faction = faction;
        Name = nameof(SpyTeammateSelectScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<Player?> ShowSelection(List<Player> teammates, Player player, FactionType faction = FactionType.Allied)
    {
        var screen = new SpyTeammateSelectScreen(teammates, faction);
        NOverlayStack.Instance?.Push(screen);

        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }

        return await screen._completionSource.Task;
    }

    public static async Task<Player?> ShowSelectionWithSync(List<Player> teammates, Player player, FactionType faction = FactionType.Allied)
    {
        List<Player> teammatesCopy = new(teammates);

        int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
        {
            Player? choice = await ShowSelection(teammatesCopy, player, faction);
            return choice != null ? teammatesCopy.FindIndex(p => p.NetId == choice.NetId) : null;
        });

        if (selectedIndex.HasValue && selectedIndex.Value >= 0 && selectedIndex.Value < teammatesCopy.Count)
        {
            return teammatesCopy[selectedIndex.Value];
        }

        return null;
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
            CustomMinimumSize = new Vector2(700f, 350f)
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
            Text = GetLocStringText(new LocString("card_keywords", "ui.spy.select_teammate")),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 26);
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.6f));
        root.AddChild(title);

        HBoxContainer teammateRow = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        teammateRow.AddThemeConstantOverride("separation", 20);
        root.AddChild(teammateRow);

        if (_teammates.Count == 0)
        {
            Label noTeammate = new()
            {
                Text = "没有队友",
                HorizontalAlignment = HorizontalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            noTeammate.AddThemeFontSizeOverride("font_size", 18);
            noTeammate.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 0.4f));
            teammateRow.AddChild(noTeammate);
        }
        else
        {
            foreach (var teammate in _teammates.Select((p, idx) => (Player: p, Index: idx)))
            {
                Button btn = CreateTeammateButton(teammate.Player, teammate.Index);
                teammateRow.AddChild(btn);
            }
        }

        Button cancelBtn = new()
        {
            Text = "取消",
            CustomMinimumSize = new Vector2(120f, 45f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        cancelBtn.AddThemeFontSizeOverride("font_size", 16);
        cancelBtn.Pressed += Close;
        root.AddChild(cancelBtn);
    }

    private Button CreateTeammateButton(Player teammate, int index)
    {
        Button button = new()
        {
            Name = $"TeammateButton_{index}",
            CustomMinimumSize = new Vector2(150f, 50f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.2f, 0.8f)));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));

        string playerName = PlatformUtil.GetPlayerNameRaw(RunManager.Instance.NetService.Platform, teammate.NetId);
        button.Text = playerName ?? teammate.Character?.GetType().Name ?? "Unknown";
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));

        button.Pressed += () => OnTeammateSelected(teammate);

        return button;
    }

    private void OnTeammateSelected(Player teammate)
    {
        if (_choiceLocked) return;

        _choiceLocked = true;
        _completionSource.SetResult(teammate);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

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