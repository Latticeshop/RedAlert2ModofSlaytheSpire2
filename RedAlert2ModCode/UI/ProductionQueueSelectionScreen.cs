using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common.Powers;

namespace RedAlert2ModCode.UI;

public enum ProductionQueueAction
{
    None,
    ToggleStop,    // 停产/恢复
    CancelQueue    // 取消队列
}

public class ProductionQueueItem
{
    public PowerModel Power { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public bool IsStopped { get; set; }
    public string Type { get; set; } = string.Empty;
    public int TotalStacks { get; set; } = 0;
    public int SelectedCount { get; set; } = 0;
}

public class ProductionQueueSelectionResult
{
    public List<ProductionQueueItem> Items { get; set; } = new();
    public ProductionQueueAction Action { get; set; } = ProductionQueueAction.None;
}

public sealed partial class ProductionQueueSelectionScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<ProductionQueueSelectionResult?> _completionSource = new();
    private readonly List<ProductionQueueItem> _items;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _itemsRow;
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

    private ProductionQueueSelectionScreen(List<ProductionQueueItem> items)
    {
        _items = items;
        Name = nameof(ProductionQueueSelectionScreen);
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

    public static async Task<ProductionQueueSelectionResult?> ShowSelection(
        List<ProductionQueueItem> items, Player player)
    {
        var screen = new ProductionQueueSelectionScreen(items);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public static async Task<ProductionQueueSelectionResult?> ShowSelectionWithSync(
        PlayerChoiceContext context, List<ProductionQueueItem> items, Player player)
    {
        List<ProductionQueueItem> itemsCopy = new(items);

        // 将选择结果编码为 [action, index1, count1, index2, count2, ...] 的整数列表
        // action: 0=ToggleStop, 1=CancelQueue
        List<int> encodedSelection = await MultiplayerSyncHelper.ExecuteSyncMultiChoice(context, player, async () =>
        {
            ProductionQueueSelectionResult? result = await ShowSelection(itemsCopy, player);
            if (result == null) return null;

            List<int> encoded = new();
            encoded.Add((int)result.Action);
            
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

        if (encodedSelection != null && encodedSelection.Count >= 1)
        {
            var result = new ProductionQueueSelectionResult
            {
                Action = (ProductionQueueAction)encodedSelection[0]
            };
            
            for (int i = 1; i < encodedSelection.Count; i += 2)
            {
                int index = encodedSelection[i];
                int count = encodedSelection[i + 1];
                if (index >= 0 && index < itemsCopy.Count && count > 0)
                {
                    var item = itemsCopy[index];
                    result.Items.Add(new ProductionQueueItem
                    {
                        Power = item.Power,
                        Name = item.Name,
                        IconPath = item.IconPath,
                        IsStopped = item.IsStopped,
                        Type = item.Type,
                        TotalStacks = item.TotalStacks,
                        SelectedCount = count
                    });
                }
            }
            return result;
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
            CustomMinimumSize = new Vector2(1200f, 500f)
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

        Label title = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.title")),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        _scrollContainer = new ScrollContainer()
        {
            Name = "ItemScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(1100f, 320f),
            MouseFilter = MouseFilterEnum.Pass,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        root.AddChild(_scrollContainer);

        _itemsRow = new HBoxContainer()
        {
            Name = "ItemsRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _itemsRow.AddThemeConstantOverride("separation", 15);
        _scrollContainer.AddChild(_itemsRow);

        foreach (var item in _items.Select((i, idx) => (Item: i, Index: idx)))
        {
            _itemsRow.AddChild(CreateItemButton(item.Item, item.Index));
        }

        ScrollDragHelper.EnableDragScroll(_scrollContainer);

        // 添加按钮容器（取消、停止/恢复、取消队列）
        HBoxContainer buttonContainer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        buttonContainer.AddThemeConstantOverride("separation", 20);

        Button toggleButton = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.toggle")),
            CustomMinimumSize = new Vector2(160f, 50f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        toggleButton.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.3f, 0.15f)));
        toggleButton.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.4f, 0.2f)));
        toggleButton.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.25f, 0.12f)));
        toggleButton.AddThemeColorOverride("font_color", new Color(0.9f, 1f, 0.9f));
        toggleButton.AddThemeFontSizeOverride("font_size", 20);
        toggleButton.Pressed += () => OnActionClicked(ProductionQueueAction.ToggleStop);
        buttonContainer.AddChild(toggleButton);

