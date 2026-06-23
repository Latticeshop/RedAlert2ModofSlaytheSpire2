using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Relics;

/// <summary>
/// 盟军阵营遗物数值存储
/// 统一管理所有盟军遗物的数值，便于本地化和平衡调整
/// </summary>
public static class AlliesRelicValues
{
	// ==================== 经济遗物 ====================
	
	/// <summary>刀乐遗物 - 战斗开始时提供启动资金</summary>
	public static CardValueStore.CardValues DollarRelic => new()
	{
		DollarValue = 5000  // 启动资金
	};

	/// <summary>先古刀乐遗物 - 战斗开始时提供更多启动资金（先古事件选项）</summary>
	public static CardValueStore.CardValues DollarAncientRelic => new()
	{
		DollarValue = 10000  // 启动资金
	};
}
