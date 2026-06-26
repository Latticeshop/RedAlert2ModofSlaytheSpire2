using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Utils;

public static class CardValueStore
{
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
		
		public int GetCost(bool upgraded = false) => upgraded ? Cost + CostUpgraded : Cost;
		
		public decimal GetDamage(bool upgraded = false) => upgraded ? Damage + DamageUpgraded : Damage;
		
		public decimal GetBlock(bool upgraded = false) => upgraded ? Block + BlockUpgraded : Block;
		
		public int GetRepeat(bool upgraded = false) => upgraded ? Repeat + RepeatUpgraded : Repeat;
		
		public int GetEnergy(bool upgraded = false) => upgraded ? Energy + EnergyUpgraded : Energy;
		
		public int GetDollarValue(bool upgraded = false) => upgraded ? DollarValue + DollarValueUpgraded : DollarValue;
		
		public int GetStars(bool upgraded = false) => upgraded ? Stars + StarsUpgraded : Stars;
		
		public int GetMagicNumber(bool upgraded = false) => upgraded ? MagicNumber + MagicNumberUpgraded : MagicNumber;
	}
	
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