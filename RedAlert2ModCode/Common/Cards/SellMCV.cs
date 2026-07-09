using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class SellMCV : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.SellMCV;

	public SellMCV() : base((int)Values.Cost, CardType.Attack, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/sellmcvicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<SovietMCV>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarValue", (int)Values.DollarValue)
	};

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// if (!HasMcvCardInPiles())
			// 	return false;

			if (!HasMcvPower())
				return false;

			return true;
		}
	}

	private bool HasMcvPower()
	{
		if (Owner?.Creature == null)
			return false;

		return Owner.Creature.Powers.Any(p => p is AlliedMCVPower || p is SovietMCVPower);
	}

	private bool HasMcvCardInPiles()
	{
		if (Owner?.Creature?.CombatState == null)
			return false;

		var drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile?.Cards != null && drawPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		var handPile = PileType.Hand.GetPile(Owner);
		if (handPile?.Cards != null && handPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		var discardPile = PileType.Discard.GetPile(Owner);
		if (discardPile?.Cards != null && discardPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		return false;
	}

	private bool IsMcvCard(CardModel card)
	{
		if (card == null)
			return false;

		return card is AlliedMCV || card is SovietMCV;
	}

	private List<CardModel> GetAllMcvCards()
	{
		var mcvCards = new List<CardModel>();

		var drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile?.Cards != null)
			mcvCards.AddRange(drawPile.Cards.Where(c => IsMcvCard(c)));

		var handPile = PileType.Hand.GetPile(Owner);
		if (handPile?.Cards != null)
			mcvCards.AddRange(handPile.Cards.Where(c => IsMcvCard(c)));

		var discardPile = PileType.Discard.GetPile(Owner);
		if (discardPile?.Cards != null)
			mcvCards.AddRange(discardPile.Cards.Where(c => IsMcvCard(c)));

		return mcvCards;
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// var mcvCards = GetAllMcvCards();
		// if (mcvCards.Count == 0)
		// {
		// 	GD.PrintErr("[SellMCV] 牌堆中没有基地车卡牌");
		// 	return;
		// }
		//
		// CardModel cardToRemove = mcvCards.FirstOrDefault(c => c.Pile == PileType.Hand.GetPile(Owner))
		//                           ?? mcvCards.First();
		//
		// await CardPileCmd.Add(cardToRemove, PileType.Exhaust);
		// GD.Print($"[SellMCV] 将基地车卡牌移到消耗牌堆: {cardToRemove.Title}");

		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar((int)Values.DollarValue);
			GD.Print($"[SellMCV] 获得资金 {Values.DollarValue}");
		}

		var alliedMcvPower = Owner.Creature.Powers.OfType<AlliedMCVPower>().FirstOrDefault();
		if (alliedMcvPower != null)
		{
			if (alliedMcvPower.Amount > 1)
			{
				await PowerCmd.Apply<AlliedMCVPower>(ctx, Owner.Creature, -1, Owner.Creature, this);
				GD.Print($"[SellMCV] 盟军基地车能力层数减少: {alliedMcvPower.Amount - 1}");
			}
			else
			{
				await PowerCmd.Remove(alliedMcvPower);
				GD.Print("[SellMCV] 已清除盟军基地车能力");
			}
			BuildingSoundHelper.PlayBuildingSellSound();
		}

		var sovietMcvPower = Owner.Creature.Powers.OfType<SovietMCVPower>().FirstOrDefault();
		if (sovietMcvPower != null)
		{
			if (sovietMcvPower.Amount > 1)
			{
				await PowerCmd.Apply<SovietMCVPower>(ctx, Owner.Creature, -1, Owner.Creature, this);
				GD.Print($"[SellMCV] 苏联基地车能力层数减少: {sovietMcvPower.Amount - 1}");
			}
			else
			{
				await PowerCmd.Remove(sovietMcvPower);
				GD.Print("[SellMCV] 已清除苏联基地车能力");
			}
			BuildingSoundHelper.PlayBuildingSellSound();
		}

		if (IsUpgraded)
		{
			CardModel engineerCard;
			if (Owner.Character.GetType().Name.Contains("Allies"))
			{
				engineerCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesEngineer>(), Owner);
			}
			else
			{
				engineerCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietEngineer>(), Owner);
			}
			await CardPileCmd.AddGeneratedCardToCombat(engineerCard, PileType.Hand, Owner);
			GD.Print("[SellMCV] 升级效果：将工程师加入手牌");
		}
	}

	protected override void OnUpgrade()
	{
	}
}