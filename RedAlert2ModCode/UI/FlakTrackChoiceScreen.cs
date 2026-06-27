#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class FlakTrackChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<ChoiceType?> _completionSource = new();
    private bool _choiceLocked;

    public enum ChoiceType
    {
        Deploy,
        Attack
    }

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private FlakTrackChoiceScreen()
    {
        Name = nameof(FlakTrackChoiceScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    private string _title = "选择行动";
    private string _deployTitle = "部署";
    private string _deployDesc = "存储当前手牌中的士兵单位";
    private string _attackTitle = "攻击";
    private string _attackDesc = "获得敏捷和攻击";

    public static async Task<ChoiceType?> ShowSelection()
    {
        var screen = new FlakTrackChoiceScreen();
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
    }

    public static async Task<ChoiceType?> ShowSelection(string title, string deployTitle, string deployDesc, string attackTitle, string attackDesc)
    {
        var screen = new FlakTrackChoiceScreen();
        screen._title = title;
        screen._deployTitle = deployTitle;
        screen._deployDesc = deployDesc;
        screen._attackTitle = attackTitle;
        screen._attackDesc = attackDesc;
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
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
            CustomMinimumSize = new Vector2(800f, 380f)
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
            Text = _title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 26);
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

        choicesRow.AddChild(CreateChoiceButton(_deployTitle, _deployDesc, ChoiceType.Deploy));
        choicesRow.AddChild(CreateChoiceButton(_attackTitle, _attackDesc, ChoiceType.Attack));
    }

    private Button CreateChoiceButton(string title, string description, ChoiceType type)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{type}",
            CustomMinimumSize = new Vector2(300f, 220f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.12f, 0.18f, 0.28f, 0.9f)));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.18f, 0.26f, 0.4f, 0.95f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.1f, 0.14f, 0.22f, 0.98f)));

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

        Label titleLabel = new()
        {
            Text = title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        content.AddChild(titleLabel);

        Label descLabel = new()
        {
            Text = description,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        content.AddChild(descLabel);

        button.Pressed += () => OnChoiceSelected(type);

        return button;
    }

    private void OnChoiceSelected(ChoiceType type)
    {
        if (_choiceLocked)
            return;

        _choiceLocked = true;
        _completionSource.SetResult(type);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = FactionHelper.GetFactionBorderColor();
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
        style.BorderColor = FactionHelper.GetFactionBorderColor();
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
}
