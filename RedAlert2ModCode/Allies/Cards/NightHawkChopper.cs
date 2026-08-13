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
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
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
                Id = "attack",
                Title = new LocString("card_keywords", "ui.night_hawk.attack_title"),
                Description = new LocString("card_keywords", "ui.night_hawk.attack_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "deploy",
                Title = new LocString("card_keywords", "ui.night_hawk.deploy_title"),
                Description = new LocString("card_keywords", "ui.night_hawk.deploy_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(ctx, Owner, new LocString("card_keywords", "ui.night_hawk.title"), options, FactionType.Allied);

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
        // 尝试执行绝地战备攻击（消耗一层），成功则替换普通攻击
        bool desperateSuccess = await DesperateMeasures.TryExecuteDesperateMeasureAttack(Owner.Creature, play.Target, ctx);
        if (desperateSuccess)
        {
            GD.Print("[NightHawkChopper] 绝地战备攻击成功，跳过普通攻击");
            return;
        }

        int dexterity = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
        await PowerCmd.Apply<NightHawkTemporaryDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, dexterity, Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(ctx);
    }

    private async Task ExecuteDeploy(PlayerChoiceContext ctx, CardPlay play)
    {
        var soldierCards = GetSoldierCardsFromHand();

        // 手牌中没有士兵卡牌时，跳过部署选择界面，直接正常打出（避免卡死）
        if (soldierCards.Count == 0)
        {
            GD.Print("[NightHawkChopper] 手牌中没有士兵卡牌，跳过部署选择并正常打出");
            await CardPileCmd.Add(this, Keywords.Contains(CardKeyword.Exhaust) ? PileType.Exhaust : PileType.Discard, CardPilePosition.Bottom, this);
            return;
        }

        // 原版 FromHand 会触发 CancelAllCardPlay（取消回手流程），联机中选择时会阻塞其他玩家出牌；
        // 改用与超时空传送一致的 ExecuteSyncChoice + mod 选择 UI（仅暂停选择者，不取消回手）。
        var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(ctx, soldierCards, 5, 0, Owner)
            ?? new List<CardModel>();

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
            var storedText = new LocString("cards", $"{Id.Entry}.stored_info");
            storedText.Add("0", string.Join(", ", _storedCards.Select(c => c.Title)));
            ((StringVar)DynamicVars["StoredCards"]).StringValue = storedText.GetFormattedText();
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
        foreach (var soldierFunc in YuriCardRegistry.Soldiers)
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
