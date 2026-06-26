using System.Collections.Generic;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军阵营卡牌数值存储
/// 统一管理所有苏军卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class SovietCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>动员兵 - 1费3伤害两次，升级后4伤害两次，价格100</summary>
	public static CardValueStore.CardValues Conscript => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 2,
		DollarValue = 100
	};
	
	/// <summary>军犬 - 0费3伤害1层虚弱，升级后4伤害2层虚弱，价格200</summary>
	public static CardValueStore.CardValues AttackDog => new()
	{
		Cost = 0,
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 1,
		RepeatUpgraded = 1,
		DollarValue = 200
	};
	
	/// <summary>磁暴步兵 - 2费6伤害，升级后9伤害，价格500</summary>
	public static CardValueStore.CardValues TeslaTrooper => new()
	{
		Cost = 2,
		Damage = 6,
		DamageUpgraded = 3,
		Repeat = 1,
		DollarValue = 500
	};
	
	/// <summary>工程师 - 1费技能卡，从选项中选择指令，价格500</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Cost = 1,
		Block = 6,
		BlockUpgraded = 3,
		DollarValue = 500
	};
	
	// ==================== 装甲单位 ====================
	
	/// <summary>犀牛坦克 - 1费5攻击7防御，升级后8攻击8防御，赋予1层易伤（升级后2层），价格900</summary>
	public static CardValueStore.CardValues RhinoTank => new()
	{
		Cost = 1,
		Damage = 5,
		DamageUpgraded = 3,
		Block = 7,
		BlockUpgraded = 1,
		MagicNumber = 1,      // 易伤层数
		MagicNumberUpgraded = 1,  // 升级后额外1层（共2层）
		DollarValue = 900
	};
	
	/// <summary>防空履带车 - 1费，获得1点敏捷，5护盾，部署：存储士兵单位，价格500</summary>
	public static CardValueStore.CardValues FlakTrack => new()
	{
		Cost = 1,
		Block = 5,
		MagicNumber = 1,  // 敏捷值
		DollarValue = 500
	};
	
	/// <summary>苏军基地车 - 0费，价格3000</summary>
	public static CardValueStore.CardValues SovietMCV => new()
	{
		Cost = 0,
		DollarValue = 3000
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>基洛夫飞艇 - 3费10伤害，升级后15伤害，价格2000</summary>
	public static CardValueStore.CardValues Kirov => new()
	{
		Cost = 3,
		Damage = 10,
		DamageUpgraded = 5,
		Repeat = 1,
		DollarValue = 2000
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>苏军兵营 - 0费，价格500</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		Cost = 0,
		DollarValue = 500
	};
	
	/// <summary>苏军重工 - 0费，价格1000</summary>
	public static CardValueStore.CardValues SovietWarFactory => new()
	{
		Cost = 0,
		DollarValue = 1000
	};
	
	/// <summary>苏军船厂 - 0费，价格1000</summary>
	public static CardValueStore.CardValues Shipyard => new()
	{
		Cost = 0,
		DollarValue = 1000
	};
	
	/// <summary>苏军维修厂 - 0费能力卡（升级后0费），价格800</summary>
	public static CardValueStore.CardValues RepairDepot => new()
	{
		Cost = 0,
		CostUpgraded = 0,
		DollarValue = 800
	};
	
	/// <summary>苏军哨戒炮 - 0费，回合开始时对敌人造成1伤害2次（升级后2伤害），获得3防御，价格500</summary>
	public static CardValueStore.CardValues SovietPillbox => new()
	{
		Cost = 0,
		Damage = 1,
		DamageUpgraded = 1,
		Repeat = 2,
		Block = 3,
		BlockUpgraded = 0,
		DollarValue = 500
	};
	
	/// <summary>磁能反应堆 - 0费，每10张牌获得1能量，升级后每7张，价格800</summary>
	public static CardValueStore.CardValues NuclearReactor => new()
	{
		Cost = 0,
		MagicNumber = 10,
		MagicNumberUpgraded = -3,
		DollarValue = 800
	};
	
	/// <summary>矿场 - 0费，价格2000</summary>
	public static CardValueStore.CardValues SovietRefinery => new()
	{
		Cost = 0,
		DollarValue = 2000
	};
	
	// ==================== 防御建筑 ====================
	
	/// <summary>苏军围墙 - 0费5护盾，升级后8护盾，价格100</summary>
	public static CardValueStore.CardValues SovietWall => new()
	{
		Cost = 0,
		Block = 1,
		BlockUpgraded = 1,
		DollarValue = 100
	};
	
	/// <summary>磁暴线圈 - 0费，回合开始时对随机敌人造成3伤害1次，价格1500</summary>
	public static CardValueStore.CardValues TeslaCoil => new()
	{
		Cost = 0,
		Damage = 3,
		DamageUpgraded = 4,
		Repeat = 1,
		DollarValue = 1500
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>武装采矿车 - 0费攻击造成2点伤害（升级后全体），获得1400资金</summary>
	public static CardValueStore.CardValues WarMiner => new()
	{
		Cost = 0,
		Damage = 2,
		DamageUpgraded = 0,  // 升级后伤害不变（2点），只是变为全体攻击
		DollarValue = 1400,
		DollarValueUpgraded = 600
	};
	
	// ==================== 数值映射创建方法 ====================
	
	public static Dictionary<string, CardValueStore.CardValues> CreateSoldierValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "CONSCRIPT", Conscript },
			{ "ATTACKDOG", AttackDog },
			{ "TESLATROOPER", TeslaTrooper },
			{ "ENGINEER", Engineer }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateVehicleValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "RHINOTANK", RhinoTank },
			{ "FLAKTRACK", FlakTrack },
			{ "SOVIETMCV", SovietMCV },
			{ "WARMINER", WarMiner }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateAircraftValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "KIROV", Kirov }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateShipValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			// 待添加苏军海军单位
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateBuildingValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "SOVIETBARRACKSCARD", Barracks },
			{ "SOVIETWARFACTORY", SovietWarFactory },
			{ "SHIPYARDCARD", Shipyard },
			{ "REPAIRDEPOT", RepairDepot },
			{ "SOVIETPILLBOXCARD", SovietPillbox },
			{ "NUCLEARREACTOR", NuclearReactor },
			{ "SOVIETREFINERY", SovietRefinery },
			{ "TESLACOIL", TeslaCoil },
			{ "SOVIETWALLCARD", SovietWall }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateAllValuesMap()
	{
		var map = new Dictionary<string, CardValueStore.CardValues>();
		
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
