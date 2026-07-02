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
/// 工程师选择界面 - 显示随机选项供玩家选择
/// </summary>
public sealed partial class EngineerChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<EngineerChoice?> _completionSource = new();
	private readonly List<EngineerChoice> _choices;
	private readonly string? _engineerPortraitPath;
	private readonly FactionType _faction;
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

	private EngineerChoiceScreen(List<EngineerChoice> choices, string? engineerPortraitPath, FactionType faction = FactionType.Allied)
	{
		_choices = choices;
		_engineerPortraitPath = engineerPortraitPath;
		_faction = faction;
		Name = nameof(EngineerChoiceScreen);
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		FocusMode = FocusModeEnum.All;
		BuildUi();
	}

	/// <summary>
	/// 显示选择界面
	/// </summary>
	public static async Task<EngineerChoice?> ShowSelection(List<EngineerChoice> choices, string? engineerPortraitPath = null, FactionType faction = FactionType.Allied)
	{
		var screen = new EngineerChoiceScreen(choices, engineerPortraitPath, faction);
		NOverlayStack.Instance?.Push(screen);
		return await screen._completionSource.Task;
	}

	/// <summary>
	/// 显示选择界面（支持多人同步）
	/// </summary>
	public static async Task<EngineerChoice?> ShowSelectionWithSync(List<EngineerChoice> choices, string? engineerPortraitPath, MegaCrit.Sts2.Core.Entities.Players.Player player, FactionType faction = FactionType.Allied)
	{
		EngineerChoice? selectedChoice = null;
		
		var runManagerType = Type.GetType("MegaCrit.Sts2.Core.Runs.RunManager, MegaCrit.Sts2.Core");
		if (runManagerType == null)
		{
			return await ShowSelection(choices, engineerPortraitPath, faction);
		}
		
		var instanceProp = runManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		if (instanceProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var runManager = instanceProp.GetValue(null);
		if (runManager == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var netServiceProp = runManagerType.GetProperty("NetService");
		if (netServiceProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var netService = netServiceProp.GetValue(runManager);
		if (netService == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var typeProp = netService.GetType().GetProperty("Type");
		if (typeProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var netType = typeProp.GetValue(netService);
		if (netType == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		string typeName = netType.ToString();
		if (typeName is not "Host" and not "Client")
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var synchronizerProp = runManagerType.GetProperty("PlayerChoiceSynchronizer");
		if (synchronizerProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var synchronizer = synchronizerProp.GetValue(runManager);
		if (synchronizer == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var reserveMethod = synchronizer.GetType().GetMethod("ReserveChoiceId");
		if (reserveMethod == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		uint choiceId = (uint)reserveMethod.Invoke(synchronizer, new[] { player });
		
		var serviceNetIdProp = netService.GetType().GetProperty("NetId");
		if (serviceNetIdProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		var playerNetIdProp = player.GetType().GetProperty("NetId");
		if (playerNetIdProp == null)
			return await ShowSelection(choices, engineerPortraitPath, faction);
		
		ulong serviceNetId = (ulong)serviceNetIdProp.GetValue(netService);
		ulong playerNetId = (ulong)playerNetIdProp.GetValue(player);
		bool isLocalPlayer = playerNetId == serviceNetId;
		
		if (isLocalPlayer)
		{
			selectedChoice = await ShowSelection(choices, engineerPortraitPath, faction);
			
			try
			{
				int selectedIndex = selectedChoice != null ? choices.FindIndex(c => c.Type == selectedChoice.Type) : -1;
				var choiceResult = new MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult();
				var choiceTypeField = choiceResult.GetType().GetField("_choiceType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var payloadField = choiceResult.GetType().GetField("_payload", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				
				if (choiceTypeField != null)
					choiceTypeField.SetValue(choiceResult, "RedAlert2ModEngineerChoice");
				if (payloadField != null)
					payloadField.SetValue(choiceResult, selectedIndex.ToString());
				
				var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
				if (syncMethod != null)
				{
					syncMethod.Invoke(synchronizer, new object[] { player, choiceId, choiceResult });
				}
			}
			catch
			{
			}
			
			return selectedChoice;
		}
		else
		{
			try
			{
				var eventInfo = synchronizer.GetType().GetEvent("PlayerChoiceReceived");
				if (eventInfo != null)
				{
					var tcs = new TaskCompletionSource<MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult>();
					
					System.Reflection.MethodInfo handlerMethod = typeof(EngineerChoiceScreen).GetMethod("OnRemoteEngineerChoiceReceived", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
					if (handlerMethod != null)
					{
						var handler = System.Delegate.CreateDelegate(eventInfo.EventHandlerType, handlerMethod);
						eventInfo.AddEventHandler(synchronizer, handler);
						
						var receivedChoice = await tcs.Task;
						eventInfo.RemoveEventHandler(synchronizer, handler);
						
						var payloadField = receivedChoice.GetType().GetField("_payload", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
						var payload = payloadField?.GetValue(receivedChoice) as string;
						
						if (int.TryParse(payload, out int selectedIndex) && selectedIndex >= 0 && selectedIndex < choices.Count)
						{
							return choices[selectedIndex];
						}
					}
				}
			}
			catch
			{
			}
			
			return choices.FirstOrDefault();
		}
	}
	
	private static void OnRemoteEngineerChoiceReceived(object player, uint choiceId, object result)
	{
		return;
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
            Text = GetLocStringText(new LocString("card_keywords", "engineer_choice.title")),
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
            Text = GetLocStringText(choice.Title),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f)); // 金色
        content.AddChild(title);

        // 添加描述
        Label description = new()
        {
            Text = GetLocStringText(choice.Description),
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
