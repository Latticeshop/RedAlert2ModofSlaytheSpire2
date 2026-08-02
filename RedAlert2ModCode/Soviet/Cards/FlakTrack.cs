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
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed partial class FlakTrack : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakTrack;

    private List<CardModel> _storedCards = new List<CardModel>();
    private bool _hasStored;

    public FlakTrack() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/htkicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DrawCount", Values.MagicNumber),
        new IntVar("DiscardCount", Values.Stars),
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
        ModCardKeywords.TechLevelT1.CreateHoverTip(),
        ModCardKeywords.Vehicle.CreateHoverTip(),
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
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        if (_hasStored)
        {
            await ReleaseStoredCards();
            return;
        }

        UnitVoiceHelper.PlaySovietAaSfx();

        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "attack",
                Title = new LocString("card_keywords", "ui.flak_track.defend_title"),
                Description = new LocString("card_keywords", "ui.flak_track.defend_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "deploy",
                Title = new LocString("card_keywords", "ui.flak_track.deploy_title"),
                Description = new LocString("card_keywords", "ui.flak_track.deploy_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.flak_track.title"), options, FactionType.Soviet);

        if (selectedIndex == 0)
        {
            await ExecuteAttack(ctx, play);
        }
        else
        {
            await ExecuteDeploy(ctx, play);
        }
    }

    private async Task ExecuteAttack(PlayerChoiceContext ctx, CardPlay play)
    {
        // 抽牌
        await CardPileCmd.Draw(ctx, (int)DynamicVars["DrawCount"].BaseValue, Owner);
        
        // 弃牌选择：参考苏联维修厂的原版UI
        int maxDiscard = (int)DynamicVars["DiscardCount"].BaseValue;
        var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_FLAK_TRACK.discard_prompt");
        selectPrompt.Add("0", 0);
        selectPrompt.Add("1", maxDiscard);
        var prefs = new CardSelectorPrefs(selectPrompt, 0, maxDiscard)
        {
            RequireManualConfirmation = true
        };

        var selectedCards = (await CardSelectCmd.FromHand(
            ctx,
            Owner,
            prefs,
            c => c != this,
            this
        )).ToList();

        // 弃掉选中的牌
        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Discard);
            GD.Print($"[FlakTrack] 弃牌: {card.Title}");
        }
    }

    private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
    {
        var soldierCards = GetSoldierCardsFromHand();

        var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_FLAK_TRACK.select_prompt");
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

        // 定时炸弹检测：若选中的卡牌中有带定时炸弹的，转化为自爆步兵车
        var timedBombCard = selectedCards.FirstOrDefault(c =>
            TimedBombManager.HasTimedBombEffect(c)
            || c is TerrorMan or CrazyIvanCard or ChronoIvanCard);

        if (timedBombCard != null)
        {
            GD.Print($"[FlakTrack] 检测到定时炸弹卡牌: {timedBombCard.Title}，转化为自爆步兵车");
            await VehicleDeployHelper.DeploySpecialVehicle<DemoVehicle>(ctx, this, timedBombCard, Owner);
            return;
        }

        foreach (var card in selectedCards)
        {
            _storedCards.Add(card);
            GD.Print($"[FlakTrack] 存储士兵卡牌: {card.Title}");
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
            card.HasBeenRemovedFromState = false;
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
        foreach (var soldierFunc in YuriCardRegistry.Soldiers)
        {
            var card = soldierFunc();
            soldierTypes.Add(card.GetType());
        }

        return handCards.Where(c => c != this && soldierTypes.Contains(c.GetType())).ToList();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
        DynamicVars["DiscardCount"].UpgradeValueBy(Values.StarsUpgraded);
    }
}
