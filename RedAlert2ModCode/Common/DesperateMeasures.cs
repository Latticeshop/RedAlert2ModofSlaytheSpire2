using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using Godot;

namespace RedAlert2ModCode.Common;

/// <summary>
/// 绝地战备系统 - 管理各种绝地战备能力和战机卡牌
/// 当飞机类卡牌或能力造成伤害时，检查是否有绝地战备能力，有则消耗一层替换攻击效果
///
/// 使用方式：
/// 1. 创建继承自 DesperateMeasurePowerBase 的能力类，自动实现 IDesperateMeasurePower 接口
/// 2. 创建继承自 DesperateMeasureCardBase 的卡牌类，自动注册为绝地战备卡牌
/// 3. 战机卡牌在攻击前调用 TryExecuteDesperateMeasureAttack，通过 IDesperateMeasurePower 接口检查并触发替换
/// </summary>
public static class DesperateMeasures
{
	/// <summary>
	/// 检查玩家是否有任何绝地战备能力
	/// </summary>
	public static bool HasDesperateMeasure(Creature player)
	{
		return player.Powers.Any(p => p is IDesperateMeasurePower);
	}

	/// <summary>
	/// 检查牌库中是否有任何绝地战备卡牌
	/// </summary>
	public static bool HasDesperateMeasureCardInDeck(CardModel card)
	{
		if (card?.Owner?.Deck?.Cards == null)
		{
			GD.Print("[DesperateMeasures] HasDesperateMeasureCardInDeck - 无效的卡牌或牌库");
			return false;
		}

		bool hasCard = card.Owner.Deck.Cards.Any(c => c is IDesperateMeasureCard);
		GD.Print($"[DesperateMeasures] HasDesperateMeasureCardInDeck - 牌库中是否有绝地战备卡牌: {hasCard}");
		return hasCard;
	}

	/// <summary>
	/// 获取玩家的第一个绝地战备能力
	/// </summary>
	public static PowerModel? GetFirstDesperateMeasure(Creature player)
	{
		return player.Powers.FirstOrDefault(p => p is IDesperateMeasurePower);
	}

	/// <summary>
	/// 获取玩家的所有绝地战备能力
	/// </summary>
	public static List<PowerModel> GetAllDesperateMeasures(Creature player)
	{
		return player.Powers.Where(p => p is IDesperateMeasurePower).ToList();
	}

	/// <summary>
	/// 检查能力是否为绝地战备类型
	/// </summary>
	public static bool IsDesperateMeasurePower(PowerModel power)
	{
		if (power == null)
		{
			GD.Print("[DesperateMeasures] IsDesperateMeasurePower: power is null");
			return false;
		}

		bool isDM = power is IDesperateMeasurePower;
		GD.Print($"[DesperateMeasures] IsDesperateMeasurePower - {power.GetType().Name}: {isDM}");
		return isDM;
	}

	/// <summary>
	/// 尝试执行绝地战备攻击（消耗一层）
	/// 用于飞机类卡牌和能力在造成伤害前调用
	/// 通过 IDesperateMeasurePower 接口检查触发
	/// </summary>
	public static async Task<bool> TryExecuteDesperateMeasureAttack(Creature player, Creature target, PlayerChoiceContext ctx)
	{
		GD.Print($"[DesperateMeasures] TryExecuteDesperateMeasureAttack 被调用 - Player={player?.Name}, Target={target?.Name}");

		if (player == null)
		{
			GD.PrintErr("[DesperateMeasures] 玩家为空，无法执行绝地战备攻击");
			return false;
		}

		var desperateMeasure = GetFirstDesperateMeasure(player);
		GD.Print($"[DesperateMeasures] 找到绝地战备能力: {desperateMeasure != null}");

		if (desperateMeasure != null && desperateMeasure is IDesperateMeasurePower dmPower)
		{
			GD.Print($"[DesperateMeasures] 发现绝地战备能力: {desperateMeasure.GetType().Name}, 层数: {desperateMeasure.Amount}");

			// 交由能力内部解析目标（战机目标 → 目标锁定 → 随机），AOE 能力则对全体生效
			bool success = await dmPower.ExecuteDesperateMeasureAttack(target, ctx);
			if (success)
			{
				GD.Print("[DesperateMeasures] 绝地战备攻击成功");
				return true;
			}
			else
			{
				GD.Print("[DesperateMeasures] 绝地战备攻击失败");
				return false;
			}
		}
		else
		{
			GD.Print("[DesperateMeasures] 没有找到有效的绝地战备能力");
			return false;
		}
	}
}