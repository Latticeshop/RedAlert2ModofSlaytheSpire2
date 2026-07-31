// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace RedAlert2ModCode.DeckConfig;

/// <summary>
/// 卡牌库选择Tab - 用于浏览和选择卡牌添加到自定义卡组
/// </summary>
internal class CardLibraryTab
{
    private const int CardsPerPage = 24;
    private const int Columns = 6;

    private static string _searchText = string.Empty;
    private static readonly Dictionary<CardType, bool> _typeFilter = new()
    {
        [CardType.Attack] = true,
        [CardType.Skill] = true,
        [CardType.Power] = true,
        [CardType.Status] = true,
        [CardType.Curse] = true,
    };
    private static int _pageIndex;

    private readonly CharacterConfig _config;
    private readonly Action _onChanged;
    private CanvasLayer? _layer;
    private ScrollContainer? _scrollContainer;
    private VBoxContainer? _contentContainer;

    public CardLibraryTab(CharacterConfig config, Action onChanged)
    {
        _config = config;
        _onChanged = onChanged;
    }

    public void Show()
    {
        Build();
    }

    public void Close()
    {
        if (_layer != null && GodotObject.IsInstanceValid(_layer))
        {
            _layer.QueueFree();
        }
        _layer = null;
    }

    private void Build()
    {
        _layer = new CanvasLayer();
        _layer.Layer = 102;
        _layer.Name = "CardLibraryTab";

        // 背景遮罩
        var backstop = new ColorRect();
        backstop.Color = new Color(0f, 0f, 0f, 0.7f);
        backstop.AnchorRight = 1;
        backstop.AnchorBottom = 1;
        backstop.MouseFilter = Control.MouseFilterEnum.Stop;
        backstop.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                Close();
                backstop.GetViewport()?.SetInputAsHandled();
                _onChanged();
            }
        };
        _layer.AddChild(backstop);

        // 主面板
        var panel = new PanelContainer();
        panel.AnchorLeft = 0.05f;
        panel.AnchorRight = 0.95f;
        panel.AnchorTop = 0.05f;
        panel.AnchorBottom = 0.95f;
        panel.GrowHorizontal = Control.GrowDirection.Both;
        panel.GrowVertical = Control.GrowDirection.Both;
        panel.MouseFilter = Control.MouseFilterEnum.Stop;

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.97f);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Color("B89840");
        panelStyle.SetCornerRadiusAll(8);
        panelStyle.SetContentMarginAll(12);
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        _layer.AddChild(panel);

        var mainVBox = new VBoxContainer();
        mainVBox.AnchorRight = 1;
        mainVBox.AnchorBottom = 1;
        mainVBox.AddThemeConstantOverride("separation", 8);
        panel.AddChild(mainVBox);

        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "选择卡牌添加到卡组";
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        titleLabel.AddThemeColorOverride("font_color", StsColors.gold);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(titleLabel);

        // 搜索栏
        BuildSearchBar(mainVBox);

        // 类型筛选
        BuildTypeFilterBar(mainVBox);

        // 分隔符
        var divider = new ColorRect();
        divider.CustomMinimumSize = new Vector2(0, 2);
        divider.Color = new Color(0.91f, 0.86f, 0.75f, 0.25f);
        divider.MouseFilter = Control.MouseFilterEnum.Ignore;
        mainVBox.AddChild(divider);

        // 滚动容器
        _scrollContainer = new ScrollContainer();
        _scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        mainVBox.AddChild(_scrollContainer);

        _contentContainer = new VBoxContainer();
        _contentContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _contentContainer.AddThemeConstantOverride("separation", 6);
        _scrollContainer.AddChild(_contentContainer);

        // 底部按钮
        var buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 12);
        buttonRow.Alignment = BoxContainer.AlignmentMode.Center;
        mainVBox.AddChild(buttonRow);

        var closeBtn = CreateActionButton("关闭", StsColors.red);
        closeBtn.CustomMinimumSize = new Vector2(100, 36);
        closeBtn.Pressed += () =>
        {
            Close();
            _onChanged();
        };
        buttonRow.AddChild(closeBtn);

        RefreshContent();
        NGame.Instance?.AddChild(_layer);
    }

    private void BuildSearchBar(VBoxContainer container)
    {
        var searchBox = new HBoxContainer();
        searchBox.AddThemeConstantOverride("separation", 6);

        var searchLabel = new Label();
        searchLabel.Text = "搜索:";
        searchLabel.AddThemeFontSizeOverride("font_size", 14);
        searchLabel.AddThemeColorOverride("font_color", StsColors.cream);
        searchBox.AddChild(searchLabel);

        var searchInput = new LineEdit();
        searchInput.Text = _searchText;
        searchInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        searchInput.CustomMinimumSize = new Vector2(200, 32);
        searchInput.AddThemeFontSizeOverride("font_size", 14);
        searchInput.TextChanged += (string newText) =>
        {
            _searchText = newText;
            _pageIndex = 0;
            RefreshContent();
        };
        searchBox.AddChild(searchInput);

        if (!string.IsNullOrEmpty(_searchText))
        {
            var clearBtn = CreateActionButton("清除", StsColors.red);
            clearBtn.Pressed += () =>
            {
                _searchText = string.Empty;
                _pageIndex = 0;
                RefreshContent();
            };
            searchBox.AddChild(clearBtn);
        }

        container.AddChild(searchBox);
    }

    private void BuildTypeFilterBar(VBoxContainer container)
    {
        var filterBox = new HBoxContainer();
        filterBox.AddThemeConstantOverride("separation", 4);
        container.AddChild(filterBox);

        var filterLabel = new Label();
        filterLabel.Text = "类型:";
        filterLabel.AddThemeFontSizeOverride("font_size", 14);
        filterLabel.AddThemeColorOverride("font_color", StsColors.cream);
        filterLabel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        filterBox.AddChild(filterLabel);

        (CardType type, string name)[] types =
        [
            (CardType.Attack, "攻击"),
            (CardType.Skill, "技能"),
            (CardType.Power, "能力"),
            (CardType.Status, "衍生"),
            (CardType.Curse, "诅咒"),
        ];

        foreach (var (type, name) in types)
        {
            bool enabled = _typeFilter.GetValueOrDefault(type, true);
            var btn = CreateFilterButton(name, enabled);
            btn.Pressed += () =>
            {
                _typeFilter[type] = !_typeFilter.GetValueOrDefault(type, true);
                btn.AddThemeColorOverride("font_color", _typeFilter[type] ? StsColors.green : StsColors.gray);
                btn.UpdateMinimumSize();
                RefreshContent();
            };
            filterBox.AddChild(btn);
        }
    }

    private void RefreshContent()
    {
        if (_contentContainer == null) return;
        ClearChildren(_contentContainer);

        // 获取所有卡牌并筛选
        var allCards = GetFilteredCards();
        int totalPages = Math.Max(1, (allCards.Count + CardsPerPage - 1) / CardsPerPage);
        _pageIndex = Math.Clamp(_pageIndex, 0, totalPages - 1);

        var pageCards = allCards.Skip(_pageIndex * CardsPerPage).Take(CardsPerPage).ToList();

        if (pageCards.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = string.IsNullOrEmpty(_searchText) ? "没有找到卡牌" : "没有匹配的卡牌";
            emptyLabel.AddThemeFontSizeOverride("font_size", 14);
            emptyLabel.AddThemeColorOverride("font_color", StsColors.gray);
            _contentContainer.AddChild(emptyLabel);
            return;
        }

        // 卡牌网格
        var grid = new GridContainer { Columns = Columns };
        grid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 16);
        _contentContainer.AddChild(grid);

        foreach (var card in pageCards)
        {
            grid.AddChild(CreateCardCell(card));
        }

        // 分页导航
        if (totalPages > 1)
        {
            BuildPageNav(_contentContainer, totalPages, allCards.Count);
        }
    }

    private List<CardModel> GetFilteredCards()
    {
        var cards = new List<CardModel>();
        try
        {
            foreach (var card in ModelDb.AllCards)
            {
                if (!_typeFilter.GetValueOrDefault(card.Type, false)) continue;

                if (!string.IsNullOrEmpty(_searchText))
                {
                    string search = _searchText.ToLowerInvariant();
                    bool matches = false;
                    try
                    {
                        if (card.Title.ToLowerInvariant().Contains(search)) matches = true;
                    }
                    catch { }
                    if (card.Id.Entry.ToLowerInvariant().Contains(search)) matches = true;
                    if (!matches) continue;
                }

                cards.Add(card);
            }
        }
        catch { }

        return cards.OrderBy(c => c.Id.Entry).ToList();
    }

    private Control CreateCardCell(CardModel card)
    {
        var cell = new VBoxContainer();
        cell.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        cell.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        cell.CustomMinimumSize = new Vector2(120, 175);
        cell.AddThemeConstantOverride("separation", 4);
        cell.Alignment = BoxContainer.AlignmentMode.Center;

        var cardFrame = new PanelContainer();
        cardFrame.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        cardFrame.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        var frameStyle = new StyleBoxFlat();
        frameStyle.BgColor = new Color(0.08f, 0.06f, 0.10f, 0.9f);
        frameStyle.SetBorderWidthAll(1);
        frameStyle.BorderColor = new Color(0.45f, 0.40f, 0.30f, 0.5f);
        frameStyle.SetCornerRadiusAll(4);
        frameStyle.SetContentMarginAll(2);
        cardFrame.AddThemeStyleboxOverride("panel", frameStyle);

        NCard? nCard = null;
        try
        {
            var displayCard = card.IsMutable ? card : card.ToMutable();
            nCard = NCard.Create(displayCard);
            if (nCard != null)
            {
                nCard.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
                nCard.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
                nCard.Scale = new Vector2(0.32f, 0.32f);
                nCard.MouseFilter = Control.MouseFilterEnum.Ignore;
                nCard.CustomMinimumSize = new Vector2(100, 130);
                cardFrame.AddChild(nCard);

                nCard.Ready += () =>
                {
                    if (GodotObject.IsInstanceValid(nCard))
                        nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                };
            }
        }
        catch { }

        cell.AddChild(cardFrame);

        cell.MouseEntered += () =>
        {
            try { ShowHoverTips(cell, card.HoverTips, HoverTipAlignment.Left); }
            catch { }
        };
        cell.MouseExited += () => NHoverTipSet.Remove(cell);

        cell.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                cell.GetViewport()?.SetInputAsHandled();
                AddCardToDeck(card);
            }
        };

        var addBtn = CreateActionButton("➕ 添加", StsColors.gold);
        addBtn.CustomMinimumSize = new Vector2(90, 28);
        addBtn.Pressed += () => AddCardToDeck(card);
        cell.AddChild(addBtn);

        return cell;
    }

    private void AddCardToDeck(CardModel card)
    {
        string typeName = card.GetType().Name;
        ModConfigPanel.AddCardToDeck(_config, typeName);
        ShowNotification($"已添加: {GetCardTitle(card)}");
        RefreshContent();
    }

    private string GetCardTitle(CardModel card)
    {
        try { return card.Title; }
        catch { return card.Id.Entry; }
    }

    private void BuildPageNav(VBoxContainer container, int totalPages, int totalCount)
    {
        var nav = new HBoxContainer();
        nav.AddThemeConstantOverride("separation", 6);

        if (_pageIndex > 0)
        {
            var prevBtn = CreateActionButton("← 上一页", StsColors.cream);
            prevBtn.Pressed += () =>
            {
                _pageIndex--;
                RefreshContent();
            };
            nav.AddChild(prevBtn);
        }

        var pageLabel = new Label();
        pageLabel.Text = $"第 {_pageIndex + 1}/{totalPages} 页 (共 {totalCount} 张)";
        pageLabel.AddThemeFontSizeOverride("font_size", 13);
        pageLabel.AddThemeColorOverride("font_color", StsColors.cream);
        nav.AddChild(pageLabel);

        if (_pageIndex < totalPages - 1)
        {
            var nextBtn = CreateActionButton("下一页 →", StsColors.cream);
            nextBtn.Pressed += () =>
            {
                _pageIndex++;
                RefreshContent();
            };
            nav.AddChild(nextBtn);
        }

        container.AddChild(nav);
    }

    // ============ UI辅助方法 ============

    private Button CreateActionButton(string text, Color? fontColor = null)
    {
        var btn = new Button();
        btn.Text = text;
        btn.AddThemeFontSizeOverride("font_size", 13);
        btn.AddThemeColorOverride("font_color", fontColor ?? StsColors.cream);
        btn.AddThemeColorOverride("font_hover_color", StsColors.gold);
        btn.AddThemeColorOverride("font_pressed_color", StsColors.gray);
        ApplyFlatStyle(btn);
        return btn;
    }

    private Button CreateFilterButton(string text, bool enabled)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(70, 28);
        btn.AddThemeFontSizeOverride("font_size", 13);
        btn.AddThemeColorOverride("font_color", enabled ? StsColors.green : StsColors.gray);
        ApplyFlatStyle(btn);
        return btn;
    }

    private void ApplyFlatStyle(Button btn)
    {
        btn.AddThemeStyleboxOverride("normal", CreateStyleBox(
            new Color(0.12f, 0.10f, 0.15f, 0.85f),
            new Color(0.35f, 0.30f, 0.25f, 0.5f)));
        btn.AddThemeStyleboxOverride("hover", CreateStyleBox(
            new Color(0.18f, 0.15f, 0.22f, 0.92f),
            StsColors.gold));
        btn.AddThemeStyleboxOverride("pressed", CreateStyleBox(
            new Color(0.08f, 0.06f, 0.10f, 0.95f),
            new Color("B89840")));
        btn.AddThemeStyleboxOverride("focus", CreateStyleBox(
            new Color(0.18f, 0.15f, 0.22f, 0.92f),
            StsColors.gold));
    }

    private StyleBoxFlat CreateStyleBox(Color bg, Color border)
    {
        var sb = new StyleBoxFlat();
        sb.BgColor = bg;
        sb.BorderColor = border;
        sb.SetBorderWidthAll(2);
        sb.SetCornerRadiusAll(6);
        sb.SetContentMarginAll(6);
        return sb;
    }

    private void ShowHoverTips(Control owner, IEnumerable<IHoverTip> tips, HoverTipAlignment alignment)
    {
        try
        {
            var tipSet = NHoverTipSet.CreateAndShow(owner, tips, alignment);
            if (tipSet != null && GodotObject.IsInstanceValid(tipSet))
            {
                tipSet.GetParent()?.RemoveChild(tipSet);
                NGame.Instance?.AddChild(tipSet);
            }
        }
        catch { }
    }

    private void ShowNotification(string message)
    {
        GD.Print($"[CardLibrary] {message}");
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
