using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 集结 - 运转卡（技能卡）
/// 1费，从牌堆中召集2张单位卡到手牌中，升级后3张
/// 先从抽牌堆抽取，若不足则从弃牌堆找补
/// </summary>
public sealed class Ra2Rally : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Ra2Rally;

	public Ra2Rally() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	/// <summary>
	/// 卡牌图片路径
	/// </summary>
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/rallyicon.png";

	/// <summary>
	/// 本地化变量 - 使用 MagicNumber 让游戏引擎自动处理升级数值
	/// </summary>
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	/// <summary>
	/// 额外的悬停提示（展示单位词条）
	/// </summary>
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Unit.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 根据是否升级决定召集数量
		int cardsToCall = IsUpgraded 
			? (int)Values.MagicNumber + (int)Values.MagicNumberUpgraded 
			: (int)Values.MagicNumber;
		int cardsCalled = 0;

		GD.Print($"[Ra2Rally] 开始召集 {cardsToCall} 张单位卡");

		// 获取所有单位卡牌类型
		var unitTypes = new List<System.Type>
		{
			typeof(AmericanSoldier),
			typeof(DogSoldier),
			typeof(RocketSoldier),
			typeof(Engineer),
			typeof(GrizzlyTank),
			typeof(Ifv),
			typeof(ChronoMiner),
			typeof(Intruder)
		};

		// 获取抽牌堆和弃牌堆
		var drawPile = PileType.Draw.GetPile(Owner);
		var discardPile = PileType.Discard.GetPile(Owner);

		// 1. 先从抽牌堆找单位卡
		var drawPileUnits = drawPile.Cards
			.Where(c => unitTypes.Contains(c.GetType()))
			.ToList();

		GD.Print($"[Ra2Rally] 抽牌堆中有 {drawPileUnits.Count} 张单位卡");

		foreach (var card in drawPileUnits)
		{
			if (cardsCalled >= cardsToCall) break;
			await CardPileCmd.Add(card, PileType.Hand);
			cardsCalled++;
			GD.Print($"[Ra2Rally] 从抽牌堆找到单位卡: {card.Id.Entry}");
		}

		// 2. 若不足，从弃牌堆找补
		if (cardsCalled < cardsToCall)
		{
			var discardPileUnits = discardPile.Cards
				.Where(c => unitTypes.Contains(c.GetType()))
				.ToList();

			GD.Print($"[Ra2Rally] 弃牌堆中有 {discardPileUnits.Count} 张单位卡");

			foreach (var card in discardPileUnits)
			{
				if (cardsCalled >= cardsToCall) break;
				await CardPileCmd.Add(card, PileType.Hand);
				cardsCalled++;
				GD.Print($"[Ra2Rally] 从弃牌堆找到单位卡: {card.Id.Entry}");
			}
		}

		GD.Print($"[Ra2Rally] 成功召集 {cardsCalled} 张单位卡");
	}

	protected override void OnUpgrade()
	{
		// 升级效果由 MagicNumberUpgraded 处理
	}
}
