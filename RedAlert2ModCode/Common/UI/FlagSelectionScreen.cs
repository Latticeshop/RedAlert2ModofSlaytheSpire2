using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.UI;

internal sealed partial class FlagSelectionScreen : Control, IOverlayScreen, IScreenContext
{
	private const string SkipButtonScenePath = "res://scenes/ui/choice_selection_skip_button.tscn";
	private const string LocTable = "relics";

	private readonly TaskCompletionSource<RelicModel?> _completionSource = new();
	private readonly FlagManager.Faction _faction;
	private readonly List<Control> _holders = new();
	private readonly string _titleOverride;

	private NChoiceSelectionSkipButton? _skipButton;
	private OptionButton? _customOptionButton;
	private VBoxContainer? _customContainer;
	private bool _isClosed;

	public NetScreenType ScreenType => NetScreenType.Rewards;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => _holders.FirstOrDefault();

	private FlagSelectionScreen(FlagManager.Faction faction, string titleOverride)
	{
		_faction = faction;
		_titleOverride = titleOverride;
		Name = nameof(FlagSelectionScreen);
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		FocusMode = FocusModeEnum.All;
		Visible = true;
		BuildUi();
	}

	public static FlagSelectionScreen Create(FlagManager.Faction faction, string titleOverride = "")
	{
		return new FlagSelectionScreen(faction, titleOverride);
	}

	private void BuildUi()
	{
		VBoxContainer root = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		root.AddThemeConstantOverride("separation", 20);
		root.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(root);

		root.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

		MegaLabel title = new()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MaxFontSize = 56,
			MinFontSize = 38,
			Position = new Vector2(0f, -34f)
		};
		ApplyDefaultMegaLabelTheme(title);
		title.Modulate = Colors.White;
		string titleText = string.IsNullOrEmpty(_titleOverride)
			? "选择你的国家"
			: _titleOverride;
		title.SetTextAutoSize(titleText);
		root.AddChild(title);

		HBoxContainer columns = new()
		{
			Alignment = BoxContainer.AlignmentMode.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ShrinkCenter,
			Position = new Vector2(0f, -20f)
		};
		columns.AddThemeConstantOverride("separation", 28);
		root.AddChild(columns);

		List<RelicModel> randomFlags = FlagManager.GetRandomFlags(_faction, 3);
		foreach (RelicModel flag in randomFlags)
		{
			VBoxContainer option = new()
			{
				CustomMinimumSize = new Vector2(170f, 160f),
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter
			};
			option.AddThemeConstantOverride("separation", 6);
			columns.AddChild(option);

			CenterContainer buttonCenter = new()
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			option.AddChild(buttonCenter);

			Button button = new()
			{
				CustomMinimumSize = new Vector2(120f, 96f),
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			button.Text = "";
			button.Flat = true;
			StyleBoxEmpty empty = new();
			button.AddThemeStyleboxOverride("normal", empty);
			button.AddThemeStyleboxOverride("hover", empty);
			button.AddThemeStyleboxOverride("pressed", empty);
			button.AddThemeStyleboxOverride("focus", empty);
			buttonCenter.AddChild(button);

			TextureRect flagIcon = CreateFlagIcon(flag);
			button.AddChild(flagIcon);

			MegaLabel flagLabel = new()
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				MaxFontSize = 22,
				MinFontSize = 14
			};
			ApplyDefaultMegaLabelTheme(flagLabel);
			flagLabel.Modulate = Colors.White;
			flagLabel.SetTextAutoSize(flag.Title.GetFormattedText());
			option.AddChild(flagLabel);

			button.Pressed += () => OnFlagSelected(flag);
			button.MouseEntered += () => ShowHoverTip(button, flagIcon, flag);
			button.MouseExited += () => HideHoverTip(button, flagIcon);
			_holders.Add(button);
		}

		CenterContainer customCenter = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		root.AddChild(customCenter);

