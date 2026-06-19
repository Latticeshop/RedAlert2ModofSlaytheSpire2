using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 盟军阵营能力数值存储
/// 统一管理所有盟军能力的数值，便于本地化和平衡调整
/// </summary>
public static class AlliesPowerValues
{
	// ==================== 经济能力 ====================
	
	/// <summary>刀乐能力 - 资金存储能力</summary>
	public static CardValueStore.CardValues DollarPower => new()
	{
		// DollarValue 用于存储当前资金
	};
	
	// ==================== 建筑能力 ====================
	
	/// <summary>发电厂能力 - 每抽一定数量的牌获得能量</summary>
	public static CardValueStore.CardValues PowerPlantPower => new()
	{
		MagicNumber = 10,      // 未升级时的抽牌阈值
		MagicNumberUpgraded = -3  // 升级后阈值降低为 7 = 10 + (-3)
	};
	
	/// <summary>光棱塔能力 - 回合开始时对随机敌人造成伤害</summary>
	public static CardValueStore.CardValues PrismTowerPower => new()
	{
		Damage = 5,            // 基础伤害
		Repeat = 1,            // 基础攻击次数
		Stars = 2,             // 未升级时每次叠加增加的伤害
		StarsUpgraded = 3      // 升级后每次叠加增加的伤害 (5 = 2 + 3)
	};

	/// <summary>爱国者导弹能力 - 回合开始时每有一个攻击意图的敌人获得格挡</summary>
	public static CardValueStore.CardValues PatriotMissilePower => new()
	{
		Block = 6,             // 基础格挡
		BlockUpgraded = 3      // 升级后9 = 6 + 3
	};

	/// <summary>黄蜂舰载机能力 - 每回合对目标锁定敌人造成伤害</summary>
	public static CardValueStore.CardValues HornetPower => new()
	{
		Damage = 3,            // 基础伤害
		DamageUpgraded = 1     // 升级后4 = 3 + 1
	};

	/// <summary>基地车能力</summary>
	public static CardValueStore.CardValues AlliedMCVPower => new()
	{
		// 基地车主要是功能能力
	};
	
	/// <summary>训练队列能力</summary>
	public static CardValueStore.CardValues TrainingQueuePower => new()
	{
		// 训练队列主要是功能能力
	};
	
	// ==================== 临时增益能力 ====================
	
	/// <summary>火箭飞行兵临时敏捷能力</summary>
	public static CardValueStore.CardValues RocketSoldierTemporaryDexterityPower => new()
	{
		Stars = 2  // 敏捷值
	};
	
	/// <summary>IFV临时敏捷能力</summary>
	public static CardValueStore.CardValues IfvTemporaryDexterityPower => new()
	{
		Stars = 1  // 敏捷值
	};
}