        Button cancelQueueButton = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.cancel_queue")),
            CustomMinimumSize = new Vector2(160f, 50f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        cancelQueueButton.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.4f, 0.1f, 0.1f)));
        cancelQueueButton.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.5f, 0.15f, 0.15f)));
        cancelQueueButton.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.3f, 0.08f, 0.08f)));
        cancelQueueButton.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.8f));
        cancelQueueButton.AddThemeFontSizeOverride("font_size", 20);
        cancelQueueButton.Pressed += () => OnActionClicked(ProductionQueueAction.CancelQueue);
        buttonContainer.AddChild(cancelQueueButton);

        root.AddChild(buttonContainer);
    }

    private Button CreateItemButton(ProductionQueueItem item, int index)
    {
        Button button = new()
        {
            Name = $"ItemButton_{index}",
            CustomMinimumSize = new Vector2(200f, 240f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        Color bgColor = item.IsStopped ? new Color(0.2f, 0.15f, 0.15f, 0.8f) : new Color(0.1f, 0.15f, 0.2f, 0.8f);
        button.AddThemeStyleboxOverride("normal", CreateCardStyle(bgColor));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));

        MarginContainer contentMargin = new();
        contentMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddThemeConstantOverride("margin_left", 10);
        contentMargin.AddThemeConstantOverride("margin_right", 10);
        contentMargin.AddThemeConstantOverride("margin_top", 10);
        contentMargin.AddThemeConstantOverride("margin_bottom", 10);
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

        // 卡牌内容区域（图片、名称、状态、总层数）
        VBoxContainer cardContent = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        cardContent.AddThemeConstantOverride("separation", 4);
        content.AddChild(cardContent);

        if (!string.IsNullOrEmpty(item.IconPath) && ResourceLoader.Exists(item.IconPath))
        {
            TextureRect texture = new()
            {
                Texture = ResourceLoader.Load<Texture2D>(item.IconPath),
                CustomMinimumSize = new Vector2(100f, 100f),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            cardContent.AddChild(texture);
        }

        Label name = new()
        {
            Text = item.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        cardContent.AddChild(name);

        Label status = new()
        {
            Text = item.IsStopped ? GetLocStringText(new LocString("card_keywords", "ui.production_queue.stopped")) : GetLocStringText(new LocString("card_keywords", "ui.production_queue.running")),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = item.IsStopped ? new Color(1f, 0.8f, 0.6f) : new Color(0.6f, 1f, 0.6f)
        };
        status.AddThemeFontSizeOverride("font_size", 14);
        cardContent.AddChild(status);

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
            int maxCount = Math.Min(99, item.TotalStacks);
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
                int maxCount = Math.Min(99, item.TotalStacks);
                
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
                int maxCount = Math.Min(99, item.TotalStacks);
                
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
            var item = _items[index];
            
            if (isSelected)
            {
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.15f, 0.35f, 0.15f)));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.2f, 0.45f, 0.2f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.12f, 0.3f, 0.12f)));
            }
            else
            {
                Color bgColor = item.IsStopped ? new Color(0.2f, 0.15f, 0.15f, 0.8f) : new Color(0.1f, 0.15f, 0.2f, 0.8f);
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(bgColor));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));
            }
        }
    }

    private void OnCancelClicked()
    {
        Close();
    }

    private void OnActionClicked(ProductionQueueAction action)
    {
        if (_choiceLocked) return;

        // 收集所有数量>0的选择
        List<ProductionQueueItem> selectedItems = new();
        foreach (int index in _selectionOrder)
        {
            if (_selectedCounts.TryGetValue(index, out int count) && count > 0 && index >= 0 && index < _items.Count)
            {
                var item = _items[index];
                selectedItems.Add(new ProductionQueueItem
                {
                    Power = item.Power,
                    Name = item.Name,
                    IconPath = item.IconPath,
                    IsStopped = item.IsStopped,
                    Type = item.Type,
                    TotalStacks = item.TotalStacks,
                    SelectedCount = count
                });
            }
        }

        _choiceLocked = true;
        // 空选时返回空结果（直接打出卡牌），而非不设置结果导致任务挂起
        _completionSource.TrySetResult(new ProductionQueueSelectionResult
        {
            Items = selectedItems,
            Action = action
        });
        NOverlayStack.Instance?.Remove(this);
    }

    public void Close()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(null);
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
        style.BorderColor = FactionHelper.GetFactionBorderColor();
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
        style.BorderColor = FactionHelper.GetFactionBorderColor();
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
        style.BorderColor = FactionHelper.GetFactionBorderColor();
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
