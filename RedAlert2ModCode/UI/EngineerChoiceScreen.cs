using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

/// <summary>
/// 工程师选择界面 - 显示随机选项供玩家选择
/// </summary>
public sealed partial class EngineerChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<EngineerChoice?> _completionSource = new();
	private readonly List<EngineerChoice> _choices;
	private readonly string? _engineerPortraitPath;
	private bool _choiceLocked;

	/// <summary>
	/// 工程师选项类型
	/// </summary>
	public enum ChoiceType
	{
		CaptureOilDerrick,      // 占领油井
		RepairBuilding,         // 修理建筑
		CaptureAirfield,        // 占领机场
		CaptureHospital,        // 占领市民医院
		CaptureWorkshop,        // 占领机械商店
		CaptureTechOutpost,     // 占领科技前哨站
		RepairBridge            // 维修桥梁
	}

	/// <summary>
	/// 工程师选项
	/// </summary>
	public sealed class EngineerChoice
	{
		public ChoiceType Type { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int Weight { get; set; }
	}

	public NetScreenType ScreenType => NetScreenType.Rewards;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => null;

	private EngineerChoiceScreen(List<EngineerChoice> choices, string? engineerPortraitPath)
	{
		_choices = choices;
		_engineerPortraitPath = engineerPortraitPath;
		Name = nameof(EngineerChoiceScreen);
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		FocusMode = FocusModeEnum.All;
		BuildUi();
	}

	/// <summary>
	/// 显示选择界面
	/// </summary>
	public static async Task<EngineerChoice?> ShowSelection(List<EngineerChoice> choices, string? engineerPortraitPath = null)
	{
		var screen = new EngineerChoiceScreen(choices, engineerPortraitPath);
		NOverlayStack.Instance?.Push(screen);
		return await screen._completionSource.Task;
	}

    /// <summary>
    /// 构建UI界面
    /// </summary>
    private void BuildUi()
    {
        // 创建半透明背景
        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0.02f, 0.025f, 0.035f, 0.8f),
            MouseFilter = MouseFilterEnum.Stop
        };
        backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        // 创建中心容器
        CenterContainer center = new() { Name = "Center" };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        // 创建面板容器
        PanelContainer panel = new()
        {
            Name = "ContentPanel",
            CustomMinimumSize = new Vector2(1100f, 480f)
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        center.AddChild(panel);

        // 创建边距容器
        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 30);
        margin.AddThemeConstantOverride("margin_right", 30);
        margin.AddThemeConstantOverride("margin_top", 40);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        // 创建根布局
        VBoxContainer root = new() { Alignment = BoxContainer.AlignmentMode.Center };
        root.AddThemeConstantOverride("separation", 15);
        margin.AddChild(root);

        // 创建标题
        Label title = new()
        {
            Text = "选择一个指令",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        // 创建选项容器
        HBoxContainer choicesRow = new()
        {
            Name = "ChoicesRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        choicesRow.AddThemeConstantOverride("separation", 20);
        root.AddChild(choicesRow);

        // 创建选项按钮
        foreach (var choice in _choices.Select((c, idx) => (Choice: c, Index: idx)))
        {
            choicesRow.AddChild(CreateChoiceButton(choice.Choice, choice.Index));
        }
    }

    /// <summary>
    /// 创建选项按钮
    /// </summary>
    private Button CreateChoiceButton(EngineerChoice choice, int index)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{index}",
            CustomMinimumSize = new Vector2(280f, 320f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        // 设置按钮样式
        button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.25f, 0.9f)));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.22f, 0.35f, 0.95f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.2f, 0.98f)));

        // 创建内容边距
        MarginContainer contentMargin = new();
        contentMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddThemeConstantOverride("margin_left", 12);
        contentMargin.AddThemeConstantOverride("margin_right", 12);
        contentMargin.AddThemeConstantOverride("margin_top", 12);
        contentMargin.AddThemeConstantOverride("margin_bottom", 12);
        button.AddChild(contentMargin);

        // 创建垂直布局
        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 8);
        contentMargin.AddChild(content);

        // 添加工程师图片
	string iconPath = _engineerPortraitPath ?? "res://RedAlert2ModResources/images/packed/card_portraits/allies/aengicon.png";
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

        // 添加标题（金色更醒目）
        Label title = new()
        {
            Text = choice.Title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f)); // 金色
        content.AddChild(title);

        // 添加描述
        Label description = new()
        {
            Text = choice.Description,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        description.AddThemeFontSizeOverride("font_size", 14);
        description.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        content.AddChild(description);

        // 添加点击事件
        button.Pressed += () => OnChoiceSelected(choice);

        return button;
    }

    /// <summary>
    /// 选择选项
    /// </summary>
    private void OnChoiceSelected(EngineerChoice choice)
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        // 使用 TrySetResult 避免重复完成任务的异常
        _completionSource.TrySetResult(choice);
        NOverlayStack.Instance?.Remove(this);
    }

    /// <summary>
    /// 创建面板样式
    /// </summary>
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
        style.BorderColor = FactionHelper.GetFactionBorderColor();
        return style;
    }

    /// <summary>
    /// 创建卡牌样式
    /// </summary>
    private StyleBoxFlat CreateCardStyle(Color bgColor)
    {
        StyleBoxFlat style = new();
        style.BgColor = bgColor;
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = FactionHelper.GetFactionBorderColor();
        return style;
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }

    public override void _ExitTree()
    {
        // 使用 TrySetCanceled 避免在任务已完成时抛出异常
        _completionSource.TrySetCanceled();
        base._ExitTree();
    }
}