		_customContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(250f, 0f),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter
		};
		_customContainer.AddThemeConstantOverride("separation", 8);
		customCenter.AddChild(_customContainer);

		MegaLabel customLabel = new()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MaxFontSize = 22,
			MinFontSize = 14,
			Position = new Vector2(0f, -12f)
		};
		ApplyDefaultMegaLabelTheme(customLabel);
		customLabel.Modulate = Colors.White;
		customLabel.SetTextAutoSize("自定义");
		_customContainer.AddChild(customLabel);

		_customOptionButton = new OptionButton
		{
			CustomMinimumSize = new Vector2(180f, 40f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		List<RelicModel> allFlags = FlagManager.GetAllFlags(_faction);
		_customOptionButton.AddItem("———", -1);
		for (int i = 0; i < allFlags.Count; i++)
		{
			_customOptionButton.AddItem(allFlags[i].Title.GetFormattedText(), i);
		}
		_customOptionButton.ItemSelected += index =>
		{
			int itemId = _customOptionButton.GetItemId((int)index);
			if (itemId >= 0 && itemId < allFlags.Count)
			{
				OnFlagSelected(allFlags[itemId]);
			}
		};
		_customContainer.AddChild(_customOptionButton);

		root.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

		PackedScene? skipScene = ResourceLoader.Load<PackedScene>(SkipButtonScenePath, cacheMode: ResourceLoader.CacheMode.Reuse);
		if (skipScene != null)
		{
			NChoiceSelectionSkipButton skipButton = skipScene.Instantiate<NChoiceSelectionSkipButton>();
			skipButton.Name = "FlagSkipButton";
			if (skipButton.GetNodeOrNull("Label") is GodotObject labelNode)
			{
				labelNode.Call("SetTextAutoSize", "跳过");
			}
			skipButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => OnSkipPressed()));
			AddChild(skipButton);
			skipButton.Enable();
			skipButton.MouseFilter = MouseFilterEnum.Stop;
			skipButton.FocusMode = FocusModeEnum.All;
			EnsureSkipButtonSize(skipButton);
			_skipButton = skipButton;
			QueueUpdateSkipButtonLayout();
		}
	}

	private void OnFlagSelected(RelicModel flag)
	{
		GD.Print($"[RedAlert2Mod][UI] OnFlagSelected: {flag.Title.GetFormattedText()}, Id={flag.Id.Entry}");
		bool result = _completionSource.TrySetResult(flag);
		GD.Print($"[RedAlert2Mod][UI] TrySetResult returned: {result}");
	}

	private void OnSkipPressed()
	{
		Log.Info($"[RedAlert2Mod][UI] Flag selection skipped.");
		_completionSource.TrySetResult(null);
		CloseSelectionScreen();
	}

	public async Task<RelicModel?> FlagSelected(bool closeOnSelection = true)
	{
		RelicModel? result = await _completionSource.Task;
		if (closeOnSelection)
		{
			CloseSelectionScreen();
		}
		return result;
	}

	public void CloseSelectionScreen()
	{
		if (_isClosed) return;
		_isClosed = true;
		NOverlayStack.Instance?.Remove(this);
	}

	public void AfterOverlayOpened()
	{
		Modulate = Colors.White;
		Visible = true;
		QueueUpdateSkipButtonLayout();
	}

	public void AfterOverlayClosed()
	{
		if (!IsInstanceValid(this)) return;
		QueueFree();
	}

	public void AfterOverlayShown()
	{
		Visible = true;
		QueueUpdateSkipButtonLayout();
	}

	public void AfterOverlayHidden()
	{
		Visible = false;
	}

	private void QueueUpdateSkipButtonLayout()
	{
		Callable.From(UpdateSkipButtonLayout).CallDeferred();
	}

	private void UpdateSkipButtonLayout()
	{
		if (!IsInstanceValid(_skipButton)) return;
		if (!IsInsideTree()) return;

		EnsureSkipButtonSize(_skipButton!);
		Vector2 viewportSize = GetViewportRect().Size;
		Vector2 size = _skipButton!.Size == Vector2.Zero ? _skipButton.GetCombinedMinimumSize() : _skipButton.Size;
		_skipButton.GlobalPosition = GlobalPosition + new Vector2((viewportSize.X - size.X) * 0.5f, viewportSize.Y - size.Y - 56f);
	}

	private static void EnsureSkipButtonSize(NChoiceSelectionSkipButton skipButton)
	{
		Vector2 minSize = skipButton.GetCombinedMinimumSize();
		if (skipButton.Size == Vector2.Zero && minSize != Vector2.Zero)
		{
			skipButton.Size = minSize;
		}
	}

	private static TextureRect CreateFlagIcon(RelicModel flag)
	{
		TextureRect icon = new()
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = new Vector2(100f, 70f),
			Size = new Vector2(100f, 70f),
			Position = new Vector2(10f, 13f),
			PivotOffset = new Vector2(50f, 35f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		try
		{
			if (flag.Icon != null)
			{
				icon.Texture = flag.Icon;
			}
		}
		catch (Exception ex)
		{
			Log.Warn($"[RedAlert2Mod] Failed to load flag icon: {ex.GetType().Name}");
		}
		return icon;
	}

	private static void ShowHoverTip(Control owner, Control flagIcon, RelicModel flag)
	{
		flagIcon.Scale = Vector2.One * 1.15f;
		NHoverTipSet? tipSet = NHoverTipSet.CreateAndShow(owner, flag.HoverTips, HoverTip.GetHoverTipAlignment(owner));
		tipSet?.SetFollowOwner();
	}

	private static void HideHoverTip(Control owner, Control flagIcon)
	{
		flagIcon.Scale = Vector2.One;
		NHoverTipSet.Remove(owner);
	}

	private static void ApplyDefaultMegaLabelTheme(MegaLabel label)
	{
		Font font = label.GetThemeDefaultFont();
		if (font != null)
		{
			label.AddThemeFontOverride("font", font);
		}
		int fontSize = label.GetThemeDefaultFontSize();
		if (fontSize > 0)
		{
			label.AddThemeFontSizeOverride("font_size", fontSize);
		}
	}
}
