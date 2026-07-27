using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
// 移除 MegaLabel 引用，使用普通 Label 避免 Godot 字体覆盖 bug
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.UI;

/// <summary>
/// 卡牌选择结果，包含卡牌和数量
/// </summary>
public class CardSelectionResult
{
    public CardModel Card { get; set; }
    public int Count { get; set; }
}

public sealed partial class CardSelectionScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<CardModel?> _completionSource = new();
    private readonly TaskCompletionSource<List<CardModel>?> _multiCompletionSource = new();
    private readonly TaskCompletionSource<List<CardSelectionResult>?> _quantityCompletionSource = new();
    private readonly List<CardModel> _cards;
    private readonly Dictionary<string, CardValueStore.CardValues> _cardValuesMap;
    private readonly FactionType _faction;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _cardsRow;
    private Button _cancelButton;
    private bool _choiceLocked;
    private bool _isMultiSelect = false;
    private bool _isQuantitySelect = false;
    private int _maxSelection = 1;
    private int _minSelection = 1;
    private List<CardModel> _selectedCards = new();
    private Dictionary<int, int> _cardQuantities = new(); // 存储每个卡牌的数量 (index -> count)
    private Dictionary<int, LineEdit> _quantityInputs = new(); // 存储每个卡牌的LineEdit引用 (index -> LineEdit)
    private Dictionary<int, Button> _cardButtons = new(); // 存储每个卡牌的Button引用 (index -> Button)
    private List<int> _selectionOrder = new(); // 存储选择顺序（按点击顺序排列的卡牌索引）

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
            catch (Exception ex)
            {
                GD.PrintErr($"[CardSelectionScreen] GetFormattedText 失败: {ex.Message}");
            }
        }

        return GetLocStringRawText(locStringObj);
    }

    private static string GetLocStringRawText(object? locStringObj)
    {
        if (locStringObj == null) return string.Empty;
        if (locStringObj is string str) return str;

        Type locStringType = locStringObj.GetType();

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

    private CardSelectionScreen(List<CardModel> cards, Dictionary<string, CardValueStore.CardValues> cardValuesMap = null, FactionType faction = FactionType.Allied)
    {
        _cards = cards;
        _cardValuesMap = cardValuesMap ?? new Dictionary<string, CardValueStore.CardValues>();
        _faction = faction;
        Name = nameof(CardSelectionScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    private CardSelectionScreen(List<CardModel> cards, int maxSelect, int minSelect, Dictionary<string, CardValueStore.CardValues> cardValuesMap = null, FactionType faction = FactionType.Allied)
        {
            _cards = cards;
            _cardValuesMap = cardValuesMap ?? new Dictionary<string, CardValueStore.CardValues>();
            _isMultiSelect = true;
            _maxSelection = maxSelect;
            _minSelection = minSelect;
            _faction = faction;
            Name = nameof(CardSelectionScreen);
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.All;
            BuildUi();
        }

        private CardSelectionScreen(List<CardModel> cards, Dictionary<string, CardValueStore.CardValues> cardValuesMap, FactionType faction, bool isQuantitySelect)
        {
            _cards = cards;
            _cardValuesMap = cardValuesMap ?? new Dictionary<string, CardValueStore.CardValues>();
            _isQuantitySelect = isQuantitySelect;
            _faction = faction;
            Name = nameof(CardSelectionScreen);
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.All;
            // 初始化每个卡牌的数量为1（默认为1，但未选中）
            for (int i = 0; i < _cards.Count; i++)
            {
                _cardQuantities[i] = 1;
                // 选择顺序列表默认为空，由玩家主动选择
            }
            BuildUi();
        }

    public static async Task<CardModel?> ShowSelection(List<CardModel> cards, Player player, FactionType faction = FactionType.Allied)
    {
        var screen = new CardSelectionScreen(cards, null, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public static async Task<CardModel?> ShowSelection(List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues> cardValuesMap, FactionType faction = FactionType.Allied)
    {
        var screen = new CardSelectionScreen(cards, cardValuesMap, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._completionSource.Task;
    }

    public static async Task<List<CardModel>?> ShowMultiSelection(List<CardModel> cards, int maxSelect, int minSelect, Player player, FactionType faction = FactionType.Allied)
    {
        var screen = new CardSelectionScreen(cards, maxSelect, minSelect, null, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._multiCompletionSource.Task;
    }

    public static async Task<List<CardModel>?> ShowMultiSelection(List<CardModel> cards, int maxSelect, int minSelect, Player player, Dictionary<string, CardValueStore.CardValues> cardValuesMap, FactionType faction = FactionType.Allied)
    {
        var screen = new CardSelectionScreen(cards, maxSelect, minSelect, cardValuesMap, faction);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._multiCompletionSource.Task;
    }

    public static async Task<List<CardSelectionResult>?> ShowSelectionWithQuantity(List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues> cardValuesMap, FactionType faction = FactionType.Allied)
    {
        var screen = new CardSelectionScreen(cards, cardValuesMap, faction, true);
        NOverlayStack.Instance?.Push(screen);
        
        if (!MultiplayerSyncHelper.IsLocalPlayer(player))
        {
            screen.Close();
            return null;
        }
        
        return await screen._quantityCompletionSource.Task;
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
            CustomMinimumSize = new Vector2(1200f, 520f) // 保持宽度，增加高度以显示完整文案和数量控件
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

        // 使用普通 Label 替代 MegaLabel，避免 Godot 字体覆盖 bug
        LocString titleLocString;
        if (_isMultiSelect)
        {
            titleLocString = new LocString("card_keywords", "ui.card_select.title_multi");
            titleLocString.Add("count", _maxSelection);
        }
        else
        {
            titleLocString = new LocString("card_keywords", "ui.card_select.title_single");
        }
        
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
            CustomMinimumSize = new Vector2(1190f, 380f), // 1200-30×2=1140，与内部可用宽度一致，保证左右间隙对称
            MouseFilter = MouseFilterEnum.Stop,
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

        foreach (var card in _cards.Select((c, i) => (Card: c, Index: i)))
        {
            _cardsRow.AddChild(CreateCardButton(card.Card, card.Index));
        }

        // 数量选择模式或多选模式下添加按钮容器（并排展示）
        if (_isQuantitySelect || _isMultiSelect)
        {
            HBoxContainer buttonContainer = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };
            buttonContainer.AddThemeConstantOverride("separation", 20);
            
            _cancelButton = new Button()
            {
                Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.cancel")),
                CustomMinimumSize = new Vector2(160f, 50f),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                FocusMode = FocusModeEnum.All,
                MouseDefaultCursorShape = CursorShape.PointingHand,
                Disabled = true
            };
            _cancelButton.AddThemeStyleboxOverride("normal", CreateCancelStyle());
            _cancelButton.AddThemeStyleboxOverride("hover", CreateCancelStyle(new Color(0.6f, 0.15f, 0.15f, 0.9f)));
            _cancelButton.AddThemeStyleboxOverride("pressed", CreateCancelStyle(new Color(0.35f, 0.08f, 0.08f, 0.95f)));
            _cancelButton.AddThemeStyleboxOverride("disabled", CreateCancelStyle(new Color(0.3f, 0.3f, 0.3f, 0.6f)));
            _cancelButton.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.85f));
            _cancelButton.AddThemeColorOverride("font_disabled_color", new Color(0.5f, 0.5f, 0.5f));
            _cancelButton.AddThemeFontSizeOverride("font_size", 20);
            _cancelButton.Pressed += OnCancelClicked;
            buttonContainer.AddChild(_cancelButton);

            Button confirmButton = new()
            {
                Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.confirm")),
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
        else
        {
            // 单选模式：只有取消按钮
            _cancelButton = new Button()
            {
                Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.cancel")),
                CustomMinimumSize = new Vector2(160f, 50f),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                FocusMode = FocusModeEnum.All,
                MouseDefaultCursorShape = CursorShape.PointingHand,
                Disabled = true
            };
            _cancelButton.AddThemeStyleboxOverride("normal", CreateCancelStyle());
            _cancelButton.AddThemeStyleboxOverride("hover", CreateCancelStyle(new Color(0.6f, 0.15f, 0.15f, 0.9f)));
            _cancelButton.AddThemeStyleboxOverride("pressed", CreateCancelStyle(new Color(0.35f, 0.08f, 0.08f, 0.95f)));
            _cancelButton.AddThemeStyleboxOverride("disabled", CreateCancelStyle(new Color(0.3f, 0.3f, 0.3f, 0.6f)));
            _cancelButton.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.85f));
            _cancelButton.AddThemeColorOverride("font_disabled_color", new Color(0.5f, 0.5f, 0.5f));
            _cancelButton.AddThemeFontSizeOverride("font_size", 20);
            _cancelButton.Pressed += OnCancelClicked;
            root.AddChild(_cancelButton);
        }

        // 900ms后启用取消按钮
        _ = EnableCancelButtonAfterDelay();
    }

    private async Task EnableCancelButtonAfterDelay()
    {
        await Task.Delay(900);
        if (_cancelButton != null && IsInstanceValid(_cancelButton))
        {
            _cancelButton.Disabled = false;
        }
    }

    private Button CreateCardButton(CardModel card, int index)
    {
        Button button = new()
        {
            Name = $"CardButton_{card.Id.Entry}_{index}",
            CustomMinimumSize = new Vector2(280f, 380f), // 增加高度以显示完整描述和数量控件
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

        // 使用Control作为容器，支持锚点布局
        Control content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore // 忽略鼠标事件，让事件穿透到父级Button
        };
        content.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddChild(content);

        // 卡牌内容区域（图片、费用、名称、描述），从顶部开始排列
        VBoxContainer cardContent = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            Alignment = BoxContainer.AlignmentMode.Begin // 从顶部开始
        };
        cardContent.AddThemeConstantOverride("separation", 4);
        cardContent.MouseFilter = MouseFilterEnum.Ignore; // 忽略鼠标事件，让事件穿透到父级Button
        // 设置锚点：顶部填充，不覆盖底部数量控件区域
        cardContent.AnchorTop = 0.0f;
        cardContent.AnchorBottom = _isQuantitySelect ? 0.88f : 1.0f;
        cardContent.AnchorLeft = 0.0f;
        cardContent.AnchorRight = 1.0f;
        cardContent.OffsetTop = 0f;
        cardContent.OffsetBottom = 0f;
        cardContent.OffsetLeft = 0f;
        cardContent.OffsetRight = 0f;
        content.AddChild(cardContent);

        if (!string.IsNullOrEmpty(card.PortraitPath) && ResourceLoader.Exists(card.PortraitPath))
        {
            TextureRect texture = new()
            {
                Texture = ResourceLoader.Load<Texture2D>(card.PortraitPath),
                CustomMinimumSize = new Vector2(140f, 140f),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
                MouseFilter = MouseFilterEnum.Ignore // 忽略鼠标事件，让事件穿透到父级Button
            };
            cardContent.AddChild(texture);
        }

        // 获取能量费用和价格
        string costLabel = GetLocStringText(new LocString("card_keywords", "ui.card_select.cost_label"));
        string priceLabel = GetLocStringText(new LocString("card_keywords", "ui.card_select.price_label"));
        string costText = $"{costLabel}：{GetEnergyCostText(card)}  |  {priceLabel}：${GetDollarValueText(card)}";

        // 使用普通 Label 替代 MegaLabel
        Label cost = new()
        {
            Text = costText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = new Color(1f, 0.9f, 0.2f),
            MouseFilter = MouseFilterEnum.Ignore // 忽略鼠标事件，让事件穿透到父级Button
        };
        cost.AddThemeFontSizeOverride("font_size", 16);
        cardContent.AddChild(cost);

        // 正确获取卡牌名称
        string titleText = GetCardTitle(card);
        Label name = new()
        {
            Text = titleText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore // 忽略鼠标事件，让事件穿透到父级Button
        };
        name.AddThemeFontSizeOverride("font_size", 18);
        name.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        cardContent.AddChild(name);

        // 正确获取卡牌描述（包含动态变量转义和IfUpgraded处理）
        string descText = GetCardDescription(card, card.IsUpgraded);
        // 字符数截断，超过65字符时省略
        if (!string.IsNullOrEmpty(descText) && descText.Length > 65)
        {
            descText = descText.Substring(0, 65) + "...";
        }

        Label descLabel = new()
        {
            Text = descText,
            HorizontalAlignment = HorizontalAlignment.Left,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MouseFilter = MouseFilterEnum.Ignore // 忽略鼠标事件，让事件穿透到父级Button
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));
        cardContent.AddChild(descLabel);

        // 数量选择模式：添加数量选择控件（使用锚点固定在底部）
        if (_isQuantitySelect)
        {
            HBoxContainer quantityRow = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
                CustomMinimumSize = new Vector2(120f, 40f),
                MouseFilter = MouseFilterEnum.Stop
            };
            quantityRow.AddThemeConstantOverride("separation", 8);

            // 设置锚点：固定在底部水平居中
            quantityRow.AnchorTop = 0.88f;
            quantityRow.AnchorBottom = 1.0f;
            quantityRow.AnchorLeft = 0.0f;
            quantityRow.AnchorRight = 1.0f;
            quantityRow.OffsetTop = 0f;
            quantityRow.OffsetBottom = 0f;
            quantityRow.OffsetLeft = 0f;
            quantityRow.OffsetRight = 0f;

            Button minusBtn = new()
            {
                Text = "-",
                CustomMinimumSize = new Vector2(36f, 36f),
                FocusMode = FocusModeEnum.All,
                MouseFilter = MouseFilterEnum.Stop
            };
            minusBtn.AddThemeFontSizeOverride("font_size", 22);
            minusBtn.Pressed += () => AdjustQuantity(index, -1);
            quantityRow.AddChild(minusBtn);

            // 使用LineEdit支持直接输入
            LineEdit quantityInput = new()
            {
                Text = "1", // 初始值为1
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                CustomMinimumSize = new Vector2(40f, 36f),
                FocusMode = FocusModeEnum.All,
                MouseFilter = MouseFilterEnum.Stop
            };
            quantityInput.AddThemeConstantOverride("align", (int)HorizontalAlignment.Center);
            quantityInput.Name = $"QuantityInput_{index}";
            quantityInput.AddThemeFontSizeOverride("font_size", 20);
            quantityInput.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
            quantityInput.FocusExited += () => OnQuantityInputFocusExited(index, quantityInput);
            quantityInput.TextChanged += (text) => OnQuantityInputTextChanged(index, text);
            quantityRow.AddChild(quantityInput);
            
            // 保存LineEdit引用，方便后续更新
            _quantityInputs[index] = quantityInput;

            Button plusBtn = new()
            {
                Text = "+",
                CustomMinimumSize = new Vector2(36f, 36f),
                FocusMode = FocusModeEnum.All,
                MouseFilter = MouseFilterEnum.Stop
            };
            plusBtn.AddThemeFontSizeOverride("font_size", 22);
            plusBtn.Pressed += () => AdjustQuantity(index, 1);
            quantityRow.AddChild(plusBtn);

            content.AddChild(quantityRow);
        }

        button.Pressed += () => OnCardSelected(card);
        
        // 保存Button引用，方便后续更新样式
        _cardButtons[index] = button;

        return button;
    }

    private void AdjustQuantity(int index, int delta)
    {
        if (_choiceLocked) return;

        if (_cardQuantities.TryGetValue(index, out int currentCount))
        {
            int newCount;
            
            if (delta < 0 && currentCount == 1)
            {
                newCount = 99;
            }
            else if (delta > 0 && currentCount == 99)
            {
                newCount = 1;
            }
            else
            {
                newCount = Math.Max(1, Math.Min(99, currentCount + delta));
            }
            
            if (newCount == currentCount) return;
            
            _cardQuantities[index] = newCount;

            UpdateQuantityDisplay(index, newCount);
        }
    }

    private void UpdateQuantityDisplay(int index, int count)
    {
        // 使用保存的引用直接更新，避免遍历查找
        if (_quantityInputs.TryGetValue(index, out LineEdit input))
        {
            input.Text = count.ToString();
        }
        
        // 更新按钮样式（根据选中状态，而非数量）
        bool isSelected = _selectionOrder.Contains(index);
        if (_cardButtons.TryGetValue(index, out Button button))
        {
            if (isSelected)
            {
                // 选中状态：绿色边框/背景
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.15f, 0.35f, 0.15f)));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.2f, 0.45f, 0.2f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.12f, 0.3f, 0.12f)));
            }
            else
            {
                // 未选中状态：默认蓝色边框/背景
                button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.15f, 0.2f, 0.8f)));
                button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.2f, 0.28f, 0.9f)));
                button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.12f, 0.18f, 0.95f)));
            }
        }
    }

    private void OnQuantityInputFocusExited(int index, LineEdit input)
    {
        if (_choiceLocked) return;

        if (int.TryParse(input.Text, out int count))
        {
            // 限制范围1-99
            int newCount = Math.Max(1, Math.Min(99, count));
            
            if (newCount != count)
            {
                input.Text = newCount.ToString();
            }
            
            _cardQuantities[index] = newCount;
        }
        else
        {
            // 无效输入，恢复为当前值
            input.Text = _cardQuantities.TryGetValue(index, out int current) ? current.ToString() : "1";
        }
    }

    private void OnQuantityInputTextChanged(int index, string text)
    {
        if (_choiceLocked) return;

        // 只允许输入数字
        if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out _))
        {
            // 过滤非数字字符
            string filtered = new string(text.Where(char.IsDigit).ToArray());
            if (filtered != text)
            {
                // 使用保存的引用直接更新
                if (_quantityInputs.TryGetValue(index, out LineEdit input))
                {
                    input.Text = filtered;
                }
            }
        }
    }

    private void UpdateQuantityFromInput(int index, int count)
    {
        if (_cardQuantities.TryGetValue(index, out int currentCount))
        {
            _cardQuantities[index] = count;

            // 维护选择顺序：输入数量>0时自动选中
            if (!_selectionOrder.Contains(index) && count > 0)
            {
                _selectionOrder.Add(index);
            }

            // 使用保存的引用更新按钮样式（根据选中状态）
            bool isSelected = _selectionOrder.Contains(index);
            if (_cardButtons.TryGetValue(index, out Button button))
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
    }

    private string GetEnergyCostText(CardModel card)
    {
        string cardKey = card.Id.Entry.ToUpper();
        
        // 1. 尝试从传递的映射中获取
        if (_cardValuesMap.TryGetValue(cardKey, out var values))
        {
            int cost = card.IsUpgraded ? values.Cost + values.CostUpgraded : values.Cost;
            return cost.ToString();
        }
        
        // 2. 尝试移除下划线后查找（因为cardValuesMap的key是不带下划线的）
        string cardKeyNoUnderscore = cardKey.Replace("_", "");
        if (_cardValuesMap.TryGetValue(cardKeyNoUnderscore, out values))
        {
            int cost = card.IsUpgraded ? values.Cost + values.CostUpgraded : values.Cost;
            return cost.ToString();
        }
        
        // 3. 尝试提取卡牌名称部分（移除前缀如 RED_ALERT2_MOD_CARD_）
        string cardName = ExtractCardName(cardKey);
        if (!string.IsNullOrEmpty(cardName) && _cardValuesMap.TryGetValue(cardName, out values))
        {
            int cost = card.IsUpgraded ? values.Cost + values.CostUpgraded : values.Cost;
            return cost.ToString();
        }
        
        // 4. 如果传递的映射中没有，尝试使用 FindCardValues 方法查找
        CardValueStore.CardValues foundValues = FindCardValues(card.Id.Entry);
        if (foundValues != null)
        {
            int cost = card.IsUpgraded ? foundValues.Cost + foundValues.CostUpgraded : foundValues.Cost;
            return cost.ToString();
        }
        
        // 5. 尝试从卡牌本身获取费用
        if (card.EnergyCost != null)
        {
            try
            {
                var resolvedCost = card.EnergyCost.GetResolved();
                return ((int)resolvedCost).ToString();
            }
            catch
            {
                try
                {
                    var canonicalCost = card.EnergyCost.Canonical;
                    return ((int)canonicalCost).ToString();
                }
                catch
                {
                    // 继续尝试其他方式
                }
            }
        }
        
        return "0";
    }
    
    private string GetDollarValueText(CardModel card)
    {
        string cardKey = card.Id.Entry.ToUpper();
        // 尝试直接查找
        if (_cardValuesMap.TryGetValue(cardKey, out var values))
        {
            if (values.BuildCost > 0)
                return values.BuildCost.ToString();
            decimal value = card.IsUpgraded ? values.DollarValue + values.DollarValueUpgraded : values.DollarValue;
            return value.ToString();
        }
        
        // 尝试移除下划线后查找（因为cardValuesMap的key是不带下划线的）
        string cardKeyNoUnderscore = cardKey.Replace("_", "");
        if (_cardValuesMap.TryGetValue(cardKeyNoUnderscore, out values))
        {
            if (values.BuildCost > 0)
                return values.BuildCost.ToString();
            decimal value = card.IsUpgraded ? values.DollarValue + values.DollarValueUpgraded : values.DollarValue;
            return value.ToString();
        }
        
        // 尝试提取卡牌名称部分（移除前缀如 RED_ALERT2_MOD_CARD_）
        string cardName = ExtractCardName(cardKey);
        if (!string.IsNullOrEmpty(cardName) && _cardValuesMap.TryGetValue(cardName, out values))
        {
            if (values.BuildCost > 0)
                return values.BuildCost.ToString();
            decimal value = card.IsUpgraded ? values.DollarValue + values.DollarValueUpgraded : values.DollarValue;
            return value.ToString();
        }
        
        CardValueStore.CardValues foundValues = FindCardValues(card.Id.Entry);
        if (foundValues != null)
        {
            if (foundValues.BuildCost > 0)
                return foundValues.BuildCost.ToString();
            decimal value = card.IsUpgraded ? foundValues.DollarValue + foundValues.DollarValueUpgraded : foundValues.DollarValue;
            return value.ToString();
        }
        
        // 使用提取的卡牌名称尝试获取价格
        if (!string.IsNullOrEmpty(cardName))
        {
            decimal result = SovietCardValues.GetDollarValue(cardName);
            if (result > 0)
            {
                return result.ToString();
            }
            
            result = AlliesCardValues.GetDollarValue(cardName);
            if (result > 0)
            {
                return result.ToString();
            }
        }
        
        decimal result2 = AlliesCardValues.GetDollarValue(card.Id.Entry);
        if (result2 > 0)
        {
            return result2.ToString();
        }
        
        result2 = SovietCardValues.GetDollarValue(card.Id.Entry);
        if (result2 > 0)
        {
            return result2.ToString();
        }
        
        if (card.DynamicVars != null)
        {
            foreach (var varItem in card.DynamicVars)
            {
                string varName = varItem.GetType().Name;
                if (varName.Contains("Dollar"))
                {
                    var valueProp = varItem.GetType().GetProperty("Value") ?? varItem.GetType().GetProperty("IntValue");
                    if (valueProp != null)
                    {
                        object? value = valueProp.GetValue(varItem);
                        if (value != null)
                        {
                            return value.ToString() ?? string.Empty;
                        }
                    }
                }
            }
        }
        
        return "0";
    }

    private string GetCardTitle(CardModel card)
    {
        string title = GetLocStringText(card.Title);
        if (!string.IsNullOrEmpty(title))
        {
            return title;
        }
        return card.Id.Entry.Replace("_", " ");
    }

    private string GetCardDescription(CardModel card, bool isUpgraded)
    {
        string desc = GetLocStringRawText(card.Description);
        if (string.IsNullOrEmpty(desc))
        {
            return string.Empty;
        }

        // 处理 {IfUpgraded:show:xxx|} 格式的条件标签
        desc = ProcessIfUpgradedTags(desc, isUpgraded);

        // 优先从数值映射中获取数值替换变量
        desc = ReplaceVarsFromStore(card, desc, isUpgraded);

        // 如果还有未替换的变量，尝试从动态变量获取
        desc = ReplaceDynamicVars(card, desc);

        // 去除 [gold] 和 [/gold] 标签
        desc = desc.Replace("[gold]", "").Replace("[/gold]", "");

        // 移除价格信息（卡牌选择UI页面不需要显示价格）
        desc = System.Text.RegularExpressions.Regex.Replace(desc, @"价格：\$\{?DollarNumber\}?。?", "");

        // 移除未替换的变量标记（避免显示 {xxx}）
        desc = System.Text.RegularExpressions.Regex.Replace(desc, @"\{[^{}]+\}", "");

        // 特殊处理：运输船卡牌不显示存储信息（UI选择界面不需要显示）
        if (card.Id.Entry.Equals("ALLIED_TRANSPORT_SHIP", System.StringComparison.OrdinalIgnoreCase) || 
            card.Id.Entry.Equals("SOVIET_TRANSPORT_SHIP", System.StringComparison.OrdinalIgnoreCase))
        {
            desc = desc.Replace("\n当前存储：{StoredCards}", "");
        }

        return desc;
    }

    private string ProcessIfUpgradedTags(string text, bool isUpgraded)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, 
            @"\{IfUpgraded:show:([^|]+)\|([^}]*)\}", 
            match => isUpgraded ? match.Groups[1].Value.Trim() : match.Groups[2].Value.Trim());
    }

    private string ReplaceVarsFromStore(CardModel card, string text, bool isUpgraded)
    {
        CardValueStore.CardValues values = FindCardValues(card.Id.Entry);
        if (values != null)
        {
            string damage = values.GetDamage(isUpgraded).ToString();
            string block = values.GetBlock(isUpgraded).ToString();
            string repeat = values.GetRepeat(isUpgraded).ToString();
            string cost = values.GetCost(isUpgraded).ToString();
            string magicNumber = values.GetMagicNumber(isUpgraded).ToString();
            string dollarValue = values.GetDollarValue(isUpgraded).ToString();
            
            text = text.Replace("{Damage}", damage).Replace("${Damage}", damage);
            text = text.Replace("{Block}", block).Replace("${Block}", block);
            text = text.Replace("{Repeat}", repeat).Replace("${Repeat}", repeat);
            text = text.Replace("{Cost}", cost).Replace("${Cost}", cost);
            text = text.Replace("{MagicNumber}", magicNumber).Replace("${MagicNumber}", magicNumber);
            text = text.Replace("{DollarValue}", dollarValue).Replace("${DollarValue}", dollarValue);
            text = text.Replace("{DollarNumber}", dollarValue).Replace("${DollarNumber}", dollarValue);
            
            text = text.Replace("{PlatingAmount}", block).Replace("${PlatingAmount}", block);
            
            int defendDamage = isUpgraded ? values.MagicNumber + values.MagicNumberUpgraded : values.MagicNumber;
            text = text.Replace("{DefendDamage}", defendDamage.ToString()).Replace("${DefendDamage}", defendDamage.ToString());
            text = text.Replace("{RepeatCount}", repeat).Replace("${RepeatCount}", repeat);
            
            int storeCount = isUpgraded ? values.MagicNumber + values.MagicNumberUpgraded : values.MagicNumber;
            text = text.Replace("{StoreCount}", storeCount.ToString()).Replace("${StoreCount}", storeCount.ToString());
        }
        
        return text;
    }
    
    private CardValueStore.CardValues FindCardValues(string cardEntry)
    {
        string cardKey = cardEntry.ToUpper();
        
        if (_cardValuesMap.TryGetValue(cardKey, out var values))
        {
            return values;
        }
        
        string withoutUnderscore = cardKey.Replace("_", "");
        if (_cardValuesMap.TryGetValue(withoutUnderscore, out values))
        {
            return values;
        }
        
        string withUnderscore = InsertUnderscores(cardKey);
        if (_cardValuesMap.TryGetValue(withUnderscore, out values))
        {
            return values;
        }
        
        return null;
    }
    
    private string InsertUnderscores(string str)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            if (i > 0 && char.IsUpper(str[i]) && char.IsLower(str[i - 1]))
            {
                sb.Append('_');
            }
            sb.Append(str[i]);
        }
        return sb.ToString();
    }
    
    /// <summary>
    /// 从完整的卡牌ID中提取卡牌名称部分
    /// 例如：RED_ALERT2_MOD_CARD_DREADNOUGHT -> DREADNOUGHT
    /// </summary>
    private string ExtractCardName(string cardKey)
    {
        // 移除前缀 RED_ALERT2_MOD_CARD_
        string prefix = "RED_ALERT2_MOD_CARD_";
        if (cardKey.StartsWith(prefix))
        {
            return cardKey.Substring(prefix.Length);
        }
        
        // 移除前缀 MOD_CARD_
        prefix = "MOD_CARD_";
        if (cardKey.StartsWith(prefix))
        {
            return cardKey.Substring(prefix.Length);
        }
        
        // 移除前缀 CARD_
        prefix = "CARD_";
        if (cardKey.StartsWith(prefix))
        {
            return cardKey.Substring(prefix.Length);
        }
        
        // 如果没有找到前缀，返回最后一个下划线之后的部分
        int lastUnderscoreIndex = cardKey.LastIndexOf('_');
        if (lastUnderscoreIndex >= 0 && lastUnderscoreIndex < cardKey.Length - 1)
        {
            return cardKey.Substring(lastUnderscoreIndex + 1);
        }
        
        return string.Empty;
    }

    private string ReplaceDynamicVars(CardModel card, string text)
    {
        if (card.DynamicVars == null)
        {
            return text;
        }

        foreach (var kvp in card.DynamicVars)
        {
            string varName = kvp.Key;
            var varItem = kvp.Value;
            string pattern = $"\\$?\\{{{varName}\\}}";
            
            object? value = null;
            var valueProp = varItem.GetType().GetProperty("Value");
            if (valueProp != null)
            {
                value = valueProp.GetValue(varItem);
            }
            else
            {
                var intValueProp = varItem.GetType().GetProperty("IntValue");
                if (intValueProp != null)
                {
                    value = intValueProp.GetValue(varItem);
                }
                else
                {
                    var stringValueProp = varItem.GetType().GetProperty("StringValue");
                    if (stringValueProp != null)
                    {
                        value = stringValueProp.GetValue(varItem);
                    }
                }
            }

            if (value != null)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, pattern, value.ToString() ?? "");
            }
        }

        return text;
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

    private void ToggleCardSelection(int index)
    {
        bool isCurrentlySelected = _selectionOrder.Contains(index);
        
        if (isCurrentlySelected)
        {
            // 取消选中
            _selectionOrder.Remove(index);
        }
        else
        {
            // 选中，添加到选择顺序末尾
            _selectionOrder.Add(index);
            // 确保数量至少为1
            if (!_cardQuantities.ContainsKey(index) || _cardQuantities[index] < 1)
            {
                _cardQuantities[index] = 1;
            }
        }
        
        // 更新UI显示
        int count = _cardQuantities.TryGetValue(index, out int c) ? c : 1;
        UpdateQuantityDisplay(index, count);
    }

    private void OnCardSelected(CardModel card)
    {
        if (_choiceLocked) return;
        
        if (_isQuantitySelect)
        {
            // 数量选择模式：点击卡牌切换选中状态
            int index = _cards.IndexOf(card);
            ToggleCardSelection(index);
        }
        else if (_isMultiSelect)
        {
            // 多选模式：切换选中状态
            if (_selectedCards.Contains(card))
            {
                _selectedCards.Remove(card);
                // 更新按钮样式表示取消选中
                UpdateCardButtonStyle(card, false);
            }
            else if (_selectedCards.Count < _maxSelection)
            {
                _selectedCards.Add(card);
                // 更新按钮样式表示选中
                UpdateCardButtonStyle(card, true);
            }
        }
        else
        {
            // 单选模式
            _choiceLocked = true;
            _completionSource.TrySetResult(card);
            NOverlayStack.Instance?.Remove(this);
        }
    }

    private void UpdateCardButtonStyle(CardModel card, bool isSelected)
    {
        // 通过按钮名称找到对应卡牌的按钮（使用卡牌在列表中的索引）
        int index = _cards.IndexOf(card);
        string buttonName = $"CardButton_{card.Id.Entry}_{index}";
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
        // 点击取消按钮，传递false表示取消操作
        Close(false);
    }

    public void Close(bool isConfirmed = false)
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        
        if (_isQuantitySelect)
        {
            if (isConfirmed)
            {
                // 确认按钮点击：收集选择结果（支持空选）
                List<CardSelectionResult> results = new();
                foreach (int index in _selectionOrder)
                {
                    if (_cardQuantities.TryGetValue(index, out int count) && count > 0 && index >= 0 && index < _cards.Count)
                    {
                        results.Add(new CardSelectionResult
                        {
                            Card = _cards[index],
                            Count = count
                        });
                    }
                }
                // 空选时返回空列表，表示确认但未选择任何卡牌
                _quantityCompletionSource.TrySetResult(results);
            }
            else
            {
                // 取消按钮点击：返回null
                _quantityCompletionSource.TrySetResult(null);
            }
        }
        else if (_isMultiSelect)
        {
            if (isConfirmed && _selectedCards.Count >= _minSelection)
            {
                _multiCompletionSource.TrySetResult(new List<CardModel>(_selectedCards));
            }
            else
            {
                _multiCompletionSource.TrySetResult(null);
            }
        }
        else
        {
            _completionSource.TrySetResult(null);
        }
        
        NOverlayStack.Instance?.Remove(this);
    }

    private void OnConfirmClicked()
    {
        if (_choiceLocked) return;
        
        // 点击确认按钮，传递true表示确认操作
        if (_isQuantitySelect)
        {
            // 数量选择模式：检查是否有选中的单位
            if (_selectionOrder.Count == 0)
            {
                ShowEmptySelectionConfirmDialog();
                return;
            }
            Close(true);
        }
        else if (_selectedCards.Count >= _minSelection)
        {
            Close(true);
        }
        else if (_selectedCards.Count == 0)
        {
            // 多选模式但未选中任何单位
            ShowEmptySelectionConfirmDialog();
        }
    }

    private void ShowEmptySelectionConfirmDialog()
    {
        // 创建弹窗背景
        ColorRect dialogBackdrop = new()
        {
            Name = "EmptySelectionDialogBackdrop",
            Color = new Color(0.0f, 0.0f, 0.0f, 0.7f),
            MouseFilter = MouseFilterEnum.Stop
        };
        dialogBackdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(dialogBackdrop);

        // 创建弹窗容器
        CenterContainer dialogCenter = new() { Name = "EmptySelectionDialogCenter" };
        dialogCenter.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        dialogBackdrop.AddChild(dialogCenter);

        PanelContainer dialogPanel = new()
        {
            Name = "EmptySelectionDialogPanel",
            CustomMinimumSize = new Vector2(400f, 200f)
        };
        dialogPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        dialogCenter.AddChild(dialogPanel);

        MarginContainer dialogMargin = new();
        dialogMargin.AddThemeConstantOverride("margin_left", 30);
        dialogMargin.AddThemeConstantOverride("margin_right", 30);
        dialogMargin.AddThemeConstantOverride("margin_top", 30);
        dialogMargin.AddThemeConstantOverride("margin_bottom", 30);
        dialogPanel.AddChild(dialogMargin);

        VBoxContainer dialogContent = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        dialogContent.AddThemeConstantOverride("separation", 20);
        dialogMargin.AddChild(dialogContent);

        // 提示文本
        string message = GetLocStringText(new LocString("card_keywords", "ui.card_select.empty_confirm"));
        Label messageLabel = new()
        {
            Text = message,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        messageLabel.AddThemeFontSizeOverride("font_size", 18);
        messageLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        dialogContent.AddChild(messageLabel);

        // 按钮容器
        HBoxContainer buttonContainer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };
        buttonContainer.AddThemeConstantOverride("separation", 20);
        dialogContent.AddChild(buttonContainer);

        // 取消按钮
        Button cancelBtn = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.cancel")),
            CustomMinimumSize = new Vector2(120f, 40f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        cancelBtn.AddThemeStyleboxOverride("normal", CreateCancelStyle());
        cancelBtn.AddThemeStyleboxOverride("hover", CreateCancelStyle(new Color(0.6f, 0.15f, 0.15f, 0.9f)));
        cancelBtn.AddThemeStyleboxOverride("pressed", CreateCancelStyle(new Color(0.35f, 0.08f, 0.08f, 0.95f)));
        cancelBtn.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.85f));
        cancelBtn.AddThemeFontSizeOverride("font_size", 18);
        cancelBtn.Pressed += () => dialogBackdrop.QueueFree();
        buttonContainer.AddChild(cancelBtn);

        // 确认按钮
        Button confirmBtn = new()
        {
            Text = GetLocStringText(new LocString("card_keywords", "ui.production_queue.confirm")),
            CustomMinimumSize = new Vector2(120f, 40f),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };
        confirmBtn.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.1f, 0.3f, 0.15f)));
        confirmBtn.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.15f, 0.4f, 0.2f)));
        confirmBtn.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.08f, 0.25f, 0.12f)));
        confirmBtn.AddThemeColorOverride("font_color", new Color(0.9f, 1f, 0.9f));
        confirmBtn.AddThemeFontSizeOverride("font_size", 18);
        confirmBtn.Pressed += () =>
        {
            dialogBackdrop.QueueFree();
            Close(true);
        };
        buttonContainer.AddChild(confirmBtn);
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }

    public override void _ExitTree()
    {
        _completionSource.TrySetCanceled();
        _multiCompletionSource.TrySetCanceled();
        _quantityCompletionSource.TrySetCanceled();
        base._ExitTree();
    }
}