using RedAlert2ModCode.Common.Utils;

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

	/// <summary>爱国者导弹能力 - 回合开始时获得固定格挡</summary>
	public static CardValueStore.CardValues PatriotMissilePower => new()
	{
		Block = 9,             // 基础格挡
		BlockUpgraded = 3      // 升级后12 = 9 + 3
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

	/// <summary>修理厂能力 - 回合开始时花费$1000从消耗牌堆选择一张牌加入弃牌堆</summary>
	public static CardValueStore.CardValues RepairDepotPower => new()
	{
		DollarValue = 1000  // 每回合花费的资金
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
	
	/// <summary>油井能力 - 回合开始时获得资金</summary>
	public static CardValueStore.CardValues OilDerrickPower => new()
	{
		DollarValue = 200,        // 基础每回合资金
		DollarValueUpgraded = 300  // 升级后每回合 500 = 200 + 300
	};

	// ==================== 绝地战备能力 ====================

	/// <summary>飞鹰500kg能力 - 对目标锁定敌人造成伤害并溅射</summary>
	public static CardValueStore.CardValues Eagle500kgPower => new()
	{
		Damage = 50,            // 基础伤害
		DamageUpgraded = 10     // 升级后60 = 50 + 10
	};

	/// <summary>飞鹰机枪扫射能力 - 对目标锁定敌人造成3点伤害4次</summary>
	public static CardValueStore.CardValues EagleMachineGunPower => new()
	{
		Damage = 3,             // 基础伤害
		DamageUpgraded = 1,     // 升级后4 = 3 + 1
		Repeat = 4              // 攻击次数
	};

	/// <summary>飞鹰空袭能力 - 对全部敌人造成9点伤害</summary>
	public static CardValueStore.CardValues EagleAirStrikePower => new()
	{
		Damage = 9,             // 基础伤害
		DamageUpgraded = 4      // 升级后13 = 9 + 4
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

	/// <summary>提前倒矿能力 - 本回合矿车收益为80%</summary>
	public static CardValueStore.CardValues EarlyMiningPower => new()
	{
		MagicNumber = 80          // 矿车收益百分比：80%
	};
}
