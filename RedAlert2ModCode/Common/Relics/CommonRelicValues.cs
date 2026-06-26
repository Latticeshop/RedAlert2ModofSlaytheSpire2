using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Relics;

public static class CommonRelicValues
{
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