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
using RedAlert2ModCode.Allies.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Allies.Cards;

public sealed partial class NightHawkChopper : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.NightHawkChopper;

    private List<CardModel> _storedCards = new List<CardModel>();
    private bool _hasStored;

    public NightHawkChopper() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/shadicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
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
        ModCardKeywords.TechLevelT2.CreateHoverTip(),
        ModCardKeywords.Aircraft.CreateHoverTip(),
        ModCardKeywords.Soldier.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip()
    ];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _storedCards = new List<CardModel>(_storedCards);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

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
                Title = new LocString("card_keywords", "ui.night_hawk.deploy_title"),
                Description = new LocString("card_keywords", "ui.night_hawk.deploy_desc")
            },
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "attack",
                Title = new LocString("card_keywords", "ui.night_hawk.attack_title"),
                Description = new LocString("card_keywords", "ui.night_hawk.attack_desc")
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.night_hawk.title"), options, FactionType.Allied);

        if (selectedIndex == 0)
        {
            await ExecuteDeploy(ctx, play);
        }
        else
        {
            await ExecuteAttack(ctx, play);
        }
    }

    private async Task ExecuteAttack(PlayerChoiceContext ctx, CardPlay play)
    {
        int dexterity = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
        await PowerCmd.Apply<NightHawkTemporaryDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, dexterity, Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(ctx);
    }

    private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
    {
        var soldierCards = GetSoldierCardsFromHand();

        var selectPrompt = new LocString("cards", "NIGHT_HAWK_CHOPPER.select_prompt");
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
            _storedCards.Add(card);
            GD.Print($"[NightHawkChopper] 存储士兵卡牌: {card.Title}");
        }

        foreach (var card in _storedCards)
        {
            await CardPileCmd.RemoveFromCombat(card);
        }

        if (_storedCards.Count > 0)
        {
            _hasStored = true;
            ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Join(", ", _storedCards.Select(c => c.Title));
            GD.Print($"[NightHawkChopper] 存储完成，已存储 {_storedCards.Count} 张卡牌");
        }

        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, this);
        GD.Print("[NightHawkChopper] 返回手牌");
    }

    private async Task ReleaseStoredCards()
    {
        GD.Print($"[NightHawkChopper] 释放存储的卡牌，数量: {_storedCards.Count}");

        foreach (var card in _storedCards)
        {
            card.HasBeenRemovedFromState = false;
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            GD.Print($"[NightHawkChopper] 释放卡牌: {card.Title}");
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
    }
}
