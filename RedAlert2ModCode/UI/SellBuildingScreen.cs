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

public class SellBuildingItem
{
    public PowerModel Power { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public int TotalStacks { get; set; } = 0;
    public int SellValue { get; set; } = 0;
    public int SelectedCount { get; set; } = 0;
}

public class SellBuildingResult
{
    public List<SellBuildingItem> Items { get; set; } = new();
}

public sealed partial class SellBuildingScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<SellBuildingResult?> _completionSource = new();
    private readonly List<SellBuildingItem> _items;
    private readonly int _maxSelection;
    private readonly FactionType _faction;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _cardsRow;
    private bool _choiceLocked;
    private Dictionary<int, int> _selectedCounts = new(); // index -> count
    private List<int> _selectionOrder = new(); // 存储选择顺序
    private Dictionary<int, LineEdit> _quantityInputs = new(); // index -> LineEdit
    private Dictionary<int, Button> _itemButtons = new(); // index -> Button

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

    private SellBuildingScreen(List<SellBuildingItem> items, int maxSelect, FactionType faction)
    {
        _items = items;
        _maxSelection = maxSelect;
        _faction = faction;
        Name = nameof(SellBuildingScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        
        // 初始化每个项的数量为1
        for (int i = 0; i < _items.Count; i++)
        {
            _selectedCounts[i] = 1;
        }
        
        BuildUi();
    }

    public static async Task<SellBuildingResult?> ShowSelection(List<SellBuildingItem> items, int maxSelect, Player player, FactionType faction)
    {
        var screen = new SellBuildingScreen(items, maxSelect, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public static async Task<SellBuildingResult?> ShowSelectionWithSync(List<SellBuildingItem> items, int maxSelect, Player player, FactionType faction)
    {
        List<SellBuildingItem> itemsCopy = new(items);

        // 将选择结果编码为整数列表
        // 使用特殊标记区分取消(-2)和空选确认(-1)
        List<int> encodedSelection = await MultiplayerSyncHelper.ExecuteSyncMultiChoice(player, async () =>
        {
            SellBuildingResult? result = await ShowSelection(itemsCopy, maxSelect, player, faction);
            if (result == null)
            {
                // 取消操作：返回包含-2的列表
                return new List<int> { -2 };
            }

            if (result.Items.Count == 0)
            {
                // 空选确认：返回包含-1的列表
                return new List<int> { -1 };
            }

            // 正常选择：编码为 [index1, count1, index2, count2, ...]
            List<int> encoded = new();
            
            foreach (var item in result.Items)
            {
                int index = itemsCopy.FindIndex(i => i.Power == item.Power && i.Name == item.Name);
                if (index >= 0)
                {
                    encoded.Add(index);
                    encoded.Add(item.SelectedCount);
                }
            }
            return encoded;
        });

        if (encodedSelection != null && encodedSelection.Count > 0)
        {
            // 检查特殊标记
            if (encodedSelection[0] == -2)
            {
                // 取消操作：返回null
                return null;
            }
            
            if (encodedSelection[0] == -1)
            {
                // 空选确认：返回空结果（直接打出卡牌）
                return new SellBuildingResult();
            }
            
            // 正常选择：解码结果
            if (encodedSelection.Count >= 2)
            {
                var result = new SellBuildingResult();
                
                for (int i = 0; i < encodedSelection.Count; i += 2)
                {
                    int index = encodedSelection[i];
                    int count = encodedSelection[i + 1];
                    if (index >= 0 && index < itemsCopy.Count && count > 0)
                    {
                        var item = itemsCopy[index];
                        result.Items.Add(new SellBuildingItem
                        {
                            Power = item.Power,
                            Name = item.Name,
                            IconPath = item.IconPath,
                            TotalStacks = item.TotalStacks,
                            SellValue = item.SellValue,
                            SelectedCount = count
                        });
                    }
                }
                return result;
            }
        }

        return null;
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

        foreach (var item in _items.Select((i, idx) => (Item: i, Index: idx)))
        {
            _cardsRow.AddChild(CreateItemButton(item.Item, item.Index));
        }

        ScrollDragHelper.EnableDragScroll(_scrollContainer);

        HBoxContainer buttonContainer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        buttonContainer.AddThemeConstantOverride("separation", 20);
        
        // 添加取消按钮（在左边）
        Button cancelButton = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.sell_building.cancel")),
            CustomMinimumSize = new Vector2(160f, 50f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        cancelButton.AddThemeStyleboxOverride("normal", CreateCancelStyle(new Color(0.45f, 0.1f, 0.1f, 0.85f)));
        cancelButton.AddThemeStyleboxOverride("hover", CreateCancelStyle(new Color(0.55f, 0.15f, 0.15f, 0.9f)));
        cancelButton.AddThemeStyleboxOverride("pressed", CreateCancelStyle(new Color(0.35f, 0.08f, 0.08f, 0.95f)));
        cancelButton.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.9f));
        cancelButton.AddThemeFontSizeOverride("font_size", 20);
        cancelButton.Pressed += OnCancelClicked;
        buttonContainer.AddChild(cancelButton);

        // 添加确认按钮（在右边）
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

    private Button CreateItemButton(SellBuildingItem item, int index)
    {
        Button button = new()
        {
            Name = $"ItemButton_{index}",
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

        // 使用VBoxContainer填充整个按钮，数量控件固定在底部
        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 4);
        contentMargin.AddChild(content);

        // 卡牌内容区域（图片、名称、出售价值、总层数）
        VBoxContainer cardContent = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        cardContent.AddThemeConstantOverride("separation", 4);
        content.AddChild(cardContent);

        string iconPath = item.IconPath;
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
            cardContent.AddChild(texture);
        }

        string titleText = GetLocStringText(item.Power.Title);
        if (string.IsNullOrEmpty(titleText))
        {
            titleText = item.Name;
        }
        Label name = new()
        {
            Text = titleText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        cardContent.AddChild(name);

        string sellValueText = $"{GetLocStringText(new LocString("card_keywords", "ui.sell_building.sell_value"))}: ${item.SellValue}";
        Label sellValueLabel = new()
        {
            Text = sellValueText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = new Color(0.8f, 0.9f, 0.6f)
        };
        sellValueLabel.AddThemeFontSizeOverride("font_size", 14);
        cardContent.AddChild(sellValueLabel);

        // 显示总层数
        Label totalStacks = new()
        {
            Text = $"总层数: {item.TotalStacks}",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = new Color(0.7f, 0.7f, 0.8f)
        };
        totalStacks.AddThemeFontSizeOverride("font_size", 14);
        cardContent.AddChild(totalStacks);

        // 添加数量选择控件（固定在底部）
        HBoxContainer quantityRow = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        quantityRow.AddThemeConstantOverride("separation", 8);

        Button minusBtn = new()
        {
            Text = "-",
            CustomMinimumSize = new Vector2(36f, 36f),
            FocusMode = FocusModeEnum.All
        };
        minusBtn.AddThemeFontSizeOverride("font_size", 22);
        minusBtn.Pressed += () => AdjustQuantity(index, -1);
        quantityRow.AddChild(minusBtn);

        // 使用LineEdit支持直接输入
        LineEdit quantityInput = new()
        {
            Text = "1",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(40f, 36f),
            FocusMode = FocusModeEnum.All
        };
        quantityInput.AddThemeConstantOverride("align", (int)HorizontalAlignment.Center);
        quantityInput.Name = $"QuantityInput_{index}";
        quantityInput.AddThemeFontSizeOverride("font_size", 20);
        quantityInput.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
        quantityInput.FocusExited += () => OnQuantityInputFocusExited(index);
        quantityInput.TextChanged += (text) => OnQuantityInputTextChanged(index, text);
        quantityRow.AddChild(quantityInput);
        
        // 保存LineEdit和Button引用
        _quantityInputs[index] = quantityInput;
        _itemButtons[index] = button;

        Button plusBtn = new()
        {
            Text = "+",
            CustomMinimumSize = new Vector2(36f, 36f),
            FocusMode = FocusModeEnum.All
        };
        plusBtn.AddThemeFontSizeOverride("font_size", 22);
        plusBtn.Pressed += () => AdjustQuantity(index, 1);
        quantityRow.AddChild(plusBtn);

        content.AddChild(quantityRow);

        button.Pressed += () => ToggleCardSelection(index);

        return button;
    }

    private void ToggleCardSelection(int index)
    {
        if (_choiceLocked) return;

        bool isSelected = _selectionOrder.Contains(index);
        
        if (isSelected)
        {
            // 已选中，点击后取消选中
            _selectionOrder.Remove(index);
            UpdateButtonSelectionStyle(index, false);
        }
        else
        {
            // 未选中，点击后选中
            _selectionOrder.Add(index);
            // 确保数量至少为1
            if (_selectedCounts.TryGetValue(index, out int count) && count < 1)
            {
                _selectedCounts[index] = 1;
            }
            UpdateQuantityDisplay(index);
            UpdateButtonSelectionStyle(index, true);
        }
    }

    private void AdjustQuantity(int index, int delta)
    {
        if (_choiceLocked) return;

        if (_selectedCounts.TryGetValue(index, out int currentCount))
        {
            var item = _items[index];
            int maxCount = Math.Min(_maxSelection, item.TotalStacks);
            int newCount = Math.Max(1, Math.Min(maxCount, currentCount + delta));
            _selectedCounts[index] = newCount;

            UpdateQuantityDisplay(index);
        }
    }

    private void OnQuantityInputFocusExited(int index)
    {
        if (_choiceLocked) return;

        if (_quantityInputs.TryGetValue(index, out LineEdit input))
        {
            if (int.TryParse(input.Text, out int value))
            {
                var item = _items[index];
                int maxCount = Math.Min(_maxSelection, item.TotalStacks);
                
                if (value <= 0)
                {
                    _selectedCounts[index] = 1; // 数量至少为1
                }
                else
                {
                    value = Math.Min(maxCount, value);
                    _selectedCounts[index] = value;
                }
                UpdateQuantityDisplay(index);
            }
            else
            {
                UpdateQuantityDisplay(index);
            }
        }
    }

    private void OnQuantityInputTextChanged(int index, string text)
    {
        if (_choiceLocked) return;

        if (_quantityInputs.TryGetValue(index, out LineEdit input))
        {
            // 过滤非数字字符
            string filtered = new string(text.Where(c => char.IsDigit(c)).ToArray());
            if (filtered != text)
            {
                input.Text = filtered;
                return;
            }

            if (int.TryParse(filtered, out int value))
            {
                var item = _items[index];
                int maxCount = Math.Min(_maxSelection, item.TotalStacks);
                
                if (value > maxCount)
                {
                    input.Text = maxCount.ToString();
                    _selectedCounts[index] = maxCount;
                }
                else if (value > 0)
                {
                    _selectedCounts[index] = value;
                }
            }
        }
    }

    private void UpdateQuantityDisplay(int index)
    {
        if (_selectedCounts.TryGetValue(index, out int count) && _quantityInputs.TryGetValue(index, out LineEdit input))
        {
            input.Text = count.ToString();
        }
    }

    private void UpdateButtonSelectionStyle(int index, bool isSelected)
    {
        if (_itemButtons.TryGetValue(index, out Button button))
        {
            if (isSelected)
            {
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.15f, 0.35f, 0.15f)));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.2f, 0.45f, 0.2f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.12f, 0.3f, 0.12f)));
            }
            else
            {
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.2f, 0.8f)));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));
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

        // 收集所有数量>0的选择
        List<SellBuildingItem> selectedItems = new();
        foreach (int index in _selectionOrder)
        {
            if (_selectedCounts.TryGetValue(index, out int count) && count > 0 && index >= 0 && index < _items.Count)
            {
                var item = _items[index];
                selectedItems.Add(new SellBuildingItem
                {
                    Power = item.Power,
                    Name = item.Name,
                    IconPath = item.IconPath,
                    TotalStacks = item.TotalStacks,
                    SellValue = item.SellValue,
                    SelectedCount = count
                });
            }
        }

        _choiceLocked = true;
        // 空选时返回空结果（直接打出卡牌），而非调用Close()返回null
        _completionSource.TrySetResult(new SellBuildingResult
        {
            Items = selectedItems
        });
        NOverlayStack.Instance?.Remove(this);
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