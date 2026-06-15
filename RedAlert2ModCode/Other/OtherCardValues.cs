using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Other;

/// <summary>
/// 其他阵营卡牌数值存储
/// 包含利赛特、古巴等特殊阵营的卡牌数值
/// </summary>
public static class OtherCardValues
{
	// ==================== 利赛特阵营 ====================
	
	/// <summary>狙击手</summary>
	public static CardValueStore.CardValues Sniper => new()
	{
		Damage = 10,
		DamageUpgraded = 5,
		Repeat = 1
	};
	
	// ==================== 古巴阵营 ====================
	
	/// <summary>恐怖分子</summary>
	public static CardValueStore.CardValues Terrorist => new()
	{
		Damage = 12,
		DamageUpgraded = 6,
		Repeat = 1
	};
	
	// ==================== 伊拉克阵营 ====================
	
	/// <summary>辐射工兵</summary>
	public static CardValueStore.CardValues Desolator => new()
	{
		Damage = 4,
		DamageUpgraded = 2,
		Repeat = 2
	};
	
	// ==================== 利比亚阵营 ====================
	
	/// <summary>自爆卡车</summary>
	public static CardValueStore.CardValues DemolitionTruck => new()
	{
		Damage = 15,
		DamageUpgraded = 8,
		Repeat = 1
	};
	
	// ==================== 通用卡牌 ====================
	
	/// <summary>工程师（通用）</summary>
	public static CardValueStore.CardValues Engineer => new()
	{
		Block = 6,
		BlockUpgraded = 3
	};
	
	/// <summary>间谍</summary>
	public static CardValueStore.CardValues Spy => new()
	{
		MagicNumber = 1,  // 偷取能量数
		MagicNumberUpgraded = 1
	};
}
