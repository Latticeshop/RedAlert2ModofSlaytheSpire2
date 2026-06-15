using System.Collections.Generic;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 卡牌数值存储类，用于统一管理卡牌的数值映射
/// 便于本地化获取具体数值，避免硬编码
/// </summary>
public static class CardValueStore
{
	/// <summary>
	/// 单张卡牌的数值集合
	/// </summary>
	public class CardValues
	{
		public int Cost { get; set; } = 0;
		public int CostUpgraded { get; set; } = 0;
		public decimal Damage { get; set; } = 0;
		public decimal DamageUpgraded { get; set; } = 0;
		public decimal Block { get; set; } = 0;
		public decimal BlockUpgraded { get; set; } = 0;
		public int Repeat { get; set; } = 1;
		public int RepeatUpgraded { get; set; } = 0;
		public int Energy { get; set; } = 0;
		public int EnergyUpgraded { get; set; } = 0;
		public int DollarValue { get; set; } = 0;
		public int DollarValueUpgraded { get; set; } = 0;
		public int Stars { get; set; } = 0;
		public int StarsUpgraded { get; set; } = 0;
		public int MagicNumber { get; set; } = 0;
		public int MagicNumberUpgraded { get; set; } = 0;
		
		/// <summary>
		/// 获取费用值（基础+升级）
		/// </summary>
		public int GetCost(bool upgraded = false) => upgraded ? Cost + CostUpgraded : Cost;
		
		/// <summary>
		/// 获取伤害值（基础+升级）
		/// </summary>
		public decimal GetDamage(bool upgraded = false) => upgraded ? Damage + DamageUpgraded : Damage;
		
		/// <summary>
		/// 获取格挡值（基础+升级）
		/// </summary>
		public decimal GetBlock(bool upgraded = false) => upgraded ? Block + BlockUpgraded : Block;
		
		/// <summary>
		/// 获取重复次数（基础+升级）
		/// </summary>
		public int GetRepeat(bool upgraded = false) => upgraded ? Repeat + RepeatUpgraded : Repeat;
		
		/// <summary>
		/// 获取能量值（基础+升级）
		/// </summary>
		public int GetEnergy(bool upgraded = false) => upgraded ? Energy + EnergyUpgraded : Energy;
		
		/// <summary>
		/// 获取资金值（基础+升级）
		/// </summary>
		public int GetDollarValue(bool upgraded = false) => upgraded ? DollarValue + DollarValueUpgraded : DollarValue;
		
		/// <summary>
		/// 获取星星值（基础+升级）
		/// </summary>
		public int GetStars(bool upgraded = false) => upgraded ? Stars + StarsUpgraded : Stars;
		
		/// <summary>
		/// 获取魔法数字（基础+升级）
		/// </summary>
		public int GetMagicNumber(bool upgraded = false) => upgraded ? MagicNumber + MagicNumberUpgraded : MagicNumber;
	}
	
	/// <summary>
	/// 从数值集合创建 DynamicVar 列表
	/// </summary>
	public static List<dynamic> CreateDynamicVars(CardValues values, bool includeDamage = true, bool includeBlock = true, 
		bool includeRepeat = false, bool includeEnergy = false, bool includeDollar = false)
	{
		var vars = new List<dynamic>();
		
		if (includeDamage)
		{
			vars.Add(new MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar(values.Damage, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move));
		}
		
		if (includeBlock)
		{
			vars.Add(new MegaCrit.Sts2.Core.Localization.DynamicVars.BlockVar(values.Block, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move));
		}
		
		if (includeRepeat)
		{
			vars.Add(new MegaCrit.Sts2.Core.Localization.DynamicVars.RepeatVar(values.Repeat));
		}
		
		return vars;
	}
}
