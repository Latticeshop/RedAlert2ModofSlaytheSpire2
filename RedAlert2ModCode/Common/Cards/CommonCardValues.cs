using System;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public static class CommonCardValues
{
	/// <summary>油井 - 1费能力卡，立即获得$1000，回合开始时获得$200资金（升级后$500）</summary>
	public static CardValueStore.CardValues OilDerrick => new()
	{
		Cost = 1,
		DollarValue = 1000,        // 立即获得的资金
		Damage = 200,              // 基础每回合资金
		DamageUpgraded = 300       // 升级后每回合 500 = 200 + 300
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

	/// <summary>飞鹰机枪扫射 - 1费攻击卡，绝地战备</summary>
	public static CardValueStore.CardValues EagleMachineGun => new()
	{
		Cost = 1,
		Damage = 3,             // 基础伤害
		DamageUpgraded = 1,     // 升级后4 = 3 + 1
		Repeat = 5              // 攻击次数
	};

	/// <summary>飞鹰空袭 - 1费攻击卡，绝地战备，对全体敌人造成8伤害（升级12伤害）</summary>
	public static CardValueStore.CardValues EagleAirStrike => new()
	{
		Cost = 1,
		Damage = 10,             // 基础伤害
		DamageUpgraded = 5,     // 升级后12 = 8 + 4
		Repeat = 1
	};

	/// <summary>飞鹰500kg - 3费攻击卡，绝地战备，指定敌人获得目标锁定</summary>
	public static CardValueStore.CardValues Eagle500kg => new()
	{
		Cost = 3,
		CostUpgraded = -1,      // 升级后费用减1
		Repeat = 1
	};

	/// <summary>飞鹰烟雾 - 1费攻击卡，绝地战备，对目标施加虚弱并给我方全体格挡</summary>
	public static CardValueStore.CardValues EagleSmokeStrike => new()
	{
		Cost = 1,
		MagicNumber = 1,             // 基础虚弱层数
		MagicNumberUpgraded = 1,     // 升级后2 = 1 + 1
		Block = 16,                  // 基础格挡
		BlockUpgraded = 4            // 升级后20 = 16 + 4
	};

	/// <summary>伞兵 - 1费攻击卡，将6张美国大兵加入手牌（升级后去掉消耗）</summary>
	public static CardValueStore.CardValues Paratrooper => new()
	{
		Cost = 1,
		Repeat = 6              // 添加6张美国大兵
	};

	public static CardValueStore.CardValues SellMCV => new()
	{
		Cost = 1,
		DollarValue = 3000
	};

	public static CardValueStore.CardValues Ra2Rally => new()
	{
		Cost = 1,
		MagicNumber = 4,           // 召集单位卡数量
		MagicNumberUpgraded = 2    // 升级后增加2张
	};

	public static CardValueStore.CardValues StopProduction => new()
	{
		Cost = 1,
		Repeat = 2                  // 未升级时选择数量
	};

	public static CardValueStore.CardValues Kiting => new()
	{
		Cost = 0,
		Damage = 3,                 // 基础格挡点数
		DamageUpgraded = 2          // 升级后5 = 3 + 2
	};

	/// <summary>扰矿 - 0费技能卡，从牌堆中抽取1张矿车卡牌（升级后2张）</summary>
	public static CardValueStore.CardValues MineRaid => new()
	{
		Cost = 0,
		MagicNumber = 1,            // 基础抽取数量
		MagicNumberUpgraded = 1     // 升级后增加1张（共2张）
	};

	/// <summary>大生产 - 3费能力卡，单位价格减少100（升级后每有一层生产序列减少100）</summary>
	public static CardValueStore.CardValues MassProduction => new()
	{
		Cost = 3,
		CostUpgraded = 0,           // 升级后费用不变（仍为3费）
		Stars = 100,                // 每层减少的价格（未升级）
		StarsUpgraded = 50          // 升级后额外减少的价格（总减少150）
	};

	/// <summary>F2A钢铁洪流 - 1费能力卡（升级后0费），手牌中的单位卡将自动打出</summary>
	public static CardValueStore.CardValues F2A => new()
	{
		Cost = 1,
		CostUpgraded = -1           // 升级后费用减1
	};

	/// <summary>力场护盾 - 1费技能卡（升级后0费），获得1层无实体，下回合失去3点能量</summary>
	public static CardValueStore.CardValues ForceField => new()
	{
		Cost = 1,
		CostUpgraded = -1           // 升级后费用减1
	};

	/// <summary>出售 - 1费技能卡，出售0-3个建筑获得50%造价资金（升级后可出售更多）</summary>
	public static CardValueStore.CardValues SellBuilding => new()
	{
		Cost = 1,
		Repeat = 3,                // 最大出售建筑数量
		RepeatUpgraded = 0         // 升级后最大数量不变，改为"任意"数量
	};

	/// <summary>尤里 - 0费技能卡，花费800资金获得一张随机带消耗的T2(升级T3)单位卡牌</summary>
	public static CardValueStore.CardValues Yuri => new()
	{
		Cost = 0,
		DollarValue = 800
	};

	/// <summary>尤里改 - 1费技能卡，token，获得10张随机带消耗的T1/T2(升级T3)不同单位卡牌</summary>
	public static CardValueStore.CardValues YuriPrime => new()
	{
		Cost = 1,
		MagicNumber = 10
	};

	/// <summary>支援 - 0费技能卡，仅多人模式，选择手牌中3(升级5)张单位卡送给队友</summary>
	public static CardValueStore.CardValues Support => new()
	{
		Cost = 0,
		MagicNumber = 3,
		MagicNumberUpgraded = 2
	};

	/// <summary>超时空伊文 - 1费攻击卡，Token，赋予3(升级2)层定时炸弹，部署给单位卡牌添加消耗并获得3(升级5)活力，超时空效果，价格1000</summary>
	public static CardValueStore.CardValues ChronoIvan => new()
	{
		Cost = 1,
		Damage = 3,
		DamageUpgraded = -1,
		MagicNumber = 5,
		DollarValue = 1000,
		DeployVigor = 3,
		DeployVigorUpgraded = 2
	};

	/// <summary>心灵突击队 - 1费攻击卡，Token，攻击意图获得随机单位卡，否则造成15(升级20)伤害，价格1000，渗透单位</summary>
	public static CardValueStore.CardValues PsiCommando => new()
	{
		Cost = 1,
		MagicNumber = 15,
		MagicNumberUpgraded = 5,
		DollarValue = 1000
	};

	/// <summary>超时空突击队 - 1费攻击卡，造成2(升级3)点伤害5次，部署对非攻击意图敌人造成20(升级25)伤害，价格2000</summary>
	public static CardValueStore.CardValues ChronoCommandos => new()
	{
		Cost = 1,
		Damage = 2,
		DamageUpgraded = 1,
		Repeat = 5,
		MagicNumber = 20,
		MagicNumberUpgraded = 5,
		DollarValue = 2000
	};

	/// <summary>城市化 - 3费能力卡，每打出一张建筑或防御塔牌，抽取一张建筑或防御塔牌（升级添加固有词条）</summary>
	public static CardValueStore.CardValues Urbanization => new()
	{
		Cost = 3,
		CostUpgraded = 0,        // 升级后添加固有词条（费用不变）
		Damage = 1,              // 每次抽取的数量
		DamageUpgraded = 0       // 升级后单次抽取数量不变
	};

	#region 箱子卡牌

	/// <summary>金钱箱子 - 1费(升级0费)，获得随机$2000-5000</summary>
	public static CardValueStore.CardValues MoneyCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1,
		DollarValue = 3500  // 基础随机值中间值
	};

	/// <summary>车辆箱子 - 0费(升级固有)，获得随机装甲单位卡</summary>
	public static CardValueStore.CardValues VehicleCrate => new()
	{
		Cost = 0,
		CostUpgraded = 0
	};

	/// <summary>回血箱子 - 1费(升级0费)，全体回5血</summary>
	public static CardValueStore.CardValues HealCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1,
		Damage = 5  // 回血量
	};

	/// <summary>火力箱子 - 1费(升级0费)，本回合单位卡伤害+50%</summary>
	public static CardValueStore.CardValues FirepowerCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};

	/// <summary>移速箱子 - 1费(升级0费)，获得2敏捷</summary>
	public static CardValueStore.CardValues SpeedCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1,
		MagicNumber = 2  // 敏捷层数
	};

	/// <summary>装甲箱子 - 1费(升级0费)，本回合单位卡格挡翻倍</summary>
	public static CardValueStore.CardValues ArmorCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};

	/// <summary>升级箱子 - 0费(升级保留)，升级手牌单位卡</summary>
	public static CardValueStore.CardValues UpgradeCrate => new()
	{
		Cost = 0,
		CostUpgraded = 0
	};

	/// <summary>随机箱子 - 1费，随机获得一张箱子卡</summary>
	public static CardValueStore.CardValues RandomCrate => new()
	{
		Cost = 1
	};

	/// <summary>隐身箱子 - 1费(升级0费)，获得伪装效果(Token)</summary>
	public static CardValueStore.CardValues StealthCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};

	/// <summary>爆炸箱子 - 0费(升级0费)，对自己造成5(升级10)伤害(Token)</summary>
	public static CardValueStore.CardValues ExplosionCrate => new()
	{
		Cost = 0,
		CostUpgraded = 0,
		Damage = 5,
		DamageUpgraded = 5  // 升级后10 = 5 + 5
	};

	/// <summary>超武箱子 - 1费(升级0费)，随机获得0费超武(Token)</summary>
	public static CardValueStore.CardValues SuperWeaponCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};

	/// <summary>矿石箱子 - 1费(升级0费)，随机获得矿区卡(Token)</summary>
	public static CardValueStore.CardValues OreCrate => new()
	{
		Cost = 1,
		CostUpgraded = -1
	};

	#endregion

	private static Dictionary<Type, decimal> _sellablePowerDollarMap;

	public static Dictionary<Type, decimal> GetSellablePowerDollarMap()
	{
		if (_sellablePowerDollarMap == null)
		{
			_sellablePowerDollarMap = new Dictionary<Type, decimal>();
			foreach (var kvp in AlliesCardValues.CreateSellablePowerDollarMap())
			{
				_sellablePowerDollarMap[kvp.Key] = kvp.Value;
			}
			foreach (var kvp in SovietCardValues.CreateSellablePowerDollarMap())
			{
				_sellablePowerDollarMap[kvp.Key] = kvp.Value;
			}
		}
		return _sellablePowerDollarMap;
	}

	public static int GetSellablePowerDollarValue(Type powerType)
	{
		var map = GetSellablePowerDollarMap();
		if (map.TryGetValue(powerType, out var value))
		{
			return (int)value;
		}
		return 500;
	}

	public static IEnumerable<Type> GetSellablePowerTypes()
	{
		return GetSellablePowerDollarMap().Keys;
	}
}