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

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietTransportShip : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietTransportShip;

    private List<CardModel> _storedCards = new List<CardModel>();
    private bool _hasStored;

    public SovietTransportShip() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/armoredTransport.png";

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
        new BlockVar(Values.Block, ValueProp.Unpowered)
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
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
        GD.Print($"[SovietTransportShip] OnPlay 被调用 - IsUpgraded={IsUpgraded}, _hasStored={_hasStored}");

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
        
        GD.Print($"[SovietTransportShip] 准备存储最多 {countToStore} 张卡牌");

        var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_SOVIET_TRANSPORT_SHIP.select_prompt");
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

        foreach (var card in selectedCards)
        {
            _storedCards.Add(card);
            GD.Print($"[SovietTransportShip] 存储卡牌: {card.Title}");
        }

        foreach (var card in _storedCards)
        {
            await CardPileCmd.RemoveFromCombat(card);
        }

        if (_storedCards.Count > 0)
        {
            _hasStored = true;
            ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
            GD.Print($"[SovietTransportShip] 存储完成，已存储 {_storedCards.Count} 张卡牌");

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            GD.Print($"[SovietTransportShip] 获得 {DynamicVars.Block.IntValue} 点格挡");
        }

        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
        GD.Print("[SovietTransportShip] 返回手牌");
    }

    private async Task ReleaseCards(CardPlay cardPlay)
    {
        if (_storedCards.Count == 0)
            return;

        GD.Print($"[SovietTransportShip] 释放存储的卡牌，数量: {_storedCards.Count}");

        foreach (var card in _storedCards)
        {
            card.HasBeenRemovedFromState = false;
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            GD.Print($"[SovietTransportShip] 释放卡牌: {card.Title}");
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        GD.Print($"[SovietTransportShip] 释放卡牌，获得 {DynamicVars.Block.IntValue} 点格挡");

        _storedCards.Clear();
        _hasStored = false;
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
        GD.Print("[SovietTransportShip] 释放完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StoreCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
        DynamicVars["Block"].UpgradeValueBy(Values.BlockUpgraded);
    }
}