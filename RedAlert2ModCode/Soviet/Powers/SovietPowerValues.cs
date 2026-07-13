using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

public static class SovietPowerValues
{
	public static CardValueStore.CardValues DollarPower => new();

	public static CardValueStore.CardValues PowerPlantPower => new()
	{
		MagicNumber = 10,
		MagicNumberUpgraded = -3
	};

	public static CardValueStore.CardValues TeslaCoilPower => new()
	{
		Damage = 5,
		Repeat = 1,
		Stars = 3,
		StarsUpgraded = 3
	};

	public static CardValueStore.CardValues TeslaCoilChargePower => new()
	{
		Repeat = 3    // 最大充能层数
	};

	public static CardValueStore.CardValues FlakTowerPower => new()
	{
		Block = 9,
		BlockUpgraded = 3
	};

	public static CardValueStore.CardValues FlakCannonPower => new()
	{
		Block = 2,           // 每个攻击意图敌人获得的格挡
		BlockUpgraded = 1    // 升级后3 = 2 + 1
	};

	public static CardValueStore.CardValues RepairDepotPower => new()
	{
		DollarValue = 1000
	};

	public static CardValueStore.CardValues TrainingQueuePower => new();

	public static CardValueStore.CardValues OilDerrickPower => new()
	{
		DollarValue = 200,
		DollarValueUpgraded = 300
	};

	public static CardValueStore.CardValues MassProductionPower => new()
	{
		Stars = 100
	};

	public static CardValueStore.CardValues GoldMinePower => new()
	{
		DollarValue = 1000,
		DollarValueUpgraded = 1000
	};

	public static CardValueStore.CardValues GemMinePower => new()
	{
		DollarValue = 5000,
		DollarValueUpgraded = 5000
	};

	public static CardValueStore.CardValues TerrorDronePower => new()
	{
		Damage = 1    // 每层每回合造成的伤害
	};

	public static CardValueStore.CardValues KirovPower => new()
	{
		Damage = 20,
		Stars = 3
	};

	public static CardValueStore.CardValues V3RocketPower => new()
	{
		Damage = 12,
		DamageUpgraded = 3
	};

	public static CardValueStore.CardValues DreadnoughtPower => new()
	{
		Damage = 12,
		DamageUpgraded = 3,
		Repeat = 2
	};
}