using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Yuri;

namespace RedAlert2ModCode.Common.Utils;

public static class CardUtils
{
	/// <summary>
	/// 跟踪被取消的卡牌打出操作（按 CardPlay 实例）。
	/// 选择面板类建筑卡（重工、兵营、MCV 等）在玩家取消选择时会统一走 HandleCardCancellation，
	/// 在此处标记后，供 UrbanizationPower.AfterCardPlayed 判断是否应跳过城市化抽牌。
	/// 使用 ConditionalWeakTable 按 CardPlay 实例附加状态，CardPlay 被回收后条目自动清除，无内存泄漏。
	/// </summary>
	private static readonly ConditionalWeakTable<CardPlay, object> _cancelledCardPlays = new();

	/// <summary>
	/// 标记某次卡牌打出操作已被取消（仅在 HandleCardCancellation 中调用）。
	/// </summary>
	public static void MarkCardPlayCancelled(CardPlay play)
	{
		if (play == null) return;
		_cancelledCardPlays.Remove(play);
		_cancelledCardPlays.Add(play, new object());
	}

	/// <summary>
	/// 判断某次卡牌打出操作是否已被取消。
	/// </summary>
	public static bool WasCardPlayCancelled(CardPlay play)
	{
		return play != null && _cancelledCardPlays.TryGetValue(play, out _);
	}

	public static int GetCardEnergyCost(CardModel card)
	{
		var cost = 0;
		
		if (card.EnergyCost != null)
		{
			try
			{
				var resolvedCost = card.EnergyCost.GetResolved();
				cost = (int)resolvedCost;
				GD.Print($"[CardUtils] 使用 GetResolved() 获取费用: {cost}");
			}
			catch
			{
				try
				{
					var canonicalCost = card.EnergyCost.Canonical;
					cost = (int)canonicalCost;
					GD.Print($"[CardUtils] 使用 Canonical 获取费用: {cost}");
				}
				catch
				{
					cost = GetEnergyCostValue(card.EnergyCost);
					GD.Print($"[CardUtils] 使用反射获取费用: {cost}");
				}
			}
		}
		
		return cost;
	}
	
