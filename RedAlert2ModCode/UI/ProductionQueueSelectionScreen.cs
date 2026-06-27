using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class ProductionQueueSelectionScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<List<StopProductionCard.ProductionQueueItem>?> _completionSource = new();
    private readonly List<StopProductionCard.ProductionQueueItem> _items;
    private readonly int _maxSelection;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _itemsRow;
    private bool _choiceLocked;
    private List<StopProductionCard.ProductionQueueItem> _selectedItems = new();

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private ProductionQueueSelectionScreen(List<StopProductionCard.ProductionQueueItem> items, int maxSelection)
    {
        _items = items;
        _maxSelection = maxSelection;
        Name = nameof(ProductionQueueSelectionScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<List<StopProductionCard.ProductionQueueItem>?> ShowSelection(
        List<StopProductionCard.ProductionQueueItem> items, int maxSelection)
    {
        var screen = new ProductionQueueSelectionScreen(items, maxSelection);
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

        Label title = new()
        {
            Text = "请选择要启动或停止的生产序列",
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

        // 添加按钮容器
        HBoxContainer buttonContainer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        buttonContainer.AddThemeConstantOverride("separation", 20);

        Button cancelButton = new()
        {
            Text = "X 取消",
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
            Text = "确认选择",
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

    private Button CreateItemButton(StopProductionCard.ProductionQueueItem item, int index)
    {
        Button button = new()
        {
            Name = $"ItemButton_{index}",
            CustomMinimumSize = new Vector2(200f, 220f),
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

        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 4);
        contentMargin.AddChild(content);

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
            content.AddChild(texture);
        }

        Label name = new()
        {
            Text = item.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        content.AddChild(name);

        Label status = new()
        {
            Text = item.IsStopped ? "已停产" : "生产中",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = item.IsStopped ? new Color(1f, 0.8f, 0.6f) : new Color(0.6f, 1f, 0.6f)
        };
        status.AddThemeFontSizeOverride("font_size", 14);
        content.AddChild(status);

        button.Pressed += () => OnItemSelected(item, index);

        return button;
    }

    private void OnItemSelected(StopProductionCard.ProductionQueueItem item, int index)
    {
        if (_choiceLocked) return;

        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
            UpdateItemButtonStyle(index, false);
        }
        else if (_selectedItems.Count < _maxSelection)
        {
            _selectedItems.Add(item);
            UpdateItemButtonStyle(index, true);
        }
    }

    private void UpdateItemButtonStyle(int index, bool isSelected)
    {
        string buttonName = $"ItemButton_{index}";
        foreach (var child in _itemsRow.GetChildren())
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
                    var item = _items[index];
                    Color bgColor = item.IsStopped ? new Color(0.2f, 0.15f, 0.15f, 0.8f) : new Color(0.1f, 0.15f, 0.2f, 0.8f);
                    button.AddThemeStyleboxOverride("normal", CreateCardStyle(bgColor));
                    button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
                }
                break;
            }
        }
    }

    private void OnCancelClicked()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(null);
        NOverlayStack.Instance?.Remove(this);
    }

    private void OnConfirmClicked()
    {
        if (_choiceLocked) return;

        if (_selectedItems.Count > 0)
        {
            _choiceLocked = true;
            _completionSource.TrySetResult(new List<StopProductionCard.ProductionQueueItem>(_selectedItems));
            NOverlayStack.Instance?.Remove(this);
        }
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