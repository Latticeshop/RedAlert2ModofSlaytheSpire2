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
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliedTransportShip : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.AlliedTransportShip;

    private List<CardModel> _storedCards = new List<CardModel>();
    private bool _hasStored;

    public AlliedTransportShip() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = new List<CardKeyword>();
            keywords.Add(CardKeyword.Retain);
            if (_hasStored)
            {
                keywords.Add(CardKeyword.Exhaust);
            }
            return keywords;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new StringVar("StoredCards"),
        new IntVar("StoreCount", IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber),
        new IntVar("DollarNumber", Values.DollarValue),
        // Move：打出卡牌获得的格挡吃敏捷加成（与原版格挡卡一致）
        new BlockVar(Values.Block, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT1.CreateHoverTip(),
        ModCardKeywords.Navy.CreateHoverTip()
    ];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _storedCards = new List<CardModel>(_storedCards);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        GD.Print($"[AlliedTransportShip] OnPlay 被调用 - IsUpgraded={IsUpgraded}, _hasStored={_hasStored}");

        if (!_hasStored)
        {
            await StoreCards(choiceContext, cardPlay);
        }
        else
        {
            await ReleaseCards(cardPlay);
        }
    }

    private async Task StoreCards(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int countToStore = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
        
        GD.Print($"[AlliedTransportShip] 准备存储最多 {countToStore} 张卡牌");

        // 手牌中没有可存储的卡牌时，跳过选择界面，直接正常打出（避免卡死）
        var handPile = PileType.Hand.GetPile(Owner);
        if (!handPile.Cards.Any(c => c != this))
        {
            GD.Print("[AlliedTransportShip] 手牌中没有可存储的卡牌，跳过选择并正常打出");
            await CardPileCmd.Add(this, Keywords.Contains(CardKeyword.Exhaust) ? PileType.Exhaust : PileType.Discard, CardPilePosition.Bottom, this);
            return;
        }

        // 原版 FromHand 会触发 CancelAllCardPlay（取消回手流程），联机中选择时会阻塞其他玩家出牌；
        // 改用与超时空传送一致的 ExecuteSyncChoice + mod 选择 UI（仅暂停选择者，不取消回手）。
        var selectableCards = PileType.Hand.GetPile(base.Owner).Cards.Where(c => c != this).ToList();
        var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(
            choiceContext, selectableCards, countToStore, 0, base.Owner)
            ?? new List<CardModel>();

        foreach (var card in selectedCards)
        {
            _storedCards.Add(card);
            GD.Print($"[AlliedTransportShip] 存储卡牌: {card.Title}");
        }

        foreach (var card in _storedCards)
        {
            await CardPileCmd.RemoveFromCombat(card);
        }

        if (_storedCards.Count > 0)
        {
            _hasStored = true;
            var storedText = new LocString("cards", $"{Id.Entry}.stored_info");
            storedText.Add("0", string.Join(", ", _storedCards.Select(c => c.Title)));
            ((StringVar)DynamicVars["StoredCards"]).StringValue = storedText.GetFormattedText();
            GD.Print($"[AlliedTransportShip] 存储完成，已存储 {_storedCards.Count} 张卡牌");

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            GD.Print($"[AlliedTransportShip] 获得 {DynamicVars.Block.IntValue} 点格挡");
        }

        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
        GD.Print("[AlliedTransportShip] 返回手牌");
    }

    private async Task ReleaseCards(CardPlay cardPlay)
    {
        if (_storedCards.Count == 0)
            return;

        GD.Print($"[AlliedTransportShip] 释放存储的卡牌，数量: {_storedCards.Count}");

        foreach (var card in _storedCards)
        {
            card.HasBeenRemovedFromState = false;
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            GD.Print($"[AlliedTransportShip] 释放卡牌: {card.Title}");
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        GD.Print($"[AlliedTransportShip] 释放卡牌，获得 {DynamicVars.Block.IntValue} 点格挡");

        _storedCards.Clear();
        _hasStored = false;
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
        GD.Print("[AlliedTransportShip] 释放完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StoreCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}
