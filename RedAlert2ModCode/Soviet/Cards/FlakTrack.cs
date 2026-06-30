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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed partial class FlakTrack : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakTrack;

    private List<CardModel> _storedCards = new List<CardModel>();
    private bool _hasStored;

    public FlakTrack() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/htkicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new BlockVar(Values.Block, ValueProp.Unpowered),
        new IntVar("Dexterity", Values.MagicNumber),
        new StringVar("StoredCards"),
        new IntVar("StoreCount", 5)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = new List<CardKeyword>();
            if (_hasStored)
            {
                keywords.Add(CardKeyword.Exhaust);
            }
            return keywords;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Vehicle.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip()
    ];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _storedCards = new List<CardModel>(_storedCards);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        if (_hasStored)
        {
            await ReleaseStoredCards();
            return;
        }

        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "deploy",
                Title = "部署",
                Description = "存储当前手牌中的士兵单位"
            },
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "attack",
                Title = "防御",
                Description = $"获得 {DynamicVars["Dexterity"].BaseValue} 点敏捷和 {DynamicVars.Block} 点格挡"
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, "选择防空履带车的行动", options, FactionType.Soviet);

        if (selectedIndex == 0)
        {
            await ExecuteDeploy(ctx, play);
        }
        else
        {
            await ExecuteAttack(play);
        }
    }

    private async Task ExecuteAttack(CardPlay play)
    {
        await PowerCmd.Apply<SovietFlakTrackDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["Dexterity"].BaseValue, Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
    {
        var soldierCards = GetSoldierCardsFromHand();

        var selectPrompt = new LocString("cards", "FLAK_TRACK.select_prompt");
        selectPrompt.Add("0", 0);
        selectPrompt.Add("1", 5);
        var prefs = new CardSelectorPrefs(selectPrompt, 0, 5)
        {
            RequireManualConfirmation = true
        };

        var selectedCards = (await CardSelectCmd.FromHand(
            ctx,
            Owner,
            prefs,
            c => soldierCards.Contains(c),
            this
        )).ToList();

        foreach (var card in selectedCards)
        {
            NPlayerHand.Instance?.Remove(card);
            card.RemoveFromCurrentPile();
            _storedCards.Add(card);
            GD.Print($"[FlakTrack] 存储士兵卡牌: {card.Title}");
        }

        if (_storedCards.Count > 0)
        {
            _hasStored = true;
            ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
            GD.Print($"[FlakTrack] 存储完成，已存储 {_storedCards.Count} 张卡牌");
        }

        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
        GD.Print("[FlakTrack] 返回手牌");
    }

    private async Task ReleaseStoredCards()
    {
        GD.Print($"[FlakTrack] 释放存储的卡牌，数量: {_storedCards.Count}");

        foreach (var card in _storedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            GD.Print($"[FlakTrack] 释放卡牌: {card.Title}");
        }

        _storedCards.Clear();
        _hasStored = false;
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
    }

    private List<CardModel> GetSoldierCardsFromHand()
    {
        var handPile = PileType.Hand.GetPile(Owner);
        var handCards = handPile.Cards.ToList();

        var soldierTypes = new HashSet<Type>();
        foreach (var soldierFunc in AlliedCardRegistry.Soldiers)
        {
            var card = soldierFunc();
            soldierTypes.Add(card.GetType());
        }
        foreach (var soldierFunc in SovietCardRegistry.Soldiers)
        {
            var card = soldierFunc();
            soldierTypes.Add(card.GetType());
        }

        return handCards.Where(c => c != this && soldierTypes.Contains(c.GetType())).ToList();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Dexterity"].UpgradeValueBy(Values.MagicNumberUpgraded);
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}
