using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class MineRaid : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.MineRaid;

	public MineRaid() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/mine_raid.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Miner.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		int cardsToDraw = IsUpgraded 
			? (int)Values.MagicNumber + (int)Values.MagicNumberUpgraded 
			: (int)Values.MagicNumber;
		int cardsDrawn = 0;

		GD.Print($"[MineRaid] 开始抽取 {cardsToDraw} 张矿车卡");

		var drawPile = PileType.Draw.GetPile(Owner);
		var discardPile = PileType.Discard.GetPile(Owner);

		var discardPileMiners = discardPile.Cards
			.Where(c => IsMinerCard(c))
			.ToList();

		GD.Print($"[MineRaid] 弃牌堆中有 {discardPileMiners.Count} 张矿车卡");

		foreach (var card in discardPileMiners)
		{
			if (cardsDrawn >= cardsToDraw) break;
			await CardPileCmd.Add(card, PileType.Hand);
			cardsDrawn++;
			GD.Print($"[MineRaid] 从弃牌堆找到矿车卡: {card.Id.Entry}");
		}

		if (cardsDrawn < cardsToDraw)
		{
			var drawPileMiners = drawPile.Cards
				.Where(c => IsMinerCard(c))
				.ToList();

			GD.Print($"[MineRaid] 抽牌堆中有 {drawPileMiners.Count} 张矿车卡");

			foreach (var card in drawPileMiners)
			{
				if (cardsDrawn >= cardsToDraw) break;
				await CardPileCmd.Add(card, PileType.Hand);
				cardsDrawn++;
				GD.Print($"[MineRaid] 从抽牌堆找到矿车卡: {card.Id.Entry}");
			}
		}

		GD.Print($"[MineRaid] 成功抽取 {cardsDrawn} 张矿车卡");
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}

	private bool IsMinerCard(CardModel card)
	{
		return card is RedAlert2ModCode.Allies.Cards.ChronoMiner || card is RedAlert2ModCode.Soviet.Cards.WarMiner;
	}
}