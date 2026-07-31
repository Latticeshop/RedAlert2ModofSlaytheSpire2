using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Soviet.Utils;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军基地车 - 技能卡
/// 0费，打出后在初始建筑+当前卡组已有建筑中选择一张加入手牌
/// 升级后：获得的卡牌为升级版本
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietMCV : CardModel, ICancellableCardPlay
{
	public SovietMCV() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";

	/// <summary>
	/// 固有词条 - 每场战斗开始时自动出现在手牌
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Innate };

	/// <summary>
	/// 额外的悬停提示（包含MCV词条、装甲词条和建筑科技线词条）
	/// </summary>
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Mcv.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.BuildingTechTree.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		List<CardModel> availableCards = new();

		var techTree = CreateTechTreeFromDeck();
		var unlockedCoreBuildings = techTree.GetUnlockedCoreBuildingTypes();

		foreach (var buildingType in unlockedCoreBuildings)
		{
			var model = GetCardModel(buildingType);
			if (model != null)
			{
				var card = Owner.Creature.CombatState.CreateCard(model, Owner);
				
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(card);
				}

				availableCards.Add(card);
				GD.Print($"[SovietMCV] 核心建筑解锁: {buildingType.Name}");
			}
		}

		AddDeckBuildings(ref availableCards, techTree.CurrentTechLevel);

		GD.Print($"[SovietMCV] 可用建筑卡牌数量: {availableCards.Count} (当前科技等级: {techTree.CurrentTechLevel})");

		var buildingValuesMap = SovietCardValues.CreateBuildingValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, buildingValuesMap, FactionType.Soviet);

		// 如果玩家选择了卡牌，执行能力效果
		if (selectedCard != null)
		{
			// 应用基地车能力（用于显示图标）
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<SovietMCVPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);

			// 将选择的卡牌加入手牌
			await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);

			GD.Print("[SovietMCV] 玩家选择了建筑，基地车将正常进入弃牌堆");
		}
		else
		{
			GD.Print("[SovietMCV] 玩家取消选择，基地车放回手牌");
			await CardUtils.HandleCardCancellation(play, this, Owner);
		}
	}

	private BuildingTechTree CreateTechTreeFromDeck()
	{
		var techTree = SovietTechTreeConfig.CreateTechTree();
		
		if (Owner?.Creature?.Powers == null)
		{
			return techTree;
		}

		techTree.UnlockTechFromPowers(Owner.Creature.Powers);

		return techTree;
	}

	private void AddDeckBuildings(ref List<CardModel> availableCards, TechLevel currentTechLevel)
	{
		if (Owner?.Deck?.Cards == null)
		{
			return;
		}

		foreach (var card in Owner.Deck.Cards)
		{
			var cardType = card.GetType();
			
			if (!BuildingCardUtils.IsDeckBuildingCard(cardType))
				continue;
			
			// 阵营过滤：苏军MCV只能造苏军建筑
			if (!BuildingCardUtils.IsDeckBuildingOfFaction(cardType, FactionType.Soviet))
				continue;
			
			var requiredLevel = BuildingCardUtils.GetDeckBuildingTechLevel(cardType);
			if (requiredLevel == null || currentTechLevel < requiredLevel.Value)
				continue;
			
			if (availableCards.Any(c => c.GetType() == cardType))
				continue;
			
			var model = GetCardModel(cardType);
			if (model != null)
			{
				var newCard = Owner.Creature.CombatState.CreateCard(model, Owner);
				
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(newCard);
				}

				availableCards.Add(newCard);
				GD.Print($"[SovietMCV] 添加牌组建筑: {cardType.Name}");
			}
		}
	}

	bool IsBuildingCardType(System.Type cardType)
	{
		return BuildingCardUtils.IsBuildingCard(cardType);
	}

	/// <summary>
	/// 根据卡牌类型获取 CardModel（使用存储类中的预定义映射避免反射错误）
	/// </summary>
	private static CardModel? GetCardModel(System.Type cardType)
	{
		try
		{
			if (SovietCardValues.BuildingModelMap.TryGetValue(cardType, out var modelFunc))
			{
				return modelFunc();
			}

			var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes);
			if (method == null)
			{
				return null;
			}

			var genericMethod = method.MakeGenericMethod(cardType);
			return genericMethod.Invoke(null, null) as CardModel;
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[SovietMCV] 获取卡牌模型失败: {ex.Message}");
			return null;
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后：获得的卡牌为升级版本（费用不变，仍为0费）
	}
}