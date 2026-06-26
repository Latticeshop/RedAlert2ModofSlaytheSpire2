using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 提前倒矿 - 运转卡（技能卡）
/// 1费，common白卡
/// 效果：抽取摸牌堆/牌堆(升级改为牌堆=抽牌堆+弃牌堆)的所有的矿车，本回合矿车收益为80%
/// </summary>
public sealed class EarlyMining : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.EarlyMining;

    public EarlyMining() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/early_mining.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MiningMultiplier", 80) // 矿车收益百分比：80%
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 获得提前倒矿能力
        await PowerCmd.Apply<EarlyMiningPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        GD.Print("[EarlyMining] 获得提前倒矿能力");

        // 抽取矿车
        var drawPile = PileType.Draw.GetPile(Owner);
        var discardPile = PileType.Discard.GetPile(Owner);

        // 查找矿车卡牌
        var minerCards = drawPile.Cards
            .Where(c => c.GetType() == typeof(ChronoMiner))
            .ToList();

        // 升级后也从弃牌堆抽取
        if (IsUpgraded)
        {
            var discardMinerCards = discardPile.Cards
                .Where(c => c.GetType() == typeof(ChronoMiner))
                .ToList();
            minerCards.AddRange(discardMinerCards);
        }

        GD.Print($"[EarlyMining] 找到 {minerCards.Count} 张矿车卡牌");

        // 将矿车加入手牌
        foreach (var card in minerCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            GD.Print($"[EarlyMining] 将矿车加入手牌");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果：从弃牌堆也抽取矿车
        // 通过 IsUpgraded 属性判断
    }
}