	public static int GetEnergyCostValue(object energyCost)
	{
		var cost = 0;
		var costType = energyCost.GetType();
		
		var valueProp = costType.GetProperty("Value");
		if (valueProp != null)
		{
			var value = valueProp.GetValue(energyCost);
			if (value != null)
			{
				cost = Convert.ToInt32(value);
			}
		}
		
		if (cost == 0)
		{
			var intValueProp = costType.GetProperty("IntegerValue");
			if (intValueProp != null)
			{
				var intValue = intValueProp.GetValue(energyCost);
				if (intValue != null)
				{
					cost = Convert.ToInt32(intValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var valueField = costType.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
			if (valueField != null)
			{
				var value = valueField.GetValue(energyCost);
				if (value != null)
				{
					cost = Convert.ToInt32(value);
				}
			}
		}
		
		if (cost == 0)
		{
			var baseValueProp = costType.GetProperty("BaseValue");
			if (baseValueProp != null)
			{
				var baseValue = baseValueProp.GetValue(energyCost);
				if (baseValue != null)
				{
					cost = Convert.ToInt32(baseValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var costProp = costType.GetProperty("Cost");
			if (costProp != null)
			{
				var costValue = costProp.GetValue(energyCost);
				if (costValue != null)
				{
					cost = Convert.ToInt32(costValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var getCostMethod = costType.GetMethod("GetCost");
			if (getCostMethod != null)
			{
				var result = getCostMethod.Invoke(energyCost, null);
				if (result != null)
				{
					cost = Convert.ToInt32(result);
				}
			}
		}
		
		return cost;
	}
	
	public static async Task HandleCardCancellation(CardPlay play, CardModel cardModel, Player owner)
	{
		// 标记本次打出操作已取消，供 UrbanizationPower.AfterCardPlayed 跳过城市化抽牌
		MarkCardPlayCancelled(play);

		var cost = GetCardEnergyCost(cardModel);
		
		GD.Print($"[CardUtils] 取消选择卡牌，费用={cost}");
		
		if (cost > 0 && owner != null)
		{
			await PlayerCmd.GainEnergy(cost, owner);
			GD.Print($"[CardUtils] 已返还 {cost} 能量给玩家");
		}
		else if (cost > 0)
		{
			GD.PrintErr("[CardUtils] 无法获取 Player 对象，能量返还失败");
		}
		
		var cardToReturn = play?.Card ?? cardModel;
		
		if (cardToReturn != null)
		{
			// 参考运输船逻辑：能力卡被移出战斗后，设置 HasBeenRemovedFromState = false 使其能重新进入战斗
			cardToReturn.HasBeenRemovedFromState = false;
			GD.Print($"[CardUtils] 重置卡牌状态 HasBeenRemovedFromState = false");
			
			// 使用 CardPileCmd.Add 添加回手牌，第四个参数传递 cardModel（调用者的 this），与运输船保持一致
			await CardPileCmd.Add(cardToReturn, PileType.Hand, CardPilePosition.Bottom, cardModel);
			
			GD.Print($"[CardUtils] 卡牌已放回手牌底部，当前手牌数: {PileType.Hand.GetPile(cardToReturn.Owner).Cards.Count}");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取实际卡牌实体，放回手牌失败");
		}
	}
	
	public static int GetCardDollarCost(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return 0;
		}
		
		string cardId = cardModel.Id.Entry;
		
		int alliesCost = RedAlert2ModCode.Allies.Cards.AlliesCardValues.GetDollarValue(cardId);
		if (alliesCost > 0)
		{
			return alliesCost;
		}
		
		return RedAlert2ModCode.Soviet.Cards.SovietCardValues.GetDollarValue(cardId);
	}
	
	public static bool IsMcvCard(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return false;
		}
		
		string cardId = cardModel.Id.Entry.ToUpper();
		string normalizedId = cardId.Replace("_", "");
		GD.Print($"[CardUtils] 检查卡牌是否是基地车 - CardId={cardId}, Normalized={normalizedId}");
		
		bool isMcv = normalizedId.Contains("MCV") || 
		             normalizedId.Contains("BASE") || 
		             normalizedId.Contains("COMMANDCENTER") ||
		             normalizedId.Contains("ALLIEDMCV");
		
		if (isMcv)
		{
			GD.Print($"[CardUtils] 识别到基地车卡牌: {cardId}");
		}
		
		return isMcv;
	}
	
	public static bool HasMcvPower(Creature creature)
	{
		if (creature == null || creature.Powers == null)
		{
			return false;
		}
		
		var alliedMcvPower = creature.Powers.OfType<RedAlert2ModCode.Allies.Powers.AlliedMCVPower>().FirstOrDefault();
		if (alliedMcvPower != null)
		{
			return true;
		}
		
		var sovietMcvPower = creature.Powers.OfType<RedAlert2ModCode.Soviet.Powers.SovietMCVPower>().FirstOrDefault();
		if (sovietMcvPower != null)
		{
			return true;
		}
		
		return false;
	}

	/// <summary>
	/// 获取所有单位卡牌类型（士兵、载具、飞机、船只、特殊单位卡、MCV）
	/// </summary>
	/// <returns>单位卡牌类型集合</returns>
	public static HashSet<Type> GetUnitTypes()
	{
		HashSet<Type> unitTypes = new();

		// 从各阵营Registry获取所有单位类型（含特殊单位卡和 MCV）
		unitTypes.UnionWith(AlliedCardRegistry.GetAllUnits().Select(u => u.GetType()));
		unitTypes.UnionWith(SovietCardRegistry.GetAllUnits().Select(u => u.GetType()));
		unitTypes.UnionWith(YuriCardRegistry.GetAllUnits().Select(u => u.GetType()));

		return unitTypes;
	}

	/// <summary>
	/// 所有建筑/防御塔卡牌类型集合的缓存（含围墙）。
	/// 供 UrbanizationPower.TriggerDrawInternal 等需要从牌堆过滤建筑卡的逻辑使用。
	/// </summary>
	private static HashSet<Type>? _allBuildingOrDefenseTowerTypes;

	/// <summary>
	/// 非围墙建筑/防御塔卡牌类型集合的缓存（不含围墙）。
	/// 供 UrbanizationPower 的 AfterCardPlayed 触发判定使用（建筑和防御塔都触发城市化）。
	/// </summary>
	private static HashSet<Type>? _nonWallBuildingOrDefenseTowerTypes;

	/// <summary>
	/// 非围墙且非防御塔的建筑卡牌类型集合的缓存。
	/// 供 BuildingDrawPower 的 AfterCardPlayed 触发判定使用（只有建筑抽牌，防御塔不抽牌）。
	/// </summary>
	private static HashSet<Type>? _nonWallNonDefenseTowerBuildingTypes;

	/// <summary>
	/// 获取所有建筑/防御塔卡牌类型（含围墙）。
	/// 合并盟军和苏军的建筑卡 + 防御塔卡类型集合。
	/// </summary>
	public static HashSet<Type> GetAllBuildingOrDefenseTowerTypes()
	{
		if (_allBuildingOrDefenseTowerTypes != null)
			return _allBuildingOrDefenseTowerTypes;

		var set = new HashSet<Type>();
		set.UnionWith(AlliedCardRegistry.GetAllBuildingCardTypes());
		set.UnionWith(AlliedCardRegistry.GetAllDefenseTowerTypes());
		set.UnionWith(SovietCardRegistry.GetAllBuildingCardTypes());
		set.UnionWith(SovietCardRegistry.GetAllDefenseTowerTypes());

		_allBuildingOrDefenseTowerTypes = set;
		return set;
	}

	/// <summary>
	/// 获取非围墙建筑/防御塔卡牌类型（不含围墙）。
	/// 在 GetAllBuildingOrDefenseTowerTypes 基础上移除各类围墙卡牌。
	/// 供 UrbanizationPower 使用（建筑和防御塔都触发城市化抽牌）。
	/// </summary>
	public static HashSet<Type> GetNonWallBuildingOrDefenseTowerTypes()
	{
		if (_nonWallBuildingOrDefenseTowerTypes != null)
			return _nonWallBuildingOrDefenseTowerTypes;

		var set = new HashSet<Type>(GetAllBuildingOrDefenseTowerTypes());
		set.Remove(typeof(AlliedWallCard));
		set.Remove(typeof(FortifiedWall));
		set.Remove(typeof(SovietWallCard));
		set.Remove(typeof(SovietFortifiedWall));

		_nonWallBuildingOrDefenseTowerTypes = set;
		return set;
	}

	/// <summary>
	/// 获取非围墙且非防御塔的建筑卡牌类型。
	/// 在 GetNonWallBuildingOrDefenseTowerTypes 基础上移除所有防御塔卡牌类型。
	/// 供 BuildingDrawPower 使用（只有建筑抽牌，防御塔不抽牌）。
	/// </summary>
	public static HashSet<Type> GetNonWallNonDefenseTowerBuildingTypes()
	{
		if (_nonWallNonDefenseTowerBuildingTypes != null)
			return _nonWallNonDefenseTowerBuildingTypes;

		var set = new HashSet<Type>(GetNonWallBuildingOrDefenseTowerTypes());
		// 移除所有防御塔类型（盟军和苏军）
		foreach (var towerType in AlliedCardRegistry.GetAllDefenseTowerTypes())
			set.Remove(towerType);
		foreach (var towerType in SovietCardRegistry.GetAllDefenseTowerTypes())
			set.Remove(towerType);

		_nonWallNonDefenseTowerBuildingTypes = set;
		return set;
	}

	/// <summary>
	/// 判断卡牌是否为建筑/防御塔（含围墙）。
	/// 用于从牌堆中过滤建筑卡（如城市化抽牌）。
	/// </summary>
	public static bool IsBuildingOrDefenseTower(CardModel card)
	{
		return GetAllBuildingOrDefenseTowerTypes().Contains(card.GetType());
	}

	/// <summary>
	/// 判断卡牌是否为非围墙建筑/防御塔。
	/// 用于城市化能力触发判定，围墙不触发，建筑和防御塔都触发。
	/// </summary>
	public static bool IsNonWallBuildingOrDefenseTower(CardModel card)
	{
		return GetNonWallBuildingOrDefenseTowerTypes().Contains(card.GetType());
	}

	/// <summary>
	/// 判断卡牌是否为非围墙且非防御塔的建筑。
	/// 用于建筑抽牌能力触发判定，围墙和防御塔都不触发，只有建筑触发。
	/// </summary>
	public static bool IsNonWallNonDefenseTowerBuilding(CardModel card)
	{
		return GetNonWallNonDefenseTowerBuildingTypes().Contains(card.GetType());
	}

	/// <summary>
	/// 所有防御塔卡牌类型（不含围墙）的缓存。
	/// 合并盟军和苏军的防御塔类型。
	/// </summary>
	private static HashSet<Type>? _allDefenseTowerTypes;

	/// <summary>
	/// 获取所有防御塔卡牌类型（不含围墙）。
	/// 合并盟军和苏军的防御塔类型。
	/// </summary>
	public static HashSet<Type> GetAllDefenseTowerTypes()
	{
		if (_allDefenseTowerTypes != null)
			return _allDefenseTowerTypes;

		var set = new HashSet<Type>();
		set.UnionWith(AlliedCardRegistry.GetAllDefenseTowerTypes());
		set.UnionWith(SovietCardRegistry.GetAllDefenseTowerTypes());

		_allDefenseTowerTypes = set;
		return set;
	}

	/// <summary>
	/// 所有防御塔卡牌类型（含围墙）的缓存。
	/// 围墙归属于防御塔类，打出建筑时可以抽到围墙。
	/// 供 UrbanizationPower 打出建筑时抽取防御塔使用。
	/// </summary>
	private static HashSet<Type>? _allDefenseTowerTypesWithWalls;

	/// <summary>
	/// 获取所有防御塔卡牌类型（含围墙）。
	/// 在 GetAllDefenseTowerTypes 基础上加入各类围墙卡牌。
	/// 供 UrbanizationPower 使用（打出建筑→抽防御塔，围墙算防御塔可被抽到）。
	/// </summary>
	public static HashSet<Type> GetAllDefenseTowerTypesWithWalls()
	{
		if (_allDefenseTowerTypesWithWalls != null)
			return _allDefenseTowerTypesWithWalls;

		var set = new HashSet<Type>(GetAllDefenseTowerTypes());
		set.Add(typeof(AlliedWallCard));
		set.Add(typeof(FortifiedWall));
		set.Add(typeof(SovietWallCard));
		set.Add(typeof(SovietFortifiedWall));

		_allDefenseTowerTypesWithWalls = set;
		return set;
	}

	/// <summary>
	/// 判断卡牌是否为防御塔（不含围墙）。
	/// 用于城市化能力触发判定：打出防御塔时触发抽建筑，打出围墙不触发。
	/// </summary>
	public static bool IsNonWallDefenseTower(CardModel card)
	{
		return GetAllDefenseTowerTypes().Contains(card.GetType());
	}
}