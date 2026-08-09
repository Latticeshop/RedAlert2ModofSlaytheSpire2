using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
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
using RedAlert2ModCode.DeckConfig;
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

		// 科技线添加超武：开启且拥有苏联作战实验室能力时，解锁铁幕装置与核弹井
		if (ModConfigManager.GetConfigForPlayer(owner)?.EnableTechSuperWeapons == true
			&& SovietCardRegistry.HasBattleLabPower(owner.Creature))
		{
			TryAddSuperWeapon<IronCurtainCard>(availableCards, owner, isUpgraded);
			TryAddSuperWeapon<NuclearMissileSiloCard>(availableCards, owner, isUpgraded);
		}

		return availableCards;
	}

	private static void TryAddSuperWeapon<T>(List<CardModel> availableCards, Player owner, bool isUpgraded)
		where T : CardModel
	{
		if (availableCards.Any(c => c is T)) return;
		var model = GetCardModel(typeof(T));
		if (model == null) return;
		var card = owner.Creature.CombatState.CreateCard(model, owner);
		if (isUpgraded && !card.IsUpgraded)
			CardCmd.Upgrade(card);
		availableCards.Add(card);
	}

	private static BuildingTechTree CreateTechTreeFromDeck(Player owner)
	{
		var techTree = SovietTechTreeConfig.CreateTechTree();
		
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
				var newCard = owner.Creature.CombatState.CreateCard(model, owner);
				
				if (isUpgraded)
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
