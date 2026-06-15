using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军阵营卡牌数值存储
/// 统一管理所有苏军卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class SovietCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>动员兵</summary>
	public static CardValueStore.CardValues Conscript => new()
	{
		Damage = 4,
		DamageUpgraded = 2,
		Repeat = 1
	};
	
	/// <summary>军犬</summary>
	public static CardValueStore.CardValues AttackDog => new()
	{
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 1,
		RepeatUpgraded = 1
	};
	
	/// <summary>磁暴步兵</summary>
	public static CardValueStore.CardValues TeslaTrooper => new()
	{
		Damage = 6,
		DamageUpgraded = 3,
		Repeat = 1
	};
	
	/// <summary>工程师</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Block = 6,
		BlockUpgraded = 3
	};
	
	// ==================== 装甲单位 ====================
	
	/// <summary>犀牛坦克</summary>
	public static CardValueStore.CardValues RhinoTank => new()
	{
		Damage = 6,
		DamageUpgraded = 3,
		Block = 4,
		BlockUpgraded = 2
	};
	
	/// <summary>防空履带车</summary>
	public static CardValueStore.CardValues FlakTrack => new()
	{
		Damage = 5,
		DamageUpgraded = 2,
		Repeat = 1
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>基洛夫飞艇</summary>
	public static CardValueStore.CardValues Kirov => new()
	{
		Damage = 10,
		DamageUpgraded = 5,
		Repeat = 1
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>兵营</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		// 兵营主要是功能牌
	};
	
	/// <summary>苏军重工</summary>
	public static CardValueStore.CardValues SovietWarFactory => new()
	{
		// 重工主要是功能牌
	};
	
	/// <summary>核电站</summary>
	public static CardValueStore.CardValues NuclearReactor => new()
	{
		MagicNumber = 5,  // 每5张牌获得1能量
		MagicNumberUpgraded = -1
	};
	
	/// <summary>矿场</summary>
	public static CardValueStore.CardValues SovietRefinery => new()
	{
		// 矿场主要是功能牌
	};
	
	/// <summary>苏军基地车</summary>
	public static CardValueStore.CardValues SovietMCV => new()
	{
		// 基地车主要是功能牌
	};
	
	// ==================== 防御建筑 ====================
	
	/// <summary>苏军围墙</summary>
	public static CardValueStore.CardValues SovietWall => new()
	{
		Block = 5,
		BlockUpgraded = 3
	};
	
	/// <summary>磁暴线圈</summary>
	public static CardValueStore.CardValues TeslaCoil => new()
	{
		Damage = 3,
		DamageUpgraded = 4,
		Repeat = 1
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>武装采矿车</summary>
	public static CardValueStore.CardValues WarMiner => new()
	{
		DollarValue = 60,
		DollarValueUpgraded = 30
	};
}
