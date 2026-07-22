using RedAlert2ModCode.Common.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军阵营卡牌数值存储
/// 统一管理所有盟军卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class AlliesCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>美国大兵 - 0费2伤害2次，升级后2伤害3次，价格200</summary>
	public static CardValueStore.CardValues AmericanSoldier => new()
	{
		Cost = 0,
		Damage = 2,
		DamageUpgraded = 0,
		Repeat = 2,
		RepeatUpgraded = 1,
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
	
	/// <summary>重装大兵 - 1费攻击卡，获得5格挡（升级8），部署造成5伤害（升级7）+1易伤，价格250</summary>
	public static CardValueStore.CardValues GuardianGI => new()
	{
		Cost = 1,
		Block = 5,
		BlockUpgraded = 3,
		Damage = 5,
		DamageUpgraded = 2,
		DollarValue = 250
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
	
	/// <summary>工程师 - 1费技能卡，从选项中选择指令，价格500</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Cost = 1,
		Repeat = 2,          // 基础选项数量
		RepeatUpgraded = 1,  // 升级后额外增加1个选项（共3个）
		DollarValue = 500
	};

	/// <summary>狙击手 - 1费攻击卡，无视格挡造成9(升级12)伤害，价格600，需要空指部/雷达(T2)</summary>
	public static CardValueStore.CardValues Sniper => new()
	{
		Cost = 1,
		Damage = 9,
		DamageUpgraded = 3,
		DollarValue = 600
	};

	/// <summary>超时空军团兵 - 1费攻击卡，赋予敌人血量6%(升级10%)层数的抹除，首次眩晕敌人，价格1200，需要作战实验室</summary>
	public static CardValueStore.CardValues ChronoLegionnaire => new()
	{
		Cost = 1,
		MagicNumber = 6,           // 抹除层数百分比（基础6%）
		MagicNumberUpgraded = 4,   // 升级后10% = 6 + 4
		DollarValue = 1500
	};
	
	/// <summary>伞兵 - 1费攻击卡，升级后0费，将6张美国大兵加入手牌，消耗</summary>
	public static CardValueStore.CardValues Paratrooper => new()
	{
		Cost = 1,
		CostUpgraded = -1,   // 升级后费用变为 1 + (-1) = 0
		Repeat = 6,          // 添加的美国大兵数量
		DollarValue = 300
	};

	/// <summary>空降部队 - 1费运转卡，升级后0费，获得4张美国大兵和1张重装大兵（带消耗）</summary>
	public static CardValueStore.CardValues AirborneDivision => new()
	{
		Cost = 1,
		CostUpgraded = -1,   // 升级后费用变为 0
		DollarValue = 0
	};
	
	// ==================== 装甲单位 ====================
	/// <summary>盟军基地车 - 0费，价格3000</summary>
	public static CardValueStore.CardValues AlliedMCV => new()
	{
		Cost = 0,
		DollarValue = 3000
		// 基地车主要是功能牌
	};

	/// <summary>灰熊坦克 - 1费3攻击5防御，升级后5攻击8防御，价格700</summary>
	public static CardValueStore.CardValues GrizzlyTank => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 2,
		Block = 5,
		BlockUpgraded = 3,
		DollarValue = 700
	};
	
	/// <summary>IFV步兵战车 - 1费，抽1/2张牌，5护盾，价格600</summary>
	public static CardValueStore.CardValues Ifv => new()
	{
		Cost = 1,
		Block = 5,
		BlockUpgraded = 2,         // 升级后7 = 5 + 2
		MagicNumber = 1,           // 抽牌数
		MagicNumberUpgraded = 1,   // 升级后2 = 1 + 1
		DollarValue = 600
	};
	
	/// <summary>坦克杀手 - 1费攻击卡，赋予自身1层虚弱，造成16(升级20)点伤害，价格900，需要空指部/雷达解锁</summary>
	public static CardValueStore.CardValues TankDestroyer => new()
	{
		Cost = 1,
		Damage = 16,
		DamageUpgraded = 4,        // 升级后20 = 16 + 4
		Repeat = 1,                // 虚弱层数
		DollarValue = 900
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>入侵者战机 - 1费12伤害1层易伤，升级后15伤害2层易伤，价格1200</summary>
	public static CardValueStore.CardValues Intruder => new()
	{
		Cost = 1,
		Damage = 12,
		DamageUpgraded = 3,
		Repeat = 1,
		RepeatUpgraded = 1,
		DollarValue = 1200
	};

	/// <summary>黑鹰战机 - 1费14伤害2层易伤，升级后17伤害3层易伤，额外携带一层飞鹰战备，价格1200</summary>
	public static CardValueStore.CardValues BlackHawk => new()
	{
		Cost = 1,
		Damage = 14,
		DamageUpgraded = 3,
		Repeat = 2,
		RepeatUpgraded = 1,
		DollarValue = 1200
	};

	/// <summary>夜莺直升机 - 1费攻击卡，本回合获得2点敏捷(升级3点)，造成3点伤害，可部署存储士兵单位，价格600</summary>
	public static CardValueStore.CardValues NightHawkChopper => new()
	{
		Cost = 1,
		Damage = 3,
		MagicNumber = 2,           // 敏捷值
		MagicNumberUpgraded = 1,   // 升级后3 = 2 + 1
		DollarValue = 600
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>兵营 - 0费，价格500</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		Cost = 0,
		DollarValue = 500
		// 兵营主要是功能牌，数值由具体生成的单位决定
	};
	
	/// <summary>盟军重工 - 0费能力卡，价格2000</summary>
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

	/// <summary>矿石精炼器 - 0费能力卡，+25%矿石价值（升级后+50%），价格2500</summary>
	public static CardValueStore.CardValues OreRefinery => new()
	{
		Cost = 0,
		DollarValue = 2500,
		MagicNumber = 25,           // 基础25%矿石价值加成
		MagicNumberUpgraded = 25    // 升级后增加25%，总共50%
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

	/// <summary>盟军坚固围墙 - 0费3护盾（升级后5护盾），花费100资金，价格100</summary>
	public static CardValueStore.CardValues AlliedFortifiedWall => new()
	{
		Cost = 0,
		Block = 3,
		BlockUpgraded = 2,
		DollarValue = 100
	};
	
	/// <summary>光棱塔 - 2费，回合开始时对随机敌人造成5伤害1次，价格1500</summary>
	public static CardValueStore.CardValues PrismTower => new()
	{
		Cost = 2,
		Damage = 5,
		Repeat = 1,
		Stars = 2,             // 未升级时每次叠加增加的伤害
		StarsUpgraded = 3,      // 升级后每次叠加增加的伤害 (5 = 2 + 3)
		DollarValue = 1500
	};
	
	/// <summary>机枪碉堡 - 1费，回合开始时对敌人造成1伤害2次（升级后2伤害），获得3防御（升级不加），价格500</summary>
	public static CardValueStore.CardValues Pillbox => new()
	{
		Cost = 1,
		Damage = 1,
		DamageUpgraded = 1,  // 升级后2 = 1 + 1
		Repeat = 2,          // 攻击次数
		Block = 3,
		BlockUpgraded = 0,   // 升级不加
		DollarValue = 500
	};

	/// <summary>爱国者导弹 - 1费能力卡，回合开始时获得5格挡（升级8），价格1000</summary>
	public static CardValueStore.CardValues PatriotMissile => new()
	{
		Cost = 1,
		Block = 5,
		BlockUpgraded = 3,  // 升级后8 = 5 + 3
		DollarValue = 1000
	};

	/// <summary>巨炮 - 2费攻击卡，金卡，回合开始时对敌人造成20(升级30)点伤害，需要空指部/雷达解锁，价格2000</summary>
	public static CardValueStore.CardValues GrandCannon => new()
	{
		Cost = 2,
		Damage = 20,
		DamageUpgraded = 10,  // 升级后30 = 20 + 10
		DollarValue = 2000
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>超时空矿车 - 0费获得500资金（升级后1000），价格1400</summary>
	public static CardValueStore.CardValues ChronoMiner => new()
	{
		Cost = 0,
		DollarValue = 500,
		DollarValueUpgraded = 500,
		BuildCost = 1400
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
		Cost = 3,
		CostUpgraded = -1          // 升级后费用降低1
	};

	/// <summary>黄金矿 - 1费能力卡，获得黄金矿储备</summary>
	public static CardValueStore.CardValues GoldMine => new()
	{
		Cost = 1,
		DollarValue = 10000,        // 基础储备
		DollarValueUpgraded = 10000 // 升级后储备 20000 = 10000 + 10000
	};

	/// <summary>宝石矿 - 1费能力卡，获得宝石矿储备</summary>
	public static CardValueStore.CardValues GemMine => new()
	{
		Cost = 1,
		DollarValue = 5000,        // 基础储备
		DollarValueUpgraded = 5000 // 升级后储备 10000 = 5000 + 5000
	};

	/// <summary>黄金矿柱 - 1费能力卡，获得黄金矿储备和黄金矿柱能力</summary>
	public static CardValueStore.CardValues GoldMineColumn => new()
	{
		Cost = 1,
		DollarValue = 5000,        // 基础储备
		DollarValueUpgraded = 10000, // 升级后储备 15000 = 5000 + 10000
		Stars = 200                // 每回合增加的金矿储备
	};

	// ==================== 海军单位 ====================

	/// <summary>盟军运输船 - 1费技能卡，存储最多3张手牌（升级后5张），获得7格挡（升级10），价格900</summary>
	public static CardValueStore.CardValues AlliedTransportShip => new()
	{
		Cost = 1,
		MagicNumber = 3,
		MagicNumberUpgraded = 2,
		Block = 7,
		BlockUpgraded = 3,
		DollarValue = 900
	};

	/// <summary>海豚 - 1费，对所有敌人造成2伤害1层易伤，升级后2层易伤，价格500</summary>
	public static CardValueStore.CardValues Dolphin => new()
	{
		Cost = 1,
		Damage = 2,
		Repeat = 1,
		RepeatUpgraded = 1,        // 升级后易伤层数+1
		DollarValue = 500
	};

	/// <summary>驱逐舰 - 1费攻击卡，造成8伤害（升级12）。若敌人意图防御，改为给予1层易伤，造成5伤害（升级8）2次，价格1000</summary>
	public static CardValueStore.CardValues Destroyer => new()
	{
		Cost = 1,
		Damage = 8,                // 基础伤害
		DamageUpgraded = 4,        // 升级后12 = 8 + 4
		MagicNumber = 5,           // 防御意图时的单次伤害
		MagicNumberUpgraded = 3,   // 升级后8 = 5 + 3
		Repeat = 2,                // 防御意图时的重复次数
		DollarValue = 1000
	};

	/// <summary>神盾巡洋舰 - 1费技能卡，获得8格挡（升级12）。若敌人意图攻击，多获得1轮，价格1200</summary>
	public static CardValueStore.CardValues Agisicon => new()
	{
		Cost = 1,
		Block = 8,                 // 基础格挡
		BlockUpgraded = 4,         // 升级后12 = 8 + 4
		DollarValue = 1200
	};

	// ==================== 建筑卡牌（新增） ====================

	/// <summary>船厂 - 0费，价格1000</summary>
	public static CardValueStore.CardValues Shipyard => new()
	{
		Cost = 0,
		DollarValue = 1000
		// 船厂主要是功能牌，数值由具体生成的单位决定
	};

	/// <summary>作战实验室 - 0费能力卡，解锁高级兵种，价格2000（升级后1000）</summary>
	public static CardValueStore.CardValues AlliedBattleLab => new()
	{
		Cost = 0,
		DollarValue = 2000,
		DollarValueUpgraded = 1000
	};

	/// <summary>修理厂 - 2费能力卡（升级后1费），回合开始时花费$1000从消耗牌堆选择一张牌加入弃牌堆，价格800</summary>
	public static CardValueStore.CardValues RepairDepot => new()
	{
		Cost = 2,           // 未升级：2费
		CostUpgraded = 1,   // 升级后：1费
		DollarValue = 800
	};

	/// <summary>油井 - 1费能力卡，立即获得$1000，回合开始时获得$200（升级后$500）资金，中立建筑不受建造厂限制</summary>
	public static CardValueStore.CardValues OilDerrick => new()
	{
		Cost = 1,                   // 1费
		DollarValue = 1000,         // 立即获得的资金
		Damage = 200,               // 每回合获得的资金（基础）
		DamageUpgraded = 300        // 升级后额外增加的资金（总500 = 200 + 300）
	};

	/// <summary>停产 - 1费技能卡，选择1个生产序列启动/停产，升级后可选择所有生产序列</summary>
	public static CardValueStore.CardValues StopProduction => new()
	{
		Cost = 1,                   // 1费
		Repeat = 1                  // 未升级时选择数量
	};

	// ==================== 绝地战备卡牌 ====================

	/// <summary>提前倒矿 - 1费技能卡，抽取所有矿车，本回合矿车收益为80%</summary>
	public static CardValueStore.CardValues EarlyMining => new()
	{
		Cost = 1,                   // 1费
		MagicNumber = 80            // 矿车收益百分比：80%
	};

	/// <summary>飞鹰500kg - 3费攻击卡，绝地战备，获得能力并赋予目标锁定，升级后2费</summary>
	public static CardValueStore.CardValues Eagle500kg => new()
	{
		Cost = 3,                   // 3费
		CostUpgraded = -1,          // 升级后费用降低1，变为2费
		DollarValue = 0             // 绝地战备卡牌无价格
	};

	/// <summary>飞鹰机枪扫射 - 1费攻击卡，绝地战备，对目标锁定敌人造成3点伤害4次，升级后4点</summary>
	public static CardValueStore.CardValues EagleMachineGun => new()
	{
		Cost = 1,                   // 1费
		Damage = 3,                 // 基础伤害
		DamageUpgraded = 1,         // 升级后4 = 3 + 1
		Repeat = 4,                 // 攻击次数
		DollarValue = 0             // 绝地战备卡牌无价格
	};

	/// <summary>飞鹰空袭 - 1费攻击卡，绝地战备，对全部敌人造成9点伤害，升级后13点</summary>
	public static CardValueStore.CardValues EagleAirStrike => new()
	{
		Cost = 1,                   // 1费
		Damage = 9,                 // 基础伤害
		DamageUpgraded = 4,         // 升级后13 = 9 + 4
		DollarValue = 0             // 绝地战备卡牌无价格
	};

	// ==================== 高科技(T2)单位 - 需要作战实验室解锁 ====================

	/// <summary>幻影坦克 - 1费攻击卡，价格1000，需要作战实验室</summary>
	public static CardValueStore.CardValues MirageTank => new()
	{
		Cost = 1,
		Damage = 10,
		DamageUpgraded = 5,  // 升级后15 = 10 + 5
		Block = 12,  // 攻击意图时的格挡
		BlockUpgraded = 3,  // 升级后15 = 12 + 3
		DollarValue = 1000
	};

	/// <summary>光棱坦克 - 1费攻击卡，价格1200，需要作战实验室</summary>
	public static CardValueStore.CardValues PrismTank => new()
	{
		Cost = 1,
		Damage = 15,
		DamageUpgraded = 5,  // 升级后20 = 15 + 5
		DollarValue = 1200
	};

	/// <summary>战斗要塞 - 2费攻击卡，高科技装甲单位，获得5格挡(升级8)，存储士兵单位并融合效果，价格2000，需要作战实验室</summary>
	public static CardValueStore.CardValues BattleFortress => new()
	{
		Cost = 2,
		Block = 5,
		BlockUpgraded = 3,  // 升级后8 = 5 + 3
		DollarValue = 2000
	};

	/// <summary>航空母舰 - 2费攻击卡，高科技海军单位，需要作战实验室，价格2000</summary>
	public static CardValueStore.CardValues AircraftCarrier => new()
	{
		Cost = 2,
		DollarValue = 2000
	};

	/// <summary>超时空传送仪 - 0费能力卡，金卡，高科技建筑，需要作战实验室，价格3000</summary>
	public static CardValueStore.CardValues ChronoSphere => new()
	{
		Cost = 0,
		DollarValue = 3000,
		Repeat = 3,                    // 基础间隔回合
		RepeatUpgraded = 2             // 升级后间隔回合
	};

	/// <summary>超时空传送 - 1费技能卡（升级后0费），金卡，消耗，高科技运转卡，需要作战实验室</summary>
	public static CardValueStore.CardValues ChronoWarp => new()
	{
		Cost = 1,                      // 基础费用
		CostUpgraded = -1,             // 升级后费用变为 1 + (-1) = 0
		DollarValue = 0                // 运转卡无价格
	};

	/// <summary>天气控制器 - 0费能力卡，金卡，高科技建筑，需要作战实验室，价格5000</summary>
	public static CardValueStore.CardValues WeatherController => new()
	{
		Cost = 0,
		DollarValue = 5000,
		Repeat = 3,                    // 基础间隔回合
		RepeatUpgraded = 2,            // 升级后间隔回合
		Block = 3                      // 触发时获得的电球数量
	};

	/// <summary>闪电风暴 - 3费技能卡（升级后3费），金卡，高科技运转卡，需要作战实验室</summary>
	public static CardValueStore.CardValues LightningStorm => new()
	{
		Cost = 3,                      // 基础费用
		CostUpgraded = 0,              // 升级后费用不变
		DollarValue = 0,               // 运转卡无价格
		Block = 1,                     // 电球数量（升级后翻倍）
		BlockUpgraded = 1              // 升级后电球数量增加1（1+1=2）
	};

	// ==================== 数值映射创建方法 ====================
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateSoldierValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "AMERICANSOLDIER", AmericanSoldier },
			{ "ALLIESDOGSOLDIER", DogSoldier },
			{ "GUARDIANGI", GuardianGI },
			{ "ROCKETSOLDIER", RocketSoldier },
			{ "ALLIESENGINEER", Engineer },
			{ "CHRONOLEGIONNAIRE", ChronoLegionnaire }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateVehicleValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "GRIZZLYTANK", GrizzlyTank },
			{ "IFV", Ifv },
			{ "ALLIEDMCV", AlliedMCV },
			{ "CHRONOMINER", ChronoMiner },
			{ "BATTLEFORTRESS", BattleFortress },
			{ "TANKDESTROYER", TankDestroyer }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateAircraftValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "INTRUDER", Intruder },
			{ "BLACKHAWK", BlackHawk },
			{ "NIGHTHAWKCHOPPER", NightHawkChopper }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateShipValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "ALLIEDTRANSPORTSHIP", AlliedTransportShip },
			{ "DOLPHIN", Dolphin },
			{ "DESTROYER", Destroyer },
			{ "AGISICON", Agisicon }
		};
	}
	
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateBuildingValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "ALLIESBARRACKSCARD", Barracks },
			{ "ALLIEDWARFACTORY", AlliedWarFactory },
			{ "AIRFORCECOMMAND", AirForceCommand },
			{ "ALLIESSHIPYARDCARD", Shipyard },
			{ "POWERPLANTCARD", PowerPlant },
			{ "ALLIEDREFINERY", AlliedRefinery },
			{ "PRISMTOWERCARD", PrismTower },
			{ "ALLIEDWALLCARD", AlliedWall },
			{ "ALLIESPILLBOXCARD", Pillbox },
			{ "PATRIOTMISSILE", PatriotMissile },
			{ "ALLIEDBATTLELAB", AlliedBattleLab },
			{ "GRANDCANNON", GrandCannon }
		};
	}

	public static System.Collections.Generic.Dictionary<System.Type, decimal> CreateSellablePowerDollarMap()
	{
		return new System.Collections.Generic.Dictionary<System.Type, decimal>
		{
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedRefineryPower), AlliedRefinery.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedWarFactoryPower), AlliedWarFactory.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.BattleLabPower), AlliedBattleLab.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedAirForceCommandPower), AirForceCommand.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedMCVPower), AlliedMCV.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedBarracksPower), Barracks.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.AlliedShipyardPower), Shipyard.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.PrismTowerPower), PrismTower.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.PillboxPower), Pillbox.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.PatriotMissilePower), PatriotMissile.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.PowerPlantPower), PowerPlant.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.OreRefineryPower), OreRefinery.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.ChronoSpherePower), ChronoSphere.DollarValue },
			{ typeof(RedAlert2ModCode.Allies.Powers.WeatherControllerPower), WeatherController.DollarValue }
		};
	}

	public static System.Collections.Generic.List<System.Func<CardModel>> CreateBuildingCardFactories()
	{
		return new System.Collections.Generic.List<System.Func<CardModel>>
		{
			() => ModelDb.Card<AlliesBarracksCard>(),
			() => ModelDb.Card<AlliedWarFactory>(),
			() => ModelDb.Card<AlliedMCV>(),
			() => ModelDb.Card<PowerPlantCard>(),
			() => ModelDb.Card<AirForceCommand>(),
			() => ModelDb.Card<AlliedRefinery>(),
			() => ModelDb.Card<AlliedWallCard>(),
			() => ModelDb.Card<FortifiedWall>(),
			() => ModelDb.Card<AlliesShipyardCard>(),
			() => ModelDb.Card<AlliedBattleLab>(),
			() => ModelDb.Card<ChronoSphere>(),
			() => ModelDb.Card<WeatherController>()
		};
	}

	public static System.Collections.Generic.List<System.Func<CardModel>> CreateDefenseTowerCardFactories()
	{
		return new System.Collections.Generic.List<System.Func<CardModel>>
		{
			() => ModelDb.Card<PrismTowerCard>(),
			() => ModelDb.Card<AlliesPillboxCard>(),
			() => ModelDb.Card<PatriotMissile>(),
			() => ModelDb.Card<GrandCannon>()
		};
	}

	/// <summary>高科技(T2)单位数值映射 - 需要作战实验室解锁</summary>
	public static System.Collections.Generic.Dictionary<string, CardValueStore.CardValues> CreateHighTechValuesMap()
	{
		return new System.Collections.Generic.Dictionary<string, CardValueStore.CardValues>
		{
			{ "MIRAGETANK", MirageTank },
			{ "PRISMTANK", PrismTank },
			{ "AIRCRAFTCARRIER", AircraftCarrier },
			{ "CHRONOSPHERE", ChronoSphere },
			{ "WEATHERCONTROLLER", WeatherController }
		};
	}

	/// <summary>建筑类型到模型的映射（避免反射错误）</summary>
	public static readonly System.Collections.Generic.Dictionary<System.Type, System.Func<CardModel>> BuildingModelMap = new()
	{
		{ typeof(PowerPlantCard), () => ModelDb.Card<PowerPlantCard>() },
		{ typeof(AlliedRefinery), () => ModelDb.Card<AlliedRefinery>() },
		{ typeof(AlliesBarracksCard), () => ModelDb.Card<AlliesBarracksCard>() },
		{ typeof(AlliedWarFactory), () => ModelDb.Card<AlliedWarFactory>() },
		{ typeof(AirForceCommand), () => ModelDb.Card<AirForceCommand>() },
		{ typeof(AlliesShipyardCard), () => ModelDb.Card<AlliesShipyardCard>() },
		{ typeof(PrismTowerCard), () => ModelDb.Card<PrismTowerCard>() },
		{ typeof(AlliedWallCard), () => ModelDb.Card<AlliedWallCard>() },
		{ typeof(AlliesPillboxCard), () => ModelDb.Card<AlliesPillboxCard>() },
		{ typeof(PatriotMissile), () => ModelDb.Card<PatriotMissile>() },
		{ typeof(AlliedBattleLab), () => ModelDb.Card<AlliedBattleLab>() },
		{ typeof(ChronoSphere), () => ModelDb.Card<ChronoSphere>() },
		{ typeof(WeatherController), () => ModelDb.Card<WeatherController>() }
	};
	
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
		
		foreach (var kvp in CreateHighTechValuesMap())
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
			return values.BuildCost > 0 ? values.BuildCost : (int)values.DollarValue;
		}
		
		return 0;
	}
}
