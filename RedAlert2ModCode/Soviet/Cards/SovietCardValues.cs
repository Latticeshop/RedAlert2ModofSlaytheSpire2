using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军阵营卡牌数值存储
/// 统一管理所有苏军卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class SovietCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>动员兵 - 0费3伤害1次，升级后5伤害1次，价格100</summary>
	public static CardValueStore.CardValues Conscript => new()
	{
		Cost = 0,
		Damage = 3,
		DamageUpgraded = 2,
		Repeat = 1,
		DollarValue = 100
	};
	
	/// <summary>防空步兵 - 1费攻击卡，每有一个攻击意图敌人获得3格挡（升级5），价格300</summary>
	public static CardValueStore.CardValues FlakTrooper => new()
	{
		Cost = 1,
		Block = 3,
		BlockUpgraded = 2,
		DollarValue = 300
	};

	/// <summary>海蝎 - 1费攻击卡，每有一个攻击意图敌人获得5格挡（升级8），价格500</summary>
	public static CardValueStore.CardValues FlakSubmarine => new()
	{
		Cost = 1,
		Block = 5,
		BlockUpgraded = 3,
		DollarValue = 500
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
	
	/// <summary>磁暴步兵 - 1费，生成闪电球，部署给磁暴线圈充能，价格500</summary>
	public static CardValueStore.CardValues TeslaTrooper => new()
	{
		Cost = 1,
		DollarValue = 500
	};

	/// <summary>辐射工兵 - 1费攻击卡，对一名敌人赋予8(升级10)层中毒，部署对全体敌人赋予4(升级5)层中毒，价格600</summary>
	public static CardValueStore.CardValues Desolator => new()
	{
		Cost = 1,
		Damage = 8,
		DamageUpgraded = 2,
		Repeat = 4,
		RepeatUpgraded = 1,
		DollarValue = 600
	};
	
	/// <summary>恐怖分子 - 1费攻击卡，造成6(升级9)伤害+溅射，价格200，需要雷达(T2)</summary>
	public static CardValueStore.CardValues Terrorist => new()
	{
		Cost = 0,
		Damage = 6,
		DamageUpgraded = 3,
		DollarValue = 200
	};

	/// <summary>疯狂伊文 - 1费攻击卡，Token，赋予3(升级2)层定时炸弹，部署给单位卡牌添加消耗并获得3(升级5)活力，价格600，需要雷达/空指部(T2)</summary>
	public static CardValueStore.CardValues CrazyIvan => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = -1,
		MagicNumber = 5,
		DollarValue = 600,
		DeployVigor = 3,
		DeployVigorUpgraded = 2
	};

	/// <summary>超时空伊文 - 1费攻击卡，Token，赋予3(升级2)层定时炸弹，部署给单位卡牌添加消耗并获得3(升级5)活力，超时空效果，价格1000，需要超时空伊文遗物</summary>
	public static CardValueStore.CardValues ChronoIvan => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 2,
		MagicNumber = 5,
		DollarValue = 1000,
		DeployVigor = 3,
		DeployVigorUpgraded = 2
	};
	
	/// <summary>工程师 - 1费技能卡，从选项中选择指令，价格500</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Cost = 1,
		Block = 6,
		BlockUpgraded = 3,
		DollarValue = 500
	};

	/// <summary>侦察机 - 0费攻击卡，获得临时敏捷，价格0（特殊单位，无法生产）</summary>
	public static CardValueStore.CardValues SpyPlane => new()
	{
		Cost = 0,
		MagicNumber = 3,
		MagicNumberUpgraded = 2,
		DollarValue = 0
	};

	/// <summary>尤里新兵 - 1费攻击卡，Token，赋予3(升级4)层灼烧，价格200</summary>
	public static CardValueStore.CardValues YuriSoldier => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 1,
		DollarValue = 200
	};
	
	// ==================== 装甲单位 ====================
	
	/// <summary>犀牛坦克 - 1费3攻击6防御+1易伤，升级后5攻击9防御+1易伤，价格900</summary>
	public static CardValueStore.CardValues RhinoTank => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = 2,
		Block = 6,
		BlockUpgraded = 3,
		MagicNumber = 1,      // 易伤层数
		MagicNumberUpgraded = 0,  // 升级后不增加易伤层数
		DollarValue = 900
	};

	/// <summary>磁能坦克 - 1费攻击卡，获得5(升级8)格挡+1充能球槽+1(升级2)闪电球，价格1200</summary>
	public static CardValueStore.CardValues TeslaTank => new()
	{
		Cost = 1,
		Block = 5,
		BlockUpgraded = 3,
		MagicNumber = 1,      // 闪电球数量
		MagicNumberUpgraded = 1,  // 升级后+1闪电球
		DollarValue = 1200
	};
	
	/// <summary>天启坦克 - 2费5伤害2次10防御+1易伤，升级后7伤害2次12防御+2易伤，价格1750</summary>
	public static CardValueStore.CardValues ApocalypseTank => new()
	{
		Cost = 2,
		Damage = 5,
		DamageUpgraded = 2,
		Repeat = 2,
		RepeatUpgraded = 0,
		Block = 10,
		BlockUpgraded = 2,
		MagicNumber = 1,      // 易伤层数
		MagicNumberUpgraded = 1,
		DollarValue = 1750
	};
	
	/// <summary>防空履带车 - 0费，抽2/3张牌，弃0-2/3张牌，部署：存储士兵单位，价格500</summary>
	public static CardValueStore.CardValues FlakTrack => new()
	{
		Cost = 0,
		MagicNumber = 2,           // 抽牌数
		MagicNumberUpgraded = 1,   // 升级后3 = 2 + 1
		Stars = 2,                 // 弃牌数上限
		StarsUpgraded = 1,         // 升级后3 = 2 + 1
		DollarValue = 500
	};
	
	/// <summary>台风级潜艇 - 1费攻击卡，给予1层易伤，造成4伤害（升级7）2次，价格1000</summary>
	public static CardValueStore.CardValues TyphoonSubmarine => new()
	{
		Cost = 1,
		Damage = 4,                // 基础伤害
		DamageUpgraded = 3,        // 升级后7 = 4 + 3
		DollarValue = 1000
	};
	
	/// <summary>恐怖机器人 - 1费攻击卡，赋予恐怖机器人+缓慢，价格500</summary>
	public static CardValueStore.CardValues TerrorDrone => new()
	{
		Cost = 1,
		MagicNumber = 1,           // 恐怖机器人层数
		MagicNumberUpgraded = 1,   // 升级后2 = 1 + 1
		DollarValue = 500
	};

	/// <summary>巨型乌贼 - 1费攻击卡，赋予1(升级2)层虚弱，赋予3(升级5)层巨型乌贼，价格1000</summary>
	public static CardValueStore.CardValues GiantSquid => new()
	{
		Cost = 1,
		MagicNumber = 3,           // 巨型乌贼层数
		MagicNumberUpgraded = 2,   // 升级后5 = 3 + 2
		DollarValue = 1000
	};
	
	/// <summary>苏军基地车 - 0费，价格3000</summary>
	public static CardValueStore.CardValues SovietMCV => new()
	{
		Cost = 0,
		DollarValue = 3000
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>基洛夫飞艇 - 3费，赋予基洛夫debuff，每回合造成20伤害（升级30），价格2000</summary>
	public static CardValueStore.CardValues Kirov => new()
	{
		Cost = 3,
		Damage = 20,
		DamageUpgraded = 10,
		Repeat = 1,
		DollarValue = 2000
	};

	/// <summary>V3火箭 - 1费，赋予目标锁定和V3火箭能力，每回合造成12伤害（升级15），价格800</summary>
	public static CardValueStore.CardValues V3Rocket => new()
	{
		Cost = 1,
		Damage = 12,
		DamageUpgraded = 3,
		DollarValue = 800
	};

	/// <summary>无畏级战舰 - 2费攻击卡，赋予目标锁定和2层V3火箭能力（升级后伤害15），价格1200</summary>
	public static CardValueStore.CardValues Dreadnought => new()
	{
		Cost = 2,
		Damage = 12,
		DamageUpgraded = 3,
		Repeat = 2,
		DollarValue = 1200
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>苏军兵营 - 0费，价格500</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		Cost = 0,
		DollarValue = 500
	};
	
	/// <summary>苏军重工 - 0费能力卡，价格2000</summary>
	public static CardValueStore.CardValues SovietWarFactory => new()
	{
		Cost = 0,
		DollarValue = 2000
	};
	
	/// <summary>苏军船厂 - 0费，价格1000</summary>
	public static CardValueStore.CardValues Shipyard => new()
	{
		Cost = 0,
		DollarValue = 1000
	};
	
	/// <summary>苏军维修厂 - 2费能力卡（升级后1费），价格800</summary>
	public static CardValueStore.CardValues RepairDepot => new()
	{
		Cost = 2,
		CostUpgraded = 1,
		DollarValue = 800
	};
	
	/// <summary>苏军哨戒炮 - 1费，回合开始时对敌人造成1伤害2次（升级后2伤害），获得3防御，价格500</summary>
	public static CardValueStore.CardValues SovietPillbox => new()
	{
		Cost = 1,
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
	
	/// <summary>防空炮 - 1费技能卡，回合开始时每有一个攻击意图敌人获得4格挡（升级6），价格1000</summary>
	public static CardValueStore.CardValues FlakCannon => new()
	{
		Cost = 1,
		Block = 4,
		BlockUpgraded = 2,  // 升级后6 = 4 + 2
		DollarValue = 1000
	};
	
	/// <summary>矿场 - 0费，价格2000</summary>
	public static CardValueStore.CardValues SovietRefinery => new()
	{
		Cost = 0,
		DollarValue = 2000
	};
	
	/// <summary>苏军作战实验室 - 0费，价格2000（升级后1000）</summary>
	public static CardValueStore.CardValues SovietBattleLab => new()
	{
		Cost = 0,
		DollarValue = 2000,
		DollarValueUpgraded = 1000
	};

	/// <summary>核电站 - 0费能力卡，每回合获得3能量（升级5），受到10点（升级15点）未格挡伤害爆炸，价格1000</summary>
	public static CardValueStore.CardValues NuclearPlant => new()
	{
		Cost = 0,
		MagicNumber = 3,           // 基础能量获取
		MagicNumberUpgraded = 2,   // 升级后+2，总共5
		Damage = 10,               // 基础爆炸阈值
		DamageUpgraded = 5,        // 升级后+5，总共15
		Repeat = 4,                // 爆炸时赋予中毒层数
		DollarValue = 1000
	};

	/// <summary>工业工厂 - 0费能力卡，-25%单位造价（升级后-40%），价格1200</summary>
	public static CardValueStore.CardValues IndustrialPlant => new()
	{
		Cost = 0,
		DollarValue = 2500,
		MagicNumber = 25,           // 基础-25%单位造价
		MagicNumberUpgraded = 15    // 升级后增加15%，总共-40%
	};

	/// <summary>雷达 - 0费能力卡，价格1000（升级后500），解锁苏联空军和轨道战备</summary>
	public static CardValueStore.CardValues Radar => new()
	{
		Cost = 0,
		DollarValue = 1000,
		DollarValueUpgraded = 500
	};
	
	// ==================== 防御建筑 ====================
	
	/// <summary>苏军围墙 - 0费1护盾，升级后3护盾，价格100</summary>
	public static CardValueStore.CardValues SovietWall => new()
	{
		Cost = 0,
		Block = 1,
		BlockUpgraded = 2,
		DollarValue = 100
	};

	/// <summary>苏军坚固围墙 - 0费3护盾（升级后5护盾），花费100资金，价格100</summary>
	public static CardValueStore.CardValues SovietFortifiedWall => new()
	{
		Cost = 0,
		Block = 3,
		BlockUpgraded = 2,
		DollarValue = 100
	};
	
	/// <summary>磁暴线圈 - 2费，回合开始时对敌人造成5伤害（升级8），价格1500</summary>
	public static CardValueStore.CardValues TeslaCoilCard => new()
	{
		Cost = 2,
		Damage = 5,
		Stars = 8,
		DollarValue = 1500
	};

	/// <summary>战斗碉堡 - 2费技能卡，金卡，选择3张士兵卡牌驻扎（升级6张），价格500</summary>
	public static CardValueStore.CardValues BattleBunker => new()
	{
		Cost = 2,
		MagicNumber = 3,
		MagicNumberUpgraded = 3,
		DollarValue = 500
	};

	/// <summary>自爆卡车 - 1费攻击卡，Token，对全体敌人造成5伤害和10层中毒（升级15层），价格1500</summary>
	public static CardValueStore.CardValues DemolitionTruck => new()
	{
		Cost = 0,
		Damage = 5,
		MagicNumber = 10,
		MagicNumberUpgraded = 5,
		DollarValue = 1500
	};
	
	/// <summary>铁幕装置 - 0费能力卡，金卡，价格2500，每4回合（升级后3回合）获得一张虚无铁幕卡</summary>
	public static CardValueStore.CardValues IronCurtainCard => new()
	{
		Cost = 0,
		DollarValue = 2500,
		Repeat = 4,
		RepeatUpgraded = 3
	};
	
	/// <summary>核弹井 - 0费能力卡，金卡，价格5000，每4回合获得一张虚无核弹攻击卡</summary>
	public static CardValueStore.CardValues NuclearMissileSiloCard => new()
	{
		Cost = 0,
		DollarValue = 5000,
		Repeat = 4,
		RepeatUpgraded = 4
	};
	
	// ==================== 超级武器运转卡 ====================
	
	/// <summary>铁幕 - 1费技能卡（升级0费），金卡，消耗，获得一层无实体</summary>
	public static CardValueStore.CardValues IronCurtain => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};
	
	/// <summary>核弹攻击 - 3费技能卡（升级后伤害提升），金卡，消耗，对全部敌人造成50伤害（升级80），赋予25层中毒</summary>
	public static CardValueStore.CardValues NuclearAttack => new()
	{
		Cost = 3,
		CostUpgraded = 0,
		Damage = 50,
		DamageUpgraded = 30,
		MagicNumber = 25
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>武装采矿车 - 0费攻击造成2点伤害（升级后全体），获得1000资金（升级后1500），价格1400</summary>
	public static CardValueStore.CardValues WarMiner => new()
	{
		Cost = 0,
		Damage = 2,
		DamageUpgraded = 0,
		DollarValue = 1000,
		DollarValueUpgraded = 500,
		BuildCost = 1400
	};

	/// <summary>提前倒矿 - 1费技能卡，抽取所有矿车，本回合矿车收益为80%</summary>
	public static CardValueStore.CardValues EarlyMining => new()
	{
		Cost = 1,
		MagicNumber = 80
	};
	
	/// <summary>苏联运输船 - 1费技能卡，存储最多3张手牌（升级后5张），获得7格挡（升级10），价格900</summary>
	public static CardValueStore.CardValues SovietTransportShip => new()
	{
		Cost = 1,
		MagicNumber = 3,
		MagicNumberUpgraded = 2,
		Block = 7,
		BlockUpgraded = 3,
		DollarValue = 900
	};
	
	// ==================== 数值映射创建方法 ====================
	
	public static Dictionary<string, CardValueStore.CardValues> CreateSoldierValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "CONSCRIPT", Conscript },
			{ "SOVIETATTACKDOG", AttackDog },
			{ "SOVIETFLAKTROOPER", FlakTrooper },
			{ "SOVIETTESLATROOPER", TeslaTrooper },
			{ "DESOLATOR", Desolator },
			{ "SOVIETENGINEER", Engineer },
			{ "TERROR_MAN", Terrorist },
			{ "CRAZY_IVAN_CARD", CrazyIvan },
			{ "CHRONO_IVAN_CARD", CommonCardValues.ChronoIvan },
			{ "PSICOMMANDOCARD", CommonCardValues.PsiCommando }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateVehicleValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "RHINOTANK", RhinoTank },
			{ "APOCALYPSETANK", ApocalypseTank },
			{ "FLAKTRACK", FlakTrack },
			{ "TERRORDRONE", TerrorDrone },
			{ "SOVIETMCV", SovietMCV },
			{ "WARMINER", WarMiner },
			{ "V3ROCKET", V3Rocket },
			{ "DEMOLITIONTRUCKCARD", DemolitionTruck },
			{ "TESLATANK", TeslaTank },
			{ "KIROV", Kirov }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateAircraftValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "KIROV", Kirov },
			{ "SPY_PLANE", SpyPlane }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateShipValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "SOVIETTRANSPORTSHIP", SovietTransportShip },
			{ "FLAKSUBMARINE", FlakSubmarine },
			{ "TYPHOONSUBMARINE", TyphoonSubmarine },
			{ "DREADNOUGHT", Dreadnought },
			{ "GIANTSQUID", GiantSquid }
		};
	}
	
	public static Dictionary<string, CardValueStore.CardValues> CreateBuildingValuesMap()
	{
		return new Dictionary<string, CardValueStore.CardValues>
		{
			{ "SOVIETBARRACKSCARD", Barracks },
			{ "SOVIETWARFACTORY", SovietWarFactory },
			{ "SOVIETSHIPYARDCARD", Shipyard },
			{ "REPAIRDEPOT", RepairDepot },
			{ "SOVIETPILLBOXCARD", SovietPillbox },
			{ "NUCLEARREACTOR", NuclearReactor },
			{ "SOVIETREFINERY", SovietRefinery },
			{ "SOVIETBATTLELAB", SovietBattleLab },
			{ "SOVIETTESLACOILCARD", TeslaCoilCard },
			{ "BATTLEBUNKERCARD", BattleBunker },
			{ "SOVIETWALLCARD", SovietWall },
			{ "SOVIETFORTIFIEDWALL", SovietFortifiedWall },
			{ "SOVIETRADAR", Radar },
			{ "IRONCURTAINCARD", IronCurtainCard },
			{ "NUCLEARMISSILESILOCARD", NuclearMissileSiloCard },
			{ "SOVIETFLAKCANNON", FlakCannon },
			{ "NUCLEARPLANTCARD", NuclearPlant },
			{ "INDUSTRIALPLANTCARD", IndustrialPlant }
		};
	}

	public static Dictionary<Type, decimal> CreateSellablePowerDollarMap()
	{
		return new Dictionary<Type, decimal>
		{
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietRefineryPower), SovietRefinery.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietWarFactoryPower), SovietWarFactory.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietBattleLabPower), SovietBattleLab.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietRadarPower), Radar.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietMCVPower), SovietMCV.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietBarracksPower), Barracks.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietShipyardPower), Shipyard.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietPillboxPower), SovietPillbox.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietTeslaCoilPower), TeslaCoilCard.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietFlakCannonPower), FlakCannon.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.BattleBunkerPower), BattleBunker.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietPowerPlantPower), NuclearReactor.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.IndustrialPlantPower), IndustrialPlant.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.IronCurtainPower), IronCurtainCard.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.NuclearMissileSiloPower), NuclearMissileSiloCard.DollarValue },
			{ typeof(RedAlert2ModCode.Soviet.Powers.SovietRepairDepotPower), RepairDepot.DollarValue }
		};
	}

	/// <summary>建筑类型到模型的映射（避免反射错误）</summary>
	public static readonly Dictionary<Type, Func<CardModel>> BuildingModelMap = new()
	{
		{ typeof(SovietBarracksCard), () => ModelDb.Card<SovietBarracksCard>() },
		{ typeof(SovietWarFactory), () => ModelDb.Card<SovietWarFactory>() },
		{ typeof(SovietShipyardCard), () => ModelDb.Card<SovietShipyardCard>() },
		{ typeof(SovietRepairDepot), () => ModelDb.Card<SovietRepairDepot>() },
		{ typeof(SovietPillboxCard), () => ModelDb.Card<SovietPillboxCard>() },
		{ typeof(SovietFlakCannon), () => ModelDb.Card<SovietFlakCannon>() },
		{ typeof(SovietWallCard), () => ModelDb.Card<SovietWallCard>() },
		{ typeof(SovietFortifiedWall), () => ModelDb.Card<SovietFortifiedWall>() },
		{ typeof(NuclearReactor), () => ModelDb.Card<NuclearReactor>() },
		{ typeof(SovietRefinery), () => ModelDb.Card<SovietRefinery>() },
		{ typeof(SovietBattleLab), () => ModelDb.Card<SovietBattleLab>() },
		{ typeof(SovietRadar), () => ModelDb.Card<SovietRadar>() },
		{ typeof(SovietTeslaCoilCard), () => ModelDb.Card<SovietTeslaCoilCard>() },
		{ typeof(BattleBunkerCard), () => ModelDb.Card<BattleBunkerCard>() },
		{ typeof(IronCurtainCard), () => ModelDb.Card<IronCurtainCard>() },
		{ typeof(NuclearMissileSiloCard), () => ModelDb.Card<NuclearMissileSiloCard>() },
		{ typeof(NuclearPlantCard), () => ModelDb.Card<NuclearPlantCard>() },
		{ typeof(IndustrialPlantCard), () => ModelDb.Card<IndustrialPlantCard>() }
	};

	public static List<Func<CardModel>> CreateBuildingCardFactories()
	{
		return new List<Func<CardModel>>
		{
			() => ModelDb.Card<SovietBarracksCard>(),
			() => ModelDb.Card<SovietWarFactory>(),
			() => ModelDb.Card<SovietShipyardCard>(),
			() => ModelDb.Card<SovietRepairDepot>(),
			() => ModelDb.Card<SovietPillboxCard>(),
			() => ModelDb.Card<SovietFlakCannon>(),
			() => ModelDb.Card<SovietWallCard>(),
			() => ModelDb.Card<SovietFortifiedWall>(),
			() => ModelDb.Card<NuclearReactor>(),
			() => ModelDb.Card<SovietRefinery>(),
			() => ModelDb.Card<SovietMCV>(),
			() => ModelDb.Card<SovietBattleLab>(),
			() => ModelDb.Card<SovietRadar>(),
			() => ModelDb.Card<SovietTeslaCoilCard>(),
			() => ModelDb.Card<BattleBunkerCard>(),
			() => ModelDb.Card<IronCurtainCard>(),
			() => ModelDb.Card<NuclearMissileSiloCard>(),
			() => ModelDb.Card<NuclearPlantCard>(),
			() => ModelDb.Card<IndustrialPlantCard>()
		};
	}

	public static List<Func<CardModel>> CreateDefenseTowerCardFactories()
	{
		return new List<Func<CardModel>>
		{
			() => ModelDb.Card<SovietPillboxCard>(),
			() => ModelDb.Card<SovietFlakCannon>(),
			() => ModelDb.Card<SovietTeslaCoilCard>(),
			() => ModelDb.Card<BattleBunkerCard>()
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
		
		var allValues = CreateAllValuesMap();
		
		string keyWithUnderscore = cardId.ToUpper();
		if (allValues.TryGetValue(keyWithUnderscore, out var values))
		{
			return values.BuildCost > 0 ? values.BuildCost : (int)values.DollarValue;
		}
		
		string keyWithoutUnderscore = keyWithUnderscore.Replace("_", "");
		if (allValues.TryGetValue(keyWithoutUnderscore, out values))
		{
			return values.BuildCost > 0 ? values.BuildCost : (int)values.DollarValue;
		}
		
		// 尝试提取卡牌名称部分（移除前缀如 RED_ALERT2_MOD_CARD_）
		string cardName = ExtractCardName(keyWithUnderscore);
		if (!string.IsNullOrEmpty(cardName))
		{
			if (allValues.TryGetValue(cardName, out values))
			{
				return values.BuildCost > 0 ? values.BuildCost : (int)values.DollarValue;
			}
			
			// 尝试移除下划线后的名称
			string cardNameNoUnderscore = cardName.Replace("_", "");
			if (allValues.TryGetValue(cardNameNoUnderscore, out values))
			{
				return values.BuildCost > 0 ? values.BuildCost : (int)values.DollarValue;
			}
		}
		
		return 0;
	}
	
	/// <summary>
	/// 从完整的卡牌ID中提取卡牌名称部分
	/// 例如：RED_ALERT2_MOD_CARD_DREADNOUGHT -> DREADNOUGHT
	/// </summary>
	private static string ExtractCardName(string cardKey)
	{
		// 移除前缀 RED_ALERT2_MOD_CARD_
		string prefix = "RED_ALERT2_MOD_CARD_";
		if (cardKey.StartsWith(prefix))
		{
			return cardKey.Substring(prefix.Length);
		}
		
		// 移除前缀 MOD_CARD_
		prefix = "MOD_CARD_";
		if (cardKey.StartsWith(prefix))
		{
			return cardKey.Substring(prefix.Length);
		}
		
		// 移除前缀 CARD_
		prefix = "CARD_";
		if (cardKey.StartsWith(prefix))
		{
			return cardKey.Substring(prefix.Length);
		}
		
		// 如果没有找到前缀，返回最后一个下划线之后的部分
		int lastUnderscoreIndex = cardKey.LastIndexOf('_');
		if (lastUnderscoreIndex >= 0 && lastUnderscoreIndex < cardKey.Length - 1)
		{
			return cardKey.Substring(lastUnderscoreIndex + 1);
		}
		
		return string.Empty;
	}
}
