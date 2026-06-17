using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军阵营卡牌数值存储
/// 统一管理所有盟军卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class AlliesCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>美国大兵 - 1费3伤害两次，升级后4伤害两次，价格200</summary>
	public static CardValueStore.CardValues AmericanSoldier => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 2,
		DollarValue = 200
	};
	
	/// <summary>军犬 - 0费3伤害1层虚弱，升级后4伤害2层虚弱，价格200</summary>
	public static CardValueStore.CardValues DogSoldier => new()
	{
		Cost = 0,
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 1,
		RepeatUpgraded = 1,
		DollarValue = 200
	};
	
	/// <summary>火箭飞行兵 - 0费1伤害2次，获得2点敏捷，价格600</summary>
	public static CardValueStore.CardValues RocketSoldier => new()
	{
		Cost = 0,
		Damage = 1,
		Repeat = 2,
		MagicNumber = 2,  // 敏捷值
		DollarValue = 600
	};
	
	/// <summary>工程师 - 0费2覆甲，升级后3覆甲，价格500</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Cost = 0,
		Block = 2,
		BlockUpgraded = 1,
		DollarValue = 500
	};
	
	// ==================== 装甲单位 ====================
	/// <summary>盟军基地车 - 0费，价格3000</summary>
	public static CardValueStore.CardValues AlliedMCV => new()
	{
		Cost = 0,
		DollarValue = 3000
		// 基地车主要是功能牌
	};

	/// <summary>灰熊坦克 - 1费5攻击5防御，升级后8攻击8防御，价格700</summary>
	public static CardValueStore.CardValues GrizzlyTank => new()
	{
		Cost = 1,
		Damage = 5,
		DamageUpgraded = 3,
		Block = 5,
		BlockUpgraded = 3,
		DollarValue = 700
	};
	
	/// <summary>IFV步兵战车 - 1费，获得1点敏捷，2伤害2次，2护盾，升级后2伤害4次，价格600</summary>
	public static CardValueStore.CardValues Ifv => new()
	{
		Cost = 1,
		Damage = 2,
		Repeat = 2,
		RepeatUpgraded = 2,
		Block = 2,
		MagicNumber = 1,  // 敏捷值
		DollarValue = 600
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>入侵者战机 - 2费10伤害2层易伤，升级后13伤害3层易伤，价格1200</summary>
	public static CardValueStore.CardValues Intruder => new()
	{
		Cost = 2,
		Damage = 10,
		DamageUpgraded = 3,
		Repeat = 2,
		RepeatUpgraded = 1,
		DollarValue = 1200
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>兵营 - 0费，价格500</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		Cost = 0,
		DollarValue = 500
		// 兵营主要是功能牌，数值由具体生成的单位决定
	};
	
	/// <summary>盟军重工 - 0费，价格2000</summary>
	public static CardValueStore.CardValues AlliedWarFactory => new()
	{
		Cost = 0,
		DollarValue = 2000
		// 重工主要是功能牌，数值由具体生成的单位决定
	};
	
	/// <summary>空指部 - 0费，价格1000</summary>
	public static CardValueStore.CardValues AirForceCommand => new()
	{
		Cost = 0,
		DollarValue = 1000
		// 空指部主要是功能牌，数值由具体生成的单位决定
	};
	
	/// <summary>发电厂 - 0费，每抽10张牌获得1能量，升级后7张，价格800</summary>
	public static CardValueStore.CardValues PowerPlant => new()
	{
		Cost = 0,
		MagicNumber = 10,  // 抽牌阈值
		MagicNumberUpgraded = -3,  // 升级后 7 = 10 + (-3)
		DollarValue = 800
	};
	
	/// <summary>矿场 - 0费，价格2000</summary>
	public static CardValueStore.CardValues AlliedRefinery => new()
	{
		Cost = 0,
		DollarValue = 2000
		// 矿场主要是功能牌
	};
	
	// ==================== 防御建筑 ====================
	
	/// <summary>盟军围墙 - 0费1护盾，花费100资金，升级后3护盾，价格100</summary>
	public static CardValueStore.CardValues AlliedWall => new()
	{
		Cost = 0,
		Block = 1,
		BlockUpgraded = 2,
		DollarValue = 100
	};
	
	/// <summary>光棱塔 - 2费，回合开始时对随机敌人造成5伤害1次，价格1500</summary>
	public static CardValueStore.CardValues PrismTower => new()
	{
		Cost = 2,
		Damage = 5,
		Repeat = 1,
		DollarValue = 1500
	};
	
	/// <summary>机枪碉堡 - 1费，每回合对随机敌人造成2伤害，自己获得5防御，价格500</summary>
	public static CardValueStore.CardValues Pillbox => new()
	{
		Cost = 1,
		Damage = 2,
		DamageUpgraded = 1,
		Block = 5,
		BlockUpgraded = 3,
		DollarValue = 500
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>超时空矿车 - 0费获得1000资金，升级后1500资金</summary>
	public static CardValueStore.CardValues ChronoMiner => new()
	{
		Cost = 0,
		DollarValue = 1000,
		DollarValueUpgraded = 500
	};

	// ==================== 运转卡牌 ====================
	
	/// <summary>卖本 - 1费获得2400资金，消耗</summary>
	public static CardValueStore.CardValues SellMCV => new()
	{
		Cost = 1,
		DollarValue = 2400
	};

	/// <summary>集结 - 1费，从牌堆中召集2张单位卡到手牌中，升级后3张</summary>
	public static CardValueStore.CardValues Ra2Rally => new()
	{
		Cost = 1,
		MagicNumber = 2,           // 召集单位卡数量
		MagicNumberUpgraded = 1    // 升级后增加1张
	};

	/// <summary>策略：塔防 - 2费能力卡，升级后1费</summary>
	public static CardValueStore.CardValues StrategyTowerDefense => new()
	{
		Cost = 2,
		CostUpgraded = -1          // 升级后费用降低1
	};

	// ==================== 海军单位 ====================

	/// <summary>海豚 - 1费，对所有敌人造成2伤害1层易伤，升级后2层易伤，价格500</summary>
	public static CardValueStore.CardValues Dolphin => new()
	{
		Cost = 1,
		Damage = 2,
		Repeat = 1,
		RepeatUpgraded = 1,        // 升级后易伤层数+1
		DollarValue = 500
	};

	// ==================== 建筑卡牌（新增） ====================

	/// <summary>船厂 - 0费，价格1000</summary>
	public static CardValueStore.CardValues Shipyard => new()
	{
		Cost = 0,
		DollarValue = 1000
		// 船厂主要是功能牌，数值由具体生成的单位决定
	};
	
	// ==================== 数值映射创建方法 ====================
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateSoldierValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "AMERICANSOLDIER", AmericanSoldier },
			{ "DOGSOLDIER", DogSoldier },
			{ "ROCKETSOLDIER", RocketSoldier },
			{ "ENGINEER", Engineer }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateVehicleValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "GRIZZLYTANK", GrizzlyTank },
			{ "IFV", Ifv },
			{ "ALLIEDMCV", AlliedMCV },
			{ "CHRONOMINER", ChronoMiner }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateAircraftValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "INTRUDER", Intruder }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateShipValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "DOLPHIN", Dolphin }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateBuildingValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "BARRACKSCARD", Barracks },
			{ "ALLIEDWARFACTORY", AlliedWarFactory },
			{ "AIRFORCECOMMAND", AirForceCommand },
			{ "POWERPLANTCARD", PowerPlant },
			{ "ALLIEDREFINERY", AlliedRefinery },
			{ "PRISMTOWERCARD", PrismTower },
			{ "ALLIEDWALLCARD", AlliedWall },
			{ "PILLBOXCARD", Pillbox }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateAllValuesMap()
	{
		var map = new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>();
		
		foreach (var kvp in CreateSoldierValuesMap())
			map[kvp.Key] = kvp.Value;
		
		foreach (var kvp in CreateVehicleValuesMap())
			map[kvp.Key] = kvp.Value;
		
		foreach (var kvp in CreateAircraftValuesMap())
			map[kvp.Key] = kvp.Value;
		
		foreach (var kvp in CreateShipValuesMap())
			map[kvp.Key] = kvp.Value;
		
		foreach (var kvp in CreateBuildingValuesMap())
			map[kvp.Key] = kvp.Value;
		
		return map;
	}
	
	/// <summary>
	/// 根据卡牌ID获取单位价格
	/// </summary>
	/// <param name="cardId">卡牌ID</param>
	/// <returns>单位价格，未找到则返回0</returns>
	public static int GetDollarValue(string cardId)
	{
		if (string.IsNullOrEmpty(cardId))
			return 0;
		
		string key = cardId.ToUpper().Replace("_", "");
		var allValues = CreateAllValuesMap();
		
		if (allValues.TryGetValue(key, out var values))
		{
			return (int)values.DollarValue;
		}
		
		return 0;
	}
}
