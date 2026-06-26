#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 运输船 - 盟军海军单位卡
/// 1费技能卡，可存储手牌中的卡牌（每张运输船独立存储）
/// </summary>
public sealed class TransportShip : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.TransportShip;

    // 存储的卡牌列表（每张运输船独立）
    private List<CardModel> _storedCards = new List<CardModel>();
    // 是否有存储的卡牌
    private bool _hasStored;

    public TransportShip() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png";

    /// <summary>
    /// 使用原版"保留"词条
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new StringVar("StoredCards"),
        new IntVar("StoreCount", IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber),
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Navy.CreateHoverTip()
    ];

    /// <summary>
    /// 深度克隆字段，确保克隆牌拥有独立的存储列表
    /// </summary>
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _storedCards = new List<CardModel>(_storedCards);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        GD.Print($"[TransportShip] OnPlay 被调用 - IsUpgraded={IsUpgraded}, _hasStored={_hasStored}");

        if (!_hasStored)
        {
            await StoreCards(choiceContext);
        }
        else
        {
            await ReleaseCards();
        }
    }

    private async Task StoreCards(PlayerChoiceContext choiceContext)
    {
        int countToStore = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
        
        GD.Print($"[TransportShip] 准备存储最多 {countToStore} 张卡牌");

        // 使用自定义提示，允许选择0到countToStore张卡牌
        var selectPrompt = new LocString("cards", "TRANSPORT_SHIP.select_prompt");
        selectPrompt.Add("0", 0);
        selectPrompt.Add("1", countToStore);
        var prefs = new CardSelectorPrefs(selectPrompt, 0, countToStore)
        {
            RequireManualConfirmation = true
        };

        var selectedCards = (await CardSelectCmd.FromHand(
            choiceContext,
            base.Owner,
            prefs,
            c => c != this,
            this
        )).ToList();

        if (selectedCards.Count == 0)
        {
            GD.Print("[TransportShip] 玩家选择0张卡牌");
            return;
        }

        GD.Print($"[TransportShip] 玩家选择了 {selectedCards.Count} 张卡牌进行存储");

        foreach (var card in selectedCards)
        {
            // 从手牌 UI 中移除卡牌
            NPlayerHand.Instance?.Remove(card);
            // 从数据层移除，但不标记为消耗
            card.RemoveFromCurrentPile();
            _storedCards.Add(card);
            GD.Print($"[TransportShip] 存储卡牌: {card.Title}");
        }

        _hasStored = true;
        // 更新动态变量，显示存储的卡牌名称
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
        GD.Print($"[TransportShip] 存储完成，已存储 {_storedCards.Count} 张卡牌");
    }

    private async Task ReleaseCards()
    {
        if (_storedCards.Count == 0)
            return;

        GD.Print($"[TransportShip] 释放存储的卡牌，数量: {_storedCards.Count}");

        foreach (var card in _storedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            GD.Print($"[TransportShip] 释放卡牌: {card.Title}");
        }

        _storedCards.Clear();
        _hasStored = false;
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
        GD.Print("[TransportShip] 释放完成");
    }

    protected override void OnUpgrade()
    {
        // 升级后存储数量从3张增加到5张
        DynamicVars["StoreCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}
