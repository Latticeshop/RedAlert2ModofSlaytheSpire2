using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 卖本 - 运转卡（攻击卡）
/// 1费，获得2400资金，消耗
/// 只有牌堆中有基地车卡牌时才能打出
/// </summary>
public sealed class SellMCV : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.SellMCV;

	public SellMCV() : base((int)Values.Cost, CardType.Attack, CardRarity.Uncommon, TargetType.Self) { }

	/// <summary>
	/// 卡牌图片路径（放在上层目录便于多阵营复用）
	/// </summary>
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/sellmcvicon.png";

	/// <summary>
	/// 消耗词条
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

	/// <summary>
	/// 本地化变量
	/// </summary>
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarValue", (int)Values.DollarValue)
	};

	/// <summary>
	/// 检查是否可以打出
	/// 只有牌堆（摸牌堆+手牌+弃牌堆）中有基地车卡牌时才能打出
	/// </summary>
	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查牌堆中是否有基地车卡牌
			if (!HasMcvCardInPiles())
				return false;

			return true;
		}
	}

	/// <summary>
	/// 检查牌堆（摸牌堆+手牌+弃牌堆）中是否有基地车卡牌
	/// </summary>
	private bool HasMcvCardInPiles()
	{
		if (Owner?.Creature?.CombatState == null)
			return false;

		// 检查摸牌堆
		var drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile?.Cards != null && drawPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		// 检查手牌
		var handPile = PileType.Hand.GetPile(Owner);
		if (handPile?.Cards != null && handPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		// 检查弃牌堆
		var discardPile = PileType.Discard.GetPile(Owner);
		if (discardPile?.Cards != null && discardPile.Cards.Any(c => IsMcvCard(c)))
			return true;

		return false;
	}

	/// <summary>
	/// 检查卡牌是否是基地车卡牌（盟军或苏联）
	/// </summary>
	private bool IsMcvCard(CardModel card)
	{
		if (card == null)
			return false;

		// 检查卡牌类型是否是 AlliedMCV 或 SovietMCV
		return card is AlliedMCV || card is RedAlert2ModCode.Soviet.Cards.SovietMCV;
	}

	/// <summary>
	/// 获取牌堆中所有基地车卡牌
	/// </summary>
	private List<CardModel> GetAllMcvCards()
	{
		var mcvCards = new List<CardModel>();

		// 从摸牌堆获取
		var drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile?.Cards != null)
			mcvCards.AddRange(drawPile.Cards.Where(c => IsMcvCard(c)));

		// 从手牌获取
		var handPile = PileType.Hand.GetPile(Owner);
		if (handPile?.Cards != null)
			mcvCards.AddRange(handPile.Cards.Where(c => IsMcvCard(c)));

		// 从弃牌堆获取
		var discardPile = PileType.Discard.GetPile(Owner);
		if (discardPile?.Cards != null)
			mcvCards.AddRange(discardPile.Cards.Where(c => IsMcvCard(c)));

		return mcvCards;
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 获取牌堆中所有基地车卡牌
		var mcvCards = GetAllMcvCards();
		if (mcvCards.Count == 0)
		{
			GD.PrintErr("[SellMCV] 牌堆中没有基地车卡牌，无法执行卖本");
			return;
		}

		// 移除一张基地车卡牌（优先移除手牌中的，否则移除其他堆中的）
		CardModel cardToRemove = mcvCards.FirstOrDefault(c => c.Pile == PileType.Hand.GetPile(Owner))
		                          ?? mcvCards.First();

		// 将基地车卡牌移到消耗牌堆（消耗掉，同时让玩家可查看）
		await CardPileCmd.Add(cardToRemove, PileType.Exhaust);
		GD.Print($"[SellMCV] 将基地车卡牌移到消耗牌堆: {cardToRemove.Title} (类型: {cardToRemove.GetType().Name})");

		// 获得2400资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar((int)Values.DollarValue);
			GD.Print($"[SellMCV] 获得资金 {Values.DollarValue}");
		}

		// 检查是否还有盟军基地车卡牌，如果没有则清除盟军基地车能力
		bool hasAlliedMcv = GetAllMcvCards().Any(c => c is AlliedMCV);
		if (!hasAlliedMcv)
		{
			var alliedMcvPower = Owner.Creature.Powers.OfType<AlliedMCVPower>().FirstOrDefault();
			if (alliedMcvPower != null)
			{
				await PowerCmd.Remove(alliedMcvPower);
				GD.Print("[SellMCV] 已清除盟军基地车能力");
			}
		}

		// 检查是否还有苏联基地车卡牌，如果没有则清除苏联基地车能力
		bool hasSovietMcv = GetAllMcvCards().Any(c => c is RedAlert2ModCode.Soviet.Cards.SovietMCV);
		if (!hasSovietMcv)
		{
			var sovietMcvPower = Owner.Creature.Powers.OfType<SovietMCVPower>().FirstOrDefault();
			if (sovietMcvPower != null)
			{
				await PowerCmd.Remove(sovietMcvPower);
				GD.Print("[SellMCV] 已清除苏联基地车能力");
			}
		}

		// 升级效果：将一张工程师卡牌加入手牌
		if (IsUpgraded)
		{
			var engineerCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesEngineer>(), Owner);
			await CardPileCmd.AddGeneratedCardToCombat(engineerCard, PileType.Hand, Owner);
			GD.Print("[SellMCV] 升级效果：将工程师加入手牌");
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后获得额外效果：将一张工程师卡牌加入手牌
	}
}
