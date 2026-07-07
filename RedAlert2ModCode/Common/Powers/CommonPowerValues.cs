using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public static class CommonPowerValues
{
	/// <summary>刀乐能力 - 资金存储能力</summary>
	public static CardValueStore.CardValues DollarPower => new();

	/// <summary>油井能力 - 回合开始时获得资金</summary>
	public static CardValueStore.CardValues OilDerrickPower => new()
	{
		DollarValue = 200,        // 基础每回合资金
		DollarValueUpgraded = 300  // 升级后每回合 500 = 200 + 300
	};

	/// <summary>黄金矿能力 - 存储黄金矿储备</summary>
	public static CardValueStore.CardValues GoldMinePower => new()
	{
		DollarValue = 1000,        // 基础储备
		DollarValueUpgraded = 1000  // 升级后储备增加量
	};

	/// <summary>宝石矿能力 - 存储宝石矿储备</summary>
	public static CardValueStore.CardValues GemMinePower => new()
	{
		DollarValue = 5000,        // 基础储备
		DollarValueUpgraded = 5000  // 升级后储备增加量
	};

	/// <summary>黄金矿柱能力 - 存储金矿储备并每回合增加</summary>
	public static CardValueStore.CardValues GoldMineColumnPower => new()
	{
		DollarValue = 5000,        // 基础储备
		DollarValueUpgraded = 5000, // 升级后储备增加量
		Stars = 200                // 每回合增加的金矿储备
	};

	/// <summary>飞鹰机枪扫射能力 - 对目标锁定敌人造成伤害</summary>
	public static CardValueStore.CardValues EagleMachineGunPower => new()
	{
		Damage = 3,             // 基础伤害
		DamageUpgraded = 1,     // 升级后4 = 3 + 1
		Repeat = 4              // 攻击次数
	};

	/// <summary>大生产能力 - 降低单位训练成本</summary>
	public static CardValueStore.CardValues MassProductionPower => new()
	{
		Stars = 50              // 每层降低的价格
	};

	/// <summary>力场护盾能力 - 回合开始时失去能量</summary>
	public static CardValueStore.CardValues ForceFieldPower => new()
	{
		Damage = 3              // 每层回合开始失去的能量
	};
}