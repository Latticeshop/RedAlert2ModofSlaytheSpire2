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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietEarlyMining : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.EarlyMining;

    public SovietEarlyMining() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/early_mining_soviet.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipHelper.FromCardWithUpgrade<WarMiner>(() => IsUpgraded),
		ModCardKeywords.Miner.CreateHoverTip()
	];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MiningMultiplier", 80)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<SovietEarlyMiningPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        GD.Print("[SovietEarlyMining] 获得提前倒矿能力");

        var drawPile = PileType.Draw.GetPile(Owner);
        var discardPile = PileType.Discard.GetPile(Owner);

        var minerCards = drawPile.Cards
            .Where(c => c.GetType() == typeof(WarMiner) || c.GetType() == typeof(ChronoMiner))
            .ToList();

        if (IsUpgraded)
        {
            var discardMinerCards = discardPile.Cards
                .Where(c => c.GetType() == typeof(WarMiner) || c.GetType() == typeof(ChronoMiner))
                .ToList();
            minerCards.AddRange(discardMinerCards);
        }

        GD.Print($"[SovietEarlyMining] 找到 {minerCards.Count} 张矿车卡牌");

        foreach (var card in minerCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            GD.Print($"[SovietEarlyMining] 将矿车加入手牌");
        }
    }

    protected override void OnUpgrade()
    {
    }
}