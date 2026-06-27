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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Soviet.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军基地车 - 技能卡
/// 0费，打出后在初始建筑+当前卡组已有建筑中选择一张加入手牌
/// 升级后：获得的卡牌为升级版本
/// </summary>
public sealed class SovietMCV : CardModel
{
	public SovietMCV() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";

	/// <summary>
	/// 固有词条 - 每场战斗开始时自动出现在手牌
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Innate };

	/// <summary>
	/// 额外的悬停提示（包含MCV词条、战车词条和建筑科技线词条）
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
		var unlockedBuildings = techTree.GetUnlockedBuildingTypes();

		foreach (var buildingType in unlockedBuildings)
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
				GD.Print($"[SovietMCV] 科技线解锁建筑: {buildingType.Name}");
			}
		}

		AddDeckBuildings(ref availableCards);

		GD.Print($"[SovietMCV] 可用建筑卡牌数量: {availableCards.Count} (当前科技等级: {techTree.CurrentTechLevel})");

		var buildingValuesMap = SovietCardValues.CreateBuildingValuesMap();
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards, buildingValuesMap, FactionType.Soviet);

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
			// 取消选择：将实际打出的卡牌实体放回手牌（相当于未打出）
			GD.Print("[SovietMCV] 玩家取消选择，基地车放回手牌");

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
				GD.Print("[SovietMCV] 基地车已放回手牌");
			}
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

	private void AddDeckBuildings(ref List<CardModel> availableCards)
	{
		if (Owner?.Deck?.Cards == null)
		{
			return;
		}

		var techTree = SovietTechTreeConfig.CreateTechTree();
		var techTreeBuildingTypes = techTree.GetUnlockedBuildingTypes();

		foreach (var card in Owner.Deck.Cards)
		{
			var cardType = card.GetType();
			
			if (!techTreeBuildingTypes.Contains(cardType) && IsBuildingCardType(cardType))
			{
				var model = GetCardModel(cardType);
				if (model != null)
				{
					var newCard = Owner.Creature.CombatState.CreateCard(model, Owner);
					
					if (base.IsUpgraded)
					{
						CardCmd.Upgrade(newCard);
					}

					if (!availableCards.Any(c => c.GetType() == cardType))
					{
						availableCards.Add(newCard);
						GD.Print($"[SovietMCV] 添加牌库建筑: {cardType.Name}");
					}
				}
			}
		}
	}

	private bool IsBuildingCardType(System.Type cardType)
	{
		var typeName = cardType.Name;
		return typeName.Contains("Repair") || typeName.Contains("Defense") || typeName.Contains("Bunker") || typeName.Contains("Wall");
	}

	/// <summary>
	/// 根据卡牌类型获取 CardModel（使用存储类中的预定义映射避免反射错误）
	/// </summary>
	private static CardModel? GetCardModel(System.Type cardType)
	{
		try
		{
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
			GD.PrintErr($"[SovietMCV] 获取卡牌模型失败: {ex.Message}");
			return null;
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后：获得的卡牌为升级版本（费用不变，仍为0费）
	}
}