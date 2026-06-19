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

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 卡牌工具类，提供卡牌费用获取和取消选择处理等功能
/// </summary>
public static class CardUtils
{
	/// <summary>
	/// 获取卡牌的能量费用
	/// </summary>
	/// <param name="card">卡牌对象</param>
	/// <returns>卡牌费用，如果无法获取则返回0</returns>
	public static int GetCardEnergyCost(CardModel card)
	{
		var cost = 0;
		
		// 优先使用用户提供的方法获取费用
		if (card.EnergyCost != null)
		{
			// 尝试获取实际结算的费用（经过所有增减后）
			try
			{
				var resolvedCost = card.EnergyCost.GetResolved();
				cost = (int)resolvedCost;
				GD.Print($"[CardUtils] 使用 GetResolved() 获取费用: {cost}");
			}
			catch
			{
				// 如果 GetResolved() 失败，尝试获取原始费用
				try
				{
					var canonicalCost = card.EnergyCost.Canonical;
					cost = (int)canonicalCost;
					GD.Print($"[CardUtils] 使用 Canonical 获取费用: {cost}");
				}
				catch
				{
					// 如果都失败，使用反射方式
					cost = GetEnergyCostValue(card.EnergyCost);
					GD.Print($"[CardUtils] 使用反射获取费用: {cost}");
				}
			}
		}
		
		return cost;
	}
	
	/// <summary>
	/// 从 CardEnergyCost 对象获取费用值
	/// </summary>
	/// <param name="energyCost">能量费用对象</param>
	/// <returns>费用值</returns>
	public static int GetEnergyCostValue(object energyCost)
	{
		var cost = 0;
		var costType = energyCost.GetType();
		
		// 尝试获取 Value 属性
		var valueProp = costType.GetProperty("Value");
		if (valueProp != null)
		{
			var value = valueProp.GetValue(energyCost);
			if (value != null)
			{
				cost = Convert.ToInt32(value);
			}
		}
		
		// 尝试获取 IntegerValue 属性
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
		
		// 尝试获取 _value 字段
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
		
		// 尝试获取 BaseValue 属性
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
		
		// 尝试获取 Cost 属性
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
		
		// 尝试调用 GetCost 方法
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
	
	/// <summary>
	/// 处理卡牌取消选择：返还费用和资金，并将卡牌放回手牌
	/// 参考原版 ParticleWall 卡牌的返回逻辑实现
	/// </summary>
	/// <param name="play">卡牌打出信息，包含实际打出的卡牌实体</param>
	/// <param name="cardModel">卡牌模型</param>
	/// <param name="owner">卡牌拥有者（Player 类型）</param>
	public static async Task HandleCardCancellation(CardPlay play, CardModel cardModel, Player owner)
	{
		var cost = GetCardEnergyCost(cardModel);
		
		GD.Print($"[CardUtils] 取消选择卡牌，费用={cost}");
		
		// 返还能量
		if (cost > 0 && owner != null)
		{
			await PlayerCmd.GainEnergy(cost, owner);
			GD.Print($"[CardUtils] 已返还 {cost} 能量给玩家");
		}
		else if (cost > 0)
		{
			GD.PrintErr("[CardUtils] 无法获取 Player 对象，能量返还失败");
		}
		
		// 返还资金（刀乐）
		// 特殊处理：基地车打出时不消耗资金，所以取消选择时也不返还资金
		if (!IsMcvCard(cardModel))
		{
			await RefundDollarCost(cardModel, owner);
		}
		
		// 使用 play.Card（实际打出的卡牌实体）而不是 cardModel（卡牌模板）
		if (play?.Card != null)
		{
			var card = play.Card;
			
			// 获取目标手牌堆
			var handPile = PileType.Hand.GetPile(card.Owner);
			
			// 确保卡牌不在其他堆中（安全检查）
			if (card.Pile != null)
			{
				// 从当前堆移除卡牌（如果还在某个堆中）
				card.RemoveFromCurrentPile();
			}
			
			// 直接将卡牌添加到手牌堆
			await CardPileCmd.Add(card, handPile);
			
			// 强制触发手牌堆的 UI 更新
			handPile.InvokeContentsChanged();
			
			GD.Print($"[CardUtils] 卡牌已放回手牌，当前手牌数: {handPile.Cards.Count}");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取实际卡牌实体，放回手牌失败");
		}
	}
	
	/// <summary>
	/// 返还卡牌的资金消耗（不触发动画，用于UI取消选择时）
	/// </summary>
	/// <param name="cardModel">卡牌模型</param>
	/// <param name="owner">卡牌拥有者（Player 类型）</param>
	private static async Task RefundDollarCost(CardModel cardModel, Player owner)
	{
		if (owner?.Creature == null)
		{
			GD.PrintErr("[CardUtils] 无法获取 Creature 对象，资金返还失败");
			return;
		}
		
		// 获取卡牌的资金消耗
		int dollarCost = GetCardDollarCost(cardModel);
		if (dollarCost <= 0)
		{
			return;
		}
		
		// 获取刀乐能力
		var dollarPower = owner.Creature.Powers.OfType<RedAlert2ModCode.Allies.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			// 直接修改资金值，不触发动画（UI取消选择时不应该看到资金动画）
			dollarPower.DollarValue += dollarCost;
			DollarVfxHelper.PlayVfx(owner.Creature, dollarCost, DollarVfxType.None);
			GD.Print($"[CardUtils] 已返还 {dollarCost} 资金给玩家（无动画）");
		}
		else
		{
			GD.PrintErr("[CardUtils] 无法获取 DollarPower，资金返还失败");
		}
	}
	
	/// <summary>
	/// 获取卡牌的资金消耗
	/// </summary>
	/// <param name="cardModel">卡牌模型</param>
	/// <returns>资金消耗值</returns>
	public static int GetCardDollarCost(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return 0;
		}
		
		// 从 AlliesCardValues 获取卡牌的资金消耗
		return RedAlert2ModCode.Allies.Cards.AlliesCardValues.GetDollarValue(cardModel.Id.Entry);
	}
	
	/// <summary>
	/// 判断卡牌是否是基地车卡牌
	/// </summary>
	/// <param name="cardModel">卡牌模型</param>
	/// <returns>是否是基地车卡牌</returns>
	public static bool IsMcvCard(CardModel cardModel)
	{
		if (cardModel == null || string.IsNullOrEmpty(cardModel.Id.Entry))
		{
			return false;
		}
		
		string cardId = cardModel.Id.Entry.ToUpper();
		// 去掉下划线后再匹配，避免 "ALLIED_MC_V" 无法匹配 "MCV" 的问题
		string normalizedId = cardId.Replace("_", "");
		GD.Print($"[CardUtils] 检查卡牌是否是基地车 - CardId={cardId}, Normalized={normalizedId}");
		
		// 检查是否是基地车卡牌（包含盟军、苏军、尤里的基地车）
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
	
	/// <summary>
	/// 检查角色是否拥有MCV能力（建造厂能力）
	/// </summary>
	/// <param name="creature">角色对象</param>
	/// <returns>是否拥有MCV能力</returns>
	public static bool HasMcvPower(Creature creature)
	{
		if (creature == null || creature.Powers == null)
		{
			return false;
		}
		
		// 检查是否有 AlliedMCVPower 能力
		var mcvPower = creature.Powers.OfType<RedAlert2ModCode.Allies.Powers.AlliedMCVPower>().FirstOrDefault();
		if (mcvPower != null)
		{
			GD.Print("[CardUtils] 角色拥有MCV能力");
			return true;
		}
		
		GD.Print("[CardUtils] 角色没有MCV能力");
		return false;
	}
}