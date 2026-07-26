using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

/// <summary>
/// 选择界面 - 显示随机选项供玩家选择（工程师/间谍通用）
/// </summary>
public sealed partial class ChoiceSelectionScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<Choice?> _completionSource = new();
	private readonly List<Choice> _choices;
	private readonly string? _portraitPath;
	private readonly FactionType _faction;
	private bool _choiceLocked;

	/// <summary>
	/// 选项类型
	/// </summary>
	public enum ChoiceType
	{
		CaptureOilDerrick,      // 占领油井
		RepairBuilding,         // 修理建筑
		CaptureAirfield,        // 占领机场
		CaptureHospital,        // 占领市民医院
		CaptureWorkshop,        // 占领机械商店
		CaptureTechOutpost,     // 占领科技前哨站
		RepairBridge,           // 维修桥梁
		SurveyMineField         // 勘测矿区
	}

	/// <summary>
	/// 选项
	/// </summary>
	public sealed class Choice
	{
		public ChoiceType Type { get; set; }
		public object Title { get; set; } = string.Empty;
		public object Description { get; set; } = string.Empty;
		public int Weight { get; set; }
	}

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

	private ChoiceSelectionScreen(List<Choice> choices, string? portraitPath, FactionType faction = FactionType.Allied)
	{
		_choices = choices;
		_portraitPath = portraitPath;
		_faction = faction;
		Name = nameof(ChoiceSelectionScreen);
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		FocusMode = FocusModeEnum.All;
		BuildUi();
	}

	/// <summary>
	/// 显示选择界面（支持多人同步）
	/// </summary>
	public static async Task<Choice?> ShowSelection(List<Choice> choices, string? portraitPath, MegaCrit.Sts2.Core.Entities.Players.Player player, FactionType faction = FactionType.Allied)
	{
		var screen = new ChoiceSelectionScreen(choices, portraitPath, faction);
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

	/// <summary>
	/// 显示选择界面（支持多人同步）
	/// </summary>
	public static async Task<Choice?> ShowSelectionWithSync(List<Choice> choices, string? portraitPath, MegaCrit.Sts2.Core.Entities.Players.Player player, FactionType faction = FactionType.Allied)
	{
		List<Choice> choicesCopy = new(choices);
		
		int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
		{
			Choice? choice = await ShowSelection(choicesCopy, portraitPath, player, faction);
			return choice != null ? choicesCopy.FindIndex(c => c.Type == choice.Type) : null;
		});
		
		if (selectedIndex.HasValue && selectedIndex.Value >= 0 && selectedIndex.Value < choicesCopy.Count)
		{
			return choicesCopy[selectedIndex.Value];
		}
		
		return null;
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
            Text = GetLocStringText(new LocString("card_keywords", "ui.spy.deploy.title")),
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
    private Button CreateChoiceButton(Choice choice, int index)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{index}",
            CustomMinimumSize = new Vector2(280f, 320f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        // 设置按钮样式
        button.AddThemeStyleboxOverride("normal", CreateCardStyle(GetButtonColor()));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(GetButtonHoverColor()));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(GetButtonPressedColor()));

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

        // 添加图片
	string iconPath = _portraitPath ?? "res://RedAlert2ModResources/images/packed/card_portraits/allies/aengicon.png";
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
            Text = GetLocStringText(choice.Title),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f)); // 金色
        content.AddChild(title);

        // 添加描述
        string descText = GetLocStringText(choice.Description);
        descText = descText.Replace("[gold]", "").Replace("[/gold]", "");
        
        Label description = new()
        {
            Text = descText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
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
    private void OnChoiceSelected(Choice choice)
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
        style.BorderColor = GetFactionColor();
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
        style.BorderColor = GetFactionColor();
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

    private Color GetFactionColor()
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
        return _faction switch
        {
            FactionType.Soviet => new Color(0.2f, 0.08f, 0.08f, 0.9f),
            FactionType.Yuri => new Color(0.2f, 0.08f, 0.2f, 0.9f),
            _ => new Color(0.1f, 0.15f, 0.25f, 0.9f)
        };
    }

    private Color GetButtonHoverColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.3f, 0.12f, 0.12f, 0.95f),
            FactionType.Yuri => new Color(0.3f, 0.12f, 0.3f, 0.95f),
            _ => new Color(0.15f, 0.22f, 0.35f, 0.95f)
        };
    }

    private Color GetButtonPressedColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.15f, 0.06f, 0.06f, 0.98f),
            FactionType.Yuri => new Color(0.15f, 0.06f, 0.15f, 0.98f),
            _ => new Color(0.08f, 0.12f, 0.2f, 0.98f)
        };
    }
}
