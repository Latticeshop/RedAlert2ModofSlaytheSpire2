#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 超时空传送 - 盟军运转卡（超级武器）
/// 1费技能卡（升级0费），金卡，消耗
/// 效果：从摸牌/手牌/弃牌堆选任意张牌到摸牌/手牌/弃牌堆
/// </summary>
public sealed class ChronoWarp : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoWarp;
    
    public ChronoWarp() : base((int)Values.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        // 不要在构造函数中调用 AddKeyword，会在规范模型上抛出异常
    }

    public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/chroicon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.SuperWeapon.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[ChronoWarp] OnPlay 被调用");

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 第一步：选择源牌堆
        var sourcePileInt = await ChronoWarpScreen.ShowPileSelection("选择卡牌来源：");
        if (sourcePileInt == null)
        {
            GD.Print("[ChronoWarp] 取消选择");
            return;
        }

        var sourcePile = ConvertToPileType(sourcePileInt.Value);

        // 获取源牌堆的卡牌
        var cardsInSource = GetCardsInPile(sourcePile);
        if (!cardsInSource.Any())
        {
            GD.Print("[ChronoWarp] 源牌堆为空");
            return;
        }

        // 选择要移动的卡牌（可多选）
        var selectedCards = await CardSelectionScreen.ShowMultiSelection(cardsInSource, cardsInSource.Count, 1);
        if (selectedCards == null || !selectedCards.Any())
        {
            GD.Print("[ChronoWarp] 未选择任何卡牌");
            return;
        }

        // 第二步：选择目标牌堆
        var targetPileInt = await ChronoWarpScreen.ShowPileSelection("选择目标位置：");
        if (targetPileInt == null)
        {
            GD.Print("[ChronoWarp] 取消选择目标");
            return;
        }

        var targetPile = ConvertToPileType(targetPileInt.Value);

        // 移动卡牌到目标牌堆
        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, targetPile);
            GD.Print($"[ChronoWarp] 移动卡牌 {card.Id.Entry} 到 {targetPile}");
        }

        // 消耗自身
        await CardPileCmd.Add(this, PileType.Exhaust);
    }

    private PileType ConvertToPileType(int choice)
    {
        return choice switch
        {
            0 => PileType.Draw,
            1 => PileType.Hand,
            2 => PileType.Discard,
            _ => PileType.Hand
        };
    }

    private List<CardModel> GetCardsInPile(PileType pileType)
    {
        var pile = pileType.GetPile(Owner);
        return pile.Cards.ToList();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy((int)Values.CostUpgraded); // 升级后费用变为 1 + (-1) = 0
    }
}
