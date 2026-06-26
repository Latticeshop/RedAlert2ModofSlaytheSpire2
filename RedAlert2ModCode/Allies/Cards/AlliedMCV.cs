using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军基地车 - 技能卡
/// 0费，打出后在初始建筑+当前卡组已有建筑中选择一张加入手牌
/// 升级后：获得的卡牌为升级版本
/// </summary>
public sealed class AlliedMCV : CardModel
{
	public AlliedMCV() : base((int)AlliesCardValues.AlliedMCV.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

	// 修正图片路径为实际文件名 mcvicon.png
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";

	/// <summary>
	/// 固有词条 - 每场战斗开始时自动出现在手牌
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Innate };

	/// <summary>
	/// 额外的悬停提示（包含MCV词条和战车词条）
	/// </summary>
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Mcv.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	/// <summary>
	/// 初始建筑类型列表（发电厂、矿场、兵营）
	/// </summary>
	private static readonly List<System.Type> InitialBuildingTypes = new()
	{
		typeof(PowerPlantCard),
		typeof(AlliedRefinery),
		typeof(AlliesBarracksCard)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		// 使用 CombatState.CreateCard 创建正确初始化的卡牌副本
		List<CardModel> availableCards = new();

		// 1. 添加初始建筑
		AddInitialBuildings(availableCards);

		// 2. 添加卡组中已有的建筑卡牌（利用 AlliesCardValues 中的映射）
		AddDeckBuildings(availableCards);

		// 去重：按卡牌ID去重，保留第一个（初始建筑优先）
		var buildingValuesMap = AlliesCardValues.CreateBuildingValuesMap();
		availableCards = availableCards
			.GroupBy(c => c.Id.Entry.ToUpper().Replace("_", ""))
			.Select(g => g.First())
			.ToList();

		GD.Print($"[AlliedMCV] 可用建筑卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		// 传递数值映射，让UI面板能够正确显示费用和描述
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards, buildingValuesMap);

		// 如果玩家选择了卡牌，执行能力效果
		if (selectedCard != null)
		{
			// 应用基地车能力（用于显示图标）
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<AlliedMCVPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);

			// 将选择的卡牌加入手牌
			await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);

			GD.Print("[AlliedMCV] 玩家选择了建筑，基地车将正常进入弃牌堆");
		}
		else
		{
			// 取消选择：将实际打出的卡牌实体放回手牌（相当于未打出）
			GD.Print("[AlliedMCV] 玩家取消选择，基地车放回手牌");

			if (play?.Card != null)
			{
				var card = play.Card;
				var handPile = PileType.Hand.GetPile(card.Owner);

				if (card.Pile != null)
				{
					card.RemoveFromCurrentPile();
				}

				await CardPileCmd.Add(card, handPile);
				handPile.InvokeContentsChanged();
				GD.Print("[AlliedMCV] 基地车已放回手牌");
			}
		}
	}

	/// <summary>
	/// 添加初始建筑卡牌到选择列表
	/// </summary>
	private void AddInitialBuildings(List<CardModel> availableCards)
	{
		foreach (var buildingType in InitialBuildingTypes)
		{
			var model = GetCardModel(buildingType);
			if (model != null)
			{
				var card = Owner.Creature.CombatState.CreateCard(model, Owner);
				
				// 如果基地车是升级过的，创建的卡牌也显示为升级版本
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(card);
				}

				availableCards.Add(card);
			}
		}
	}

	/// <summary>
	/// 添加卡组中已有的建筑卡牌到选择列表（利用 AlliesCardValues 中的映射）
	/// </summary>
	private void AddDeckBuildings(List<CardModel> availableCards)
	{
		if (Owner?.Deck?.Cards == null)
		{
			return;
		}

		// 获取所有建筑卡牌的ID集合（来自 AlliesCardValues，避免重复定义）
		var buildingIds = AlliesCardValues.CreateBuildingValuesMap().Keys.ToHashSet();

		// 获取卡组中的建筑卡牌（按ID去重）
		var deckBuildingCards = Owner.Deck.Cards
			.Where(c => 
			{
				string cardId = c.Id.Entry.ToUpper().Replace("_", "");
				return buildingIds.Contains(cardId);
			})
			.GroupBy(c => c.Id.Entry.ToUpper().Replace("_", ""))
			.Select(g => g.First());

		foreach (var deckCard in deckBuildingCards)
		{
			// 使用反射调用 ModelDb.Card<T>() 获取卡牌模型
			var model = GetCardModel(deckCard.GetType());
			if (model == null)
			{
				continue;
			}

			// 创建新的卡牌实例
			var newCard = Owner.Creature.CombatState.CreateCard(model, Owner);

			// 保持原卡牌的升级状态
			if (deckCard.IsUpgraded)
			{
				CardCmd.Upgrade(newCard);
			}

			availableCards.Add(newCard);
			GD.Print($"[AlliedMCV] 添加卡组中的建筑: {deckCard.Id.Entry} (升级:{deckCard.IsUpgraded})");
		}

		// 如果牌库有飞鹰战备系列卡牌，添加空指部
		if (HasEagleCardInDeck())
		{
			AddAirForceCommand(availableCards);
		}
	}

	/// <summary>
	/// 检查牌库中是否有飞鹰战备系列卡牌
	/// </summary>
	private bool HasEagleCardInDeck()
	{
		return DesperateMeasures.HasDesperateMeasureCardInDeck(this);
	}

	/// <summary>
	/// 添加空指部到可用建筑列表
	/// </summary>
	private void AddAirForceCommand(List<CardModel> availableCards)
	{
		// 检查是否已有空指部
		if (availableCards.Any(c => c is AirForceCommand))
		{
			return;
		}

		var model = GetCardModel(typeof(AirForceCommand));
		if (model == null)
		{
			GD.PrintErr("[AlliedMCV] 无法获取空指部模型");
			return;
		}

		var newCard = Owner.Creature.CombatState.CreateCard(model, Owner);

		// 如果基地车是升级过的，空指部也显示为升级版本
		if (base.IsUpgraded)
		{
			CardCmd.Upgrade(newCard);
		}

		availableCards.Add(newCard);
		GD.Print($"[AlliedMCV] 牌库有飞鹰战备卡牌，添加空指部到选择列表 (升级:{base.IsUpgraded})");
	}

	/// <summary>
	/// 根据卡牌类型获取 CardModel（使用存储类中的预定义映射避免反射错误）
	/// </summary>
	private static CardModel? GetCardModel(System.Type cardType)
	{
		try
		{
			// 首先尝试从存储类的预定义映射中获取
			if (AlliesCardValues.BuildingModelMap.TryGetValue(cardType, out var modelFunc))
			{
				return modelFunc();
			}

			// 对于不在映射中的类型，尝试反射（用于卡组中可能存在的其他建筑）
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
			GD.PrintErr($"[AlliedMCV] 获取卡牌模型失败: {ex.Message}");
			return null;
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后：获得的卡牌为升级版本（费用不变，仍为0费）
	}
}
