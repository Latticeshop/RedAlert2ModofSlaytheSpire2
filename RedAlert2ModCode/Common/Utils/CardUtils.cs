using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Yuri;

namespace RedAlert2ModCode.Common.Utils;

public static class CardUtils
{
	public static int GetCardEnergyCost(CardModel card)
	{
		var cost = 0;
		
		if (card.EnergyCost != null)
		{
			try
			{
				var resolvedCost = card.EnergyCost.GetResolved();
				cost = (int)resolvedCost;
				GD.Print($"[CardUtils] 使用 GetResolved() 获取费用: {cost}");
			}
			catch
			{
				try
				{
					var canonicalCost = card.EnergyCost.Canonical;
					cost = (int)canonicalCost;
					GD.Print($"[CardUtils] 使用 Canonical 获取费用: {cost}");
				}
				catch
				{
					cost = GetEnergyCostValue(card.EnergyCost);
					GD.Print($"[CardUtils] 使用反射获取费用: {cost}");
				}
			}
		}
		
		return cost;
	}
	
	public static int GetEnergyCostValue(object energyCost)
	{
		var cost = 0;
		var costType = energyCost.GetType();
		
		var valueProp = costType.GetProperty("Value");
		if (valueProp != null)
		{
			var value = valueProp.GetValue(energyCost);
			if (value != null)
			{
				cost = Convert.ToInt32(value);
			}
		}
		
		if (cost == 0)
		{
			var intValueProp = costType.GetProperty("IntegerValue");
			if (intValueProp != null)
			{
				var intValue = intValueProp.GetValue(energyCost);
				if (intValue != null)
				{
					cost = Convert.ToInt32(intValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var valueField = costType.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
			if (valueField != null)
			{
				var value = valueField.GetValue(energyCost);
				if (value != null)
				{
					cost = Convert.ToInt32(value);
				}
			}
		}
		
		if (cost == 0)
		{
			var baseValueProp = costType.GetProperty("BaseValue");
			if (baseValueProp != null)
			{
				var baseValue = baseValueProp.GetValue(energyCost);
				if (baseValue != null)
				{
					cost = Convert.ToInt32(baseValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var costProp = costType.GetProperty("Cost");
			if (costProp != null)
			{
				var costValue = costProp.GetValue(energyCost);
				if (costValue != null)
				{
					cost = Convert.ToInt32(costValue);
				}
			}
		}
		
		if (cost == 0)
		{
			var getCostMethod = costType.GetMethod("GetCost");
			if (getCostMethod != null)
			{
				var result = getCostMethod.Invoke(energyCost, null);
				if (result != null)
				{
					cost = Convert.ToInt32(result);
				}
			}
		}
		
		return cost;
	}
	
	public static async Task HandleCardCancellation(CardPlay play, CardModel cardModel, Player owner)
	{
		var cost = GetCardEnergyCost(cardModel);
		
		GD.Print($"[CardUtils] 取消选择卡牌，费用={cost}");
		
		if (cost > 0 && owner != null)
		{
			await PlayerCmd.GainEnergy(cost, owner);
			GD.Print($"[CardUtils] 已返还 {cost} 能量给玩家");
		}
		else if (cost > 0)
		{
			GD.PrintErr("[CardUtils] 无法获取 Player 对象，能量返还失败");
		}
		
		var cardToReturn = play?.Card ?? cardModel;
		
		if (cardToReturn != null)
		{
			// 参考运输船逻辑：能力卡被移出战斗后，设置 HasBeenRemovedFromState = false 使其能重新进入战斗
			cardToReturn.HasBeenRemovedFromState = false;
			GD.Print($"[CardUtils] 重置卡牌状态 HasBeenRemovedFromState = false");
			
			// 使用 CardPileCmd.Add 添加回手牌，第四个参数传递 cardModel（调用者的 this），与运输船保持一致
			await CardPileCmd.Add(cardToReturn, PileType.Hand, CardPilePosition.Bottom, cardModel);
			
			GD.Print($"[CardUtils] 卡牌已放回手牌底部，当前手牌数: {PileType.Hand.GetPile(cardToReturn.Owner).Cards.Count}");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取实际卡牌实体，放回手牌失败");
		}
	}
	
	public static int GetCardDollarCost(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return 0;
		}
		
		string cardId = cardModel.Id.Entry;
		
		int alliesCost = RedAlert2ModCode.Allies.Cards.AlliesCardValues.GetDollarValue(cardId);
		if (alliesCost > 0)
		{
			return alliesCost;
		}
		
		return RedAlert2ModCode.Soviet.Cards.SovietCardValues.GetDollarValue(cardId);
	}
	
	public static bool IsMcvCard(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return false;
		}
		
		string cardId = cardModel.Id.Entry.ToUpper();
		string normalizedId = cardId.Replace("_", "");
		GD.Print($"[CardUtils] 检查卡牌是否是基地车 - CardId={cardId}, Normalized={normalizedId}");
		
		bool isMcv = normalizedId.Contains("MCV") || 
		             normalizedId.Contains("BASE") || 
		             normalizedId.Contains("COMMANDCENTER") ||
		             normalizedId.Contains("ALLIEDMCV");
		
		if (isMcv)
		{
			GD.Print($"[CardUtils] 识别到基地车卡牌: {cardId}");
		}
		
		return isMcv;
	}
	
	public static bool HasMcvPower(Creature creature)
	{
		if (creature == null || creature.Powers == null)
		{
			return false;
		}
		
		var alliedMcvPower = creature.Powers.OfType<RedAlert2ModCode.Allies.Powers.AlliedMCVPower>().FirstOrDefault();
		if (alliedMcvPower != null)
		{
			return true;
		}
		
		var sovietMcvPower = creature.Powers.OfType<RedAlert2ModCode.Soviet.Powers.SovietMCVPower>().FirstOrDefault();
		if (sovietMcvPower != null)
		{
			return true;
		}
		
		return false;
	}

	/// <summary>
	/// 获取所有单位卡牌类型（士兵、载具、飞机、船只、MCV）
	/// </summary>
	/// <returns>单位卡牌类型集合</returns>
	public static HashSet<Type> GetUnitTypes()
	{
		HashSet<Type> unitTypes = new();

		// 从各阵营Registry获取所有单位类型
		unitTypes.UnionWith(AlliedCardRegistry.GetAllUnits().Select(u => u.GetType()));
		unitTypes.UnionWith(SovietCardRegistry.GetAllUnits().Select(u => u.GetType()));
		unitTypes.UnionWith(YuriCardRegistry.GetAllUnits().Select(u => u.GetType()));

		// MCV不在GetAllUnits中，需要手动补充
		unitTypes.Add(typeof(Allies.Cards.AlliedMCV));
		unitTypes.Add(typeof(Soviet.Cards.SovietMCV));

		return unitTypes;
	}
}