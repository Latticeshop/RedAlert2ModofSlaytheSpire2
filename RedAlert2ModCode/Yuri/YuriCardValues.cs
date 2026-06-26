using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Yuri;

/// <summary>
/// 尤里阵营卡牌数值存储
/// 统一管理所有尤里卡牌的数值，便于本地化和平衡调整
/// </summary>
public static class YuriCardValues
{
	// ==================== 士兵单位 ====================
	
	/// <summary>尤里新兵</summary>
	public static CardValueStore.CardValues YuriInitiate => new()
	{
		Damage = 3,
		DamageUpgraded = 2,
		Repeat = 2
	};
	
	/// <summary>狂兽人</summary>
	public static CardValueStore.CardValues Brute => new()
	{
		Damage = 8,
		DamageUpgraded = 4,
		Repeat = 1
	};
	
	/// <summary>心灵突击队</summary>
	public static CardValueStore.CardValues PsiCommando => new()
	{
		Damage = 5,
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
	
	/// <summary>狂风坦克</summary>
	public static CardValueStore.CardValues LasherTank => new()
	{
		Damage = 5,
		DamageUpgraded = 3,
		Block = 4,
		BlockUpgraded = 2
	};
	
	/// <summary>盖特机炮坦克</summary>
	public static CardValueStore.CardValues GatlingTank => new()
	{
		Damage = 3,
		DamageUpgraded = 1,
		Repeat = 2
	};
	
	// ==================== 空军单位 ====================
	
	/// <summary>镭射幽浮</summary>
	public static CardValueStore.CardValues FloatingDisk => new()
	{
		Damage = 7,
		DamageUpgraded = 3,
		Repeat = 1
	};
	
	// ==================== 建筑卡牌 ====================
	
	/// <summary>兵营</summary>
	public static CardValueStore.CardValues Barracks => new()
	{
		// 兵营主要是功能牌
	};
	
	/// <summary>尤里重工</summary>
	public static CardValueStore.CardValues YuriWarFactory => new()
	{
		// 重工主要是功能牌
	};
	
	/// <summary>心灵探测器</summary>
	public static CardValueStore.CardValues PsychicRadar => new()
	{
		MagicNumber = 8,
		MagicNumberUpgraded = -2
	};
	
	/// <summary>矿场</summary>
	public static CardValueStore.CardValues YuriRefinery => new()
	{
		// 矿场主要是功能牌
	};
	
	/// <summary>尤里基地车</summary>
	public static CardValueStore.CardValues YuriMCV => new()
	{
		// 基地车主要是功能牌
	};
	
	// ==================== 防御建筑 ====================
	
	/// <summary>尤里围墙</summary>
	public static CardValueStore.CardValues YuriWall => new()
	{
		Block = 5,
		BlockUpgraded = 3
	};
	
	/// <summary>心灵控制塔</summary>
	public static CardValueStore.CardValues PsychicTower => new()
	{
		Damage = 2,
		DamageUpgraded = 2,
		Repeat = 1
	};
	
	// ==================== 经济单位 ====================
	
	/// <summary>奴隶矿车</summary>
	public static CardValueStore.CardValues SlaveMiner => new()
	{
		DollarValue = 55,
		DollarValueUpgraded = 25
	};
}
