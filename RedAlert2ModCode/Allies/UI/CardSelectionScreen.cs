using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace RedAlert2ModCode.Allies.UI;

public sealed partial class CardSelectionScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<CardModel> _completionSource = new();
    private readonly List<CardModel> _cards;
    private ScrollContainer _scrollContainer;
    private HBoxContainer _cardsRow;
    private bool _choiceLocked;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private CardSelectionScreen(List<CardModel> cards)
    {
        _cards = cards;
        Name = nameof(CardSelectionScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<CardModel> ShowSelection(List<CardModel> cards)
    {
        var screen = new CardSelectionScreen(cards);
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
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        panel.AddChild(margin);

        VBoxContainer root = new() { Alignment = BoxContainer.AlignmentMode.Center };
        root.AddThemeConstantOverride("separation", 15);
        margin.AddChild(root);

        MegaLabel title = new()
        {
            Text = "请选择单位",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MaxFontSize = 32,
            MinFontSize = 20
        };
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
        _cardsRow.AddThemeConstantOverride("separation", 8);
        _scrollContainer.AddChild(_cardsRow);

        foreach (var card in _cards)
        {
            _cardsRow.AddChild(CreateCardButton(card));
        }
    }

    private Button CreateCardButton(CardModel card)
    {
        Button button = new()
        {
            Name = $"{card.Id.Entry}_Button",
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
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 6);
        contentMargin.AddChild(content);

        if (!string.IsNullOrEmpty(card.PortraitPath) && ResourceLoader.Exists(card.PortraitPath))
        {
            TextureRect texture = new()
            {
                Texture = ResourceLoader.Load<Texture2D>(card.PortraitPath),
                CustomMinimumSize = new Vector2(140f, 140f),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            content.AddChild(texture);
        }

        // 正确获取能量费用
        string costText = $"费用：{GetEnergyCostText(card)}";

        MegaLabel cost = new()
        {
            Text = costText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MaxFontSize = 18,
            MinFontSize = 14,
            Modulate = new Color(1f, 0.9f, 0.2f)
        };
        content.AddChild(cost);

        // 正确获取卡牌名称
        string titleText = GetCardTitle(card);
        MegaLabel name = new()
        {
            Text = titleText,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MaxFontSize = 20,
            MinFontSize = 14
        };
        content.AddChild(name);

        // 正确获取卡牌描述（包含动态变量转义和IfUpgraded处理）
        string descText = GetCardDescription(card, card.IsUpgraded);
        // 字符数截断，超过65字符时省略
        if (!string.IsNullOrEmpty(descText) && descText.Length > 65)
        {
            descText = descText.Substring(0, 65) + "...";
        }

        MegaLabel descLabel = new()
        {
            Text = descText,
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxFontSize = 12,
            MinFontSize = 10,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        content.AddChild(descLabel);

        button.Pressed += () => OnCardSelected(card);

        return button;
    }

    private string GetLocStringText(object? locStringObj)
    {
        if (locStringObj == null) return string.Empty;

        // 尝试调用 GetFormattedText() 方法获取格式化后的本地化文本
        System.Reflection.MethodInfo? formatMethod = locStringObj.GetType().GetMethod("GetFormattedText");
        if (formatMethod != null)
        {
            object? result = formatMethod.Invoke(locStringObj, null);
            if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
            {
                return formattedText;
            }
        }

        // 回退到 GetRawText() 方法
        System.Reflection.MethodInfo? rawMethod = locStringObj.GetType().GetMethod("GetRawText");
        if (rawMethod != null)
        {
            object? result = rawMethod.Invoke(locStringObj, null);
            if (result is string rawText && !string.IsNullOrEmpty(rawText))
            {
                return rawText;
            }
        }

        // 最后回退到 ToString()
        string str = locStringObj.ToString() ?? string.Empty;
        if (!str.StartsWith("MegaCrit.Sts2.Core.Localization") && !str.Contains("LocString"))
        {
            return str;
        }

        return string.Empty;
    }

    private string GetEnergyCostText(CardModel card)
    {
        if (card.EnergyCost == null)
        {
            return "0";
        }
        
        // 尝试通过反射获取能量费用的实际值
        var costType = card.EnergyCost.GetType();
        var baseCostProp = costType.GetProperty("BaseCost");
        if (baseCostProp != null)
        {
            var baseCost = baseCostProp.GetValue(card.EnergyCost);
            if (baseCost != null)
            {
                return baseCost.ToString();
            }
        }
        
        // 尝试获取 Value 属性
        var valueProp = costType.GetProperty("Value");
        if (valueProp != null)
        {
            var value = valueProp.GetValue(card.EnergyCost);
            if (value != null)
            {
                return value.ToString();
            }
        }
        
        // 尝试获取整数形式的值
        var intValueProp = costType.GetProperty("IntValue");
        if (intValueProp != null)
        {
            var intValue = intValueProp.GetValue(card.EnergyCost);
            if (intValue != null)
            {
                return intValue.ToString();
            }
        }
        
        // 默认返回类型名中的数字或0
        string costStr = card.EnergyCost.ToString();
        if (costStr.StartsWith("MegaCrit.Sts2.Core.Entities.Cards"))
        {
            return "0";
        }
        return costStr;
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
        string desc = GetLocStringText(card.Description);
        if (string.IsNullOrEmpty(desc))
        {
            return string.Empty;
        }

        // 处理 {IfUpgraded:show:xxx|} 格式的条件标签
        // 如果是升级卡牌，提取 :show: 和 | 之间的内容；否则移除整个标签
        desc = ProcessIfUpgradedTags(desc, isUpgraded);

        // 转义动态变量（如 {Damage}, {Repeat}, {Block} 等）
        desc = ReplaceDynamicVars(card, desc);

        return desc;
    }

    private string ProcessIfUpgradedTags(string text, bool isUpgraded)
    {
        // 匹配 {IfUpgraded:show:升级内容|普通内容} 格式
        // 如果是升级卡牌，保留升级内容；否则保留普通内容
        return System.Text.RegularExpressions.Regex.Replace(text, 
            @"\{IfUpgraded:show:([^|]+)\|([^}]*)\}", 
            match => isUpgraded ? match.Groups[1].Value.Trim() : match.Groups[2].Value.Trim());
    }

    private string ReplaceDynamicVars(CardModel card, string text)
    {
        if (card.DynamicVars == null)
        {
            return text;
        }

        // 遍历所有动态变量，替换文本中的 {VarName} 为实际值
        foreach (var varItem in card.DynamicVars)
        {
            string varName = varItem.GetType().Name.Replace("Var", "");
            string pattern = $"\\{{{varName}\\}}";
            
            // 尝试获取变量值
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
            }

            if (value != null)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, pattern, value.ToString() ?? "");
            }
        }

        return text;
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
        style.BorderColor = new Color(0.3f, 0.5f, 0.8f);
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
        style.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        return style;
    }

    private void OnCardSelected(CardModel card)
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(card);
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