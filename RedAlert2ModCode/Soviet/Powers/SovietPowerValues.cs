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
		Stars = 2,
		StarsUpgraded = 3
	};

	public static CardValueStore.CardValues FlakTowerPower => new()
	{
		Block = 9,
		BlockUpgraded = 3
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
}