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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.UI;

public sealed partial class SellBuildingScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<List<int>?> _completionSource = new();
    private readonly List<(PowerModel Power, int Index)> _buildingPowerItems;
    private readonly int _maxSelection;
    private readonly int _minSelection = 0;
    private readonly FactionType _faction;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _cardsRow;
    private bool _choiceLocked;
    private List<int> _selectedIndices = new();

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private string GetLocStringText(object? locStringObj)
    {
        if (locStringObj == null) return string.Empty;
        if (locStringObj is string str) return str;

        Type locStringType = locStringObj.GetType();
        
        System.Reflection.MethodInfo? formattedMethod = locStringType.GetMethod("GetFormattedText", new Type[0]);
        if (formattedMethod != null)
        {
            try
            {
                object? result = formattedMethod.Invoke(locStringObj, null);
                if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
                {
                    return formattedText;
                }
            }
            catch { }
        }

        System.Reflection.MethodInfo? rawMethod = locStringType.GetMethod("GetRawText");
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

    private SellBuildingScreen(List<(PowerModel Power, int Index)> buildingPowerItems, int maxSelect, FactionType faction)
    {
        _buildingPowerItems = buildingPowerItems;
        _maxSelection = maxSelect;
        _faction = faction;
        Name = nameof(SellBuildingScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<List<int>?> ShowSelection(List<(PowerModel Power, int Index)> buildingPowerItems, int maxSelect, Player player, FactionType faction)
    {
        var screen = new SellBuildingScreen(buildingPowerItems, maxSelect, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public static async Task<List<int>> ShowSelectionWithSync(List<(PowerModel Power, int Index)> buildingPowerItems, int maxSelect, Player player, FactionType faction)
    {
        List<(PowerModel Power, int Index)> itemsCopy = new(buildingPowerItems);

        return await MultiplayerSyncHelper.ExecuteSyncMultiChoice(player, async () =>
        {
            List<int>? selected = await ShowSelection(itemsCopy, maxSelect, player, faction);
            return selected;
        });
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
            CustomMinimumSize = new Vector2(1200f, 450f)
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

        var titleLocString = new LocString("card_keywords", "ui.sell_building.title");
        titleLocString.Add("count", _maxSelection);
        
        Label title = new()
        {
            Text = GetLocStringText(titleLocString),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        _scrollContainer = new ScrollContainer()
        {
            Name = "CardScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(1100f, 320f),
            MouseFilter = MouseFilterEnum.Pass,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        root.AddChild(_scrollContainer);

        _cardsRow = new HBoxContainer()
        {
            Name = "CardsRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _cardsRow.AddThemeConstantOverride("separation", 15);
        _scrollContainer.AddChild(_cardsRow);

        foreach (var item in _buildingPowerItems)
        {
            _cardsRow.AddChild(CreatePowerButton(item.Power, item.Index));
        }

        HBoxContainer buttonContainer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        buttonContainer.AddThemeConstantOverride("separation", 20);
        
        Button cancelButton = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.sell_building.cancel")),
            CustomMinimumSize = new Vector2(160f, 50f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        cancelButton.AddThemeStyleboxOverride("normal", CreateCancelStyle());
        cancelButton.AddThemeStyleboxOverride("hover", CreateCancelStyle(new Color(0.6f, 0.15f, 0.15f, 0.9f)));
        cancelButton.AddThemeStyleboxOverride("pressed", CreateCancelStyle(new Color(0.35f, 0.08f, 0.08f, 0.95f)));
        cancelButton.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.85f));
        cancelButton.AddThemeFontSizeOverride("font_size", 20);
        cancelButton.Pressed += OnCancelClicked;
        buttonContainer.AddChild(cancelButton);

        Button confirmButton = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.sell_building.confirm")),
            CustomMinimumSize = new Vector2(160f, 50f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        confirmButton.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.3f, 0.15f)));
        confirmButton.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.4f, 0.2f)));
        confirmButton.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.25f, 0.12f)));
        confirmButton.AddThemeColorOverride("font_color", new Color(0.9f, 1f, 0.9f));
        confirmButton.AddThemeFontSizeOverride("font_size", 20);
        confirmButton.Pressed += OnConfirmClicked;
        buttonContainer.AddChild(confirmButton);
        
        root.AddChild(buttonContainer);
    }

    private Button CreatePowerButton(PowerModel power, int index)
    {
        Button button = new()
        {
            Name = $"PowerButton_{power.Id.Entry}_{index}",
            CustomMinimumSize = new Vector2(260f, 280f),
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

        string iconPath = GetPowerIconPath(power);
        if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
        {
            TextureRect texture = new()
            {
                Texture = ResourceLoader.Load<Texture2D>(iconPath),
                CustomMinimumSize = new Vector2(140f, 140f),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            content.AddChild(texture);
        }

        string titleText = GetLocStringText(power.Title);
        if (string.IsNullOrEmpty(titleText))
        {
            titleText = power.Id.Entry.Replace("_", " ");
        }
        Label name = new()
        {
            Text = titleText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        content.AddChild(name);

        int dollarValue = GetPowerBuildCost(power);
        int sellValue = dollarValue / 2;
        string sellValueText = $"{GetLocStringText(new LocString("card_keywords", "ui.sell_building.sell_value"))}: ${sellValue}";
        Label sellValueLabel = new()
        {
            Text = sellValueText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = new Color(0.8f, 0.9f, 0.6f)
        };
        sellValueLabel.AddThemeFontSizeOverride("font_size", 14);
        content.AddChild(sellValueLabel);

        button.Pressed += () => OnPowerSelected(index);

        return button;
    }

    private string GetPowerIconPath(PowerModel power)
    {
        Type powerType = power.GetType();
        
        if (powerType == typeof(AlliedRefineryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/reficon.png";
        if (powerType == typeof(SovietRefineryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nreficon.png";
        if (powerType == typeof(AlliedWarFactoryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/gwepicon.png";
        if (powerType == typeof(SovietWarFactoryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwepicon.png";
        if (powerType == typeof(BattleLabPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/techicon.png";
        if (powerType == typeof(SovietBattleLabPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/ntchicon.png";
        if (powerType == typeof(SovietRadarPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png";
        if (powerType == typeof(AlliedMCVPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";
        if (powerType == typeof(SovietMCVPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";

        string iconPath = power.PackedIconPath;
        if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
            return iconPath;

        return string.Empty;
    }

    private int GetPowerBuildCost(PowerModel power)
    {
        return RedAlert2ModCode.Common.Cards.CommonCardValues.GetSellablePowerDollarValue(power.GetType());
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

    private StyleBoxFlat CreateCancelStyle(Color? bgColor = null)
    {
        StyleBoxFlat style = new();
        style.BgColor = bgColor ?? new Color(0.45f, 0.1f, 0.1f, 0.85f);
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

    private void OnPowerSelected(int index)
    {
        if (_choiceLocked) return;

        if (_selectedIndices.Contains(index))
        {
            _selectedIndices.Remove(index);
            UpdatePowerButtonStyle(index, false);
        }
        else if (_selectedIndices.Count < _maxSelection)
        {
            _selectedIndices.Add(index);
            UpdatePowerButtonStyle(index, true);
        }
    }

    private void UpdatePowerButtonStyle(int index, bool isSelected)
    {
        var item = _buildingPowerItems[index];
        string buttonName = $"PowerButton_{item.Power.Id.Entry}_{index}";
        foreach (var child in _cardsRow.GetChildren())
        {
            if (child is Button button && button.Name == buttonName)
            {
                if (isSelected)
                {
                    button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.15f, 0.35f, 0.15f)));
                    button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.2f, 0.45f, 0.2f)));
                }
                else
                {
                    button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.2f, 0.8f)));
                    button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
                }
                break;
            }
        }
    }

    private void OnCancelClicked()
    {
        Close();
    }

    public void Close()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        
        _completionSource.TrySetResult(null);
        NOverlayStack.Instance?.Remove(this);
    }

    private void OnConfirmClicked()
    {
        if (_choiceLocked) return;
        
        _choiceLocked = true;
        _completionSource.TrySetResult(new List<int>(_selectedIndices));
        NOverlayStack.Instance?.Remove(this);
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