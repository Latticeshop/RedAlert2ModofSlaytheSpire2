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
		Repeat = 4              // 攻击次数
	};
}