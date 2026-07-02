using System;
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
		
		if (!IsMcvCard(cardModel))
		{
			await RefundDollarCost(cardModel, owner);
		}
		
		if (play?.Card != null)
		{
			var card = play.Card;
			
			// 不要手动调用 RemoveFromCurrentPile()，让 CardPileCmd.Add 处理牌堆转移和视觉节点
			await CardPileCmd.Add(card, PileType.Hand);
			
			GD.Print($"[CardUtils] 卡牌已放回手牌，当前手牌数: {PileType.Hand.GetPile(card.Owner).Cards.Count}");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取实际卡牌实体，放回手牌失败");
		}
	}
	
	private static async Task RefundDollarCost(CardModel cardModel, Player owner)
	{
		if (owner?.Creature == null)
		{
			GD.PrintErr("[CardUtils] 无法获取 Creature 对象，资金返还失败");
			return;
		}
		
		int dollarCost = GetCardDollarCost(cardModel);
		if (dollarCost <= 0)
		{
			return;
		}
		
		var dollarPower = owner.Creature.Powers.OfType<RedAlert2ModCode.Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.DollarValue += dollarCost;
			DollarVfxHelper.PlayVfx(owner.Creature, dollarCost, DollarVfxType.None);
			GD.Print($"[CardUtils] 已返还 {dollarCost} 资金给玩家（无动画）");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取 DollarPower，资金返还失败");
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
}