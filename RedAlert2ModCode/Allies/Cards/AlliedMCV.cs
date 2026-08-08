using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
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
using RedAlert2ModCode.Allies.Utils;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军基地车 - 技能卡
/// 0费，打出后在初始建筑+当前卡组已有建筑中选择一张加入手牌
/// 升级后：获得的卡牌为升级版本
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliedMCV : CardModel, ICancellableCardPlay
{
	public AlliedMCV() : base((int)AlliesCardValues.AlliedMCV.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

	// 修正图片路径为实际文件名 mcvicon.png
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";

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
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		// 注：A2 预选模式下，选择在打出前完成；扣费/基地车能力/建筑入队由 BuildingResolutionAction 结算。
		// 自动打出兜底：若没有手动 A2 的待结算标记，则本地补开预选面板（确认后由结算动作执行效果）
		if (BuildingPrePlayHelper.TryConsumePendingResolution(this))
			return;
		if (MultiplayerSyncHelper.IsLocalPlayer(Owner))
			BuildingPrePlayHelper.OpenAutoPlayPanel(this);
	}
	/// <summary>
	/// A2 预选面板候选：与结算动作共用同一套确定性候选构建。
	/// </summary>
	public static List<CardModel> GetPrePlayCandidates(Player owner, bool isUpgraded)
	{
		List<CardModel> availableCards = new();

		var techTree = CreateTechTreeFromDeck(owner);
		var unlockedCoreBuildings = techTree.GetUnlockedCoreBuildingTypes();

		foreach (var buildingType in unlockedCoreBuildings)
		{
			var model = GetCardModel(buildingType);
			if (model != null)
			{
				var card = owner.Creature.CombatState.CreateCard(model, owner);
				if (isUpgraded)
					CardCmd.Upgrade(card);
				availableCards.Add(card);
			}
		}

		AddDeckBuildings(owner, isUpgraded, ref availableCards, techTree.CurrentTechLevel);

		// 巨炮（法国专属）：拥有法国国旗且有空指部/雷达/作战实验室能力时添加
		if (FlagManager.HasFrance(owner) && AlliedCardRegistry.HasAirForceCommandPower(owner.Creature))
		{
			if (!availableCards.Any(c => c is GrandCannon))
			{
				var grandCannonModel = GetCardModel(typeof(GrandCannon));
				if (grandCannonModel != null)
				{
					var grandCannonCard = owner.Creature.CombatState.CreateCard(grandCannonModel, owner);
					if (isUpgraded)
						CardCmd.Upgrade(grandCannonCard);
					availableCards.Add(grandCannonCard);
				}
			}
		}
		else
		{
			availableCards = availableCards.Where(c => c is not GrandCannon).ToList();
		}

		return availableCards;
	}

	private static BuildingTechTree CreateTechTreeFromDeck(Player owner)
	{
		var techTree = AlliedTechTreeConfig.CreateTechTree();
		
		if (owner?.Creature?.Powers == null)
		{
			return techTree;
		}

		techTree.UnlockTechFromPowers(owner.Creature.Powers);

		return techTree;
	}

	private static void AddDeckBuildings(Player owner, bool isUpgraded, ref List<CardModel> availableCards, TechLevel currentTechLevel)
	{
		if (owner?.Deck?.Cards == null)
		{
			return;
		}

		foreach (var card in owner.Deck.Cards)
		{
			var cardType = card.GetType();
			
			if (!BuildingCardUtils.IsDeckBuildingCard(cardType))
				continue;
			
			// 阵营过滤：盟军MCV只能造盟军建筑
			if (!BuildingCardUtils.IsDeckBuildingOfFaction(cardType, FactionType.Allied))
				continue;
			
			var requiredLevel = BuildingCardUtils.GetDeckBuildingTechLevel(cardType);
			if (requiredLevel == null || currentTechLevel < requiredLevel.Value)
				continue;
			
			if (availableCards.Any(c => c.GetType() == cardType))
				continue;
			
			var model = GetCardModel(cardType);
			if (model != null)
			{
				var newCard = owner.Creature.CombatState.CreateCard(model, owner);
				
				if (isUpgraded)
				{
					CardCmd.Upgrade(newCard);
				}

				availableCards.Add(newCard);
				GD.Print($"[AlliedMCV] 添加牌组建筑: {cardType.Name}");
			}
		}
	}

	private bool IsBuildingCardType(System.Type cardType)
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
