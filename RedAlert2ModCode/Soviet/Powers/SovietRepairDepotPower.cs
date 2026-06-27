using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Localization;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Soviet.Powers;

public class SovietRepairDepotPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = SovietPowerValues.RepairDepotPower;

	public override PowerType Type => IsStopped ? PowerType.Debuff : PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public int CurrentDollarCost { get; set; } = (int)Values.DollarValue;

	public bool IsStopped { get; set; } = false;

	public bool IsUpgraded { get; set; } = false;

	public int CurrentCardCount { get; set; } = 1;

	public SovietRepairDepotPower()
	{
		GD.Print($"[SovietRepairDepotPower] 构造函数被调用 - DollarCost={CurrentDollarCost}, CardCount={CurrentCardCount}");
	}

	public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/rfixicon.png";

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("DollarCost", CurrentDollarCost);
			locString.Add("CardCount", CurrentCardCount);
			
			if (IsStopped)
			{
				locString.Add("StoppedMarker", "[gold]已停产[/gold]。");
			}
			else
			{
				locString.Add("StoppedMarker", "");
			}
			
			return locString;
		}
	}

	public static async Task ApplyRepairDepot(Creature owner, bool isUpgraded = false, bool isStopped = false)
	{
		GD.Print($"[SovietRepairDepotPower] ApplyRepairDepot 被调用 - IsUpgraded={isUpgraded}, IsStopped={isStopped}");

		var existingPower = owner.Powers
			.OfType<SovietRepairDepotPower>()
			.FirstOrDefault();

		if (existingPower != null)
		{
			GD.Print($"[SovietRepairDepotPower] 发现已有能力，增加资金和牌数 - 当前资金: {existingPower.CurrentDollarCost}, 当前牌数: {existingPower.CurrentCardCount}");
			
			existingPower.CurrentDollarCost += (int)Values.DollarValue;
			existingPower.CurrentCardCount += 1;
			
			GD.Print($"[SovietRepairDepotPower] 增加后资金: {existingPower.CurrentDollarCost}, 牌数: {existingPower.CurrentCardCount}");
			
			await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
			GD.Print($"[SovietRepairDepotPower] 触发叠加动画");
			return;
		}

		var newPower = await PowerCmd.Apply<SovietRepairDepotPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentDollarCost = (int)Values.DollarValue;
			newPower.CurrentCardCount = 1;
			newPower.IsUpgraded = isUpgraded;
			newPower.IsStopped = isStopped;
			GD.Print($"[SovietRepairDepotPower] 创建成功 - DollarCost={newPower.CurrentDollarCost}, CardCount={newPower.CurrentCardCount}, IsUpgraded={newPower.IsUpgraded}, IsStopped={newPower.IsStopped}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		if (IsStopped)
		{
			GD.Print($"[SovietRepairDepotPower] 已停产，跳过修理");
			return;
		}

		GD.Print($"[SovietRepairDepotPower] 回合开始触发 - DollarCost={CurrentDollarCost}, CardCount={CurrentCardCount}");

		var exhaustPile = PileType.Exhaust.GetPile(Owner.Player);
		if (exhaustPile == null || exhaustPile.Cards.Count == 0)
		{
			GD.Print("[SovietRepairDepotPower] 消耗牌堆为空，跳过");
			return;
		}

		GD.Print($"[SovietRepairDepotPower] 消耗牌堆中有 {exhaustPile.Cards.Count} 张牌");

		var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower == null)
		{
			GD.Print("[SovietRepairDepotPower] 没有资金能力，跳过");
			return;
		}

		int baseCostPerCard = (int)Values.DollarValue;
		GD.Print($"[SovietRepairDepotPower] 单张牌基础花费: {baseCostPerCard}");

		int maxAffordableCards = dollarPower.DollarValue / baseCostPerCard;
		GD.Print($"[SovietRepairDepotPower] 资金 {dollarPower.DollarValue}，可负担 {maxAffordableCards} 张牌");

		int actualMaxCards = Math.Min(Math.Min(maxAffordableCards, CurrentCardCount), exhaustPile.Cards.Count);
		GD.Print($"[SovietRepairDepotPower] 实际最大可选牌数: {actualMaxCards}");

		if (actualMaxCards <= 0)
		{
			GD.Print($"[SovietRepairDepotPower] 资金不足，跳过 - 当前资金: {dollarPower.DollarValue}, 单张花费: {baseCostPerCard}");
			return;
		}

		var selectedCards = await CardSelectionScreen.ShowMultiSelection(exhaustPile.Cards.ToList(), actualMaxCards, 1);

		if (selectedCards != null && selectedCards.Count > 0)
		{
			GD.Print($"[SovietRepairDepotPower] 选择了 {selectedCards.Count} 张卡牌");

			int actualCost = selectedCards.Count * baseCostPerCard;
			dollarPower.AddDollar(-actualCost);
			GD.Print($"[SovietRepairDepotPower] 扣除资金 {actualCost}（{selectedCards.Count} 张 × {baseCostPerCard}）");

			foreach (var selectedCard in selectedCards)
			{
				GD.Print($"[SovietRepairDepotPower] 选择了卡牌: {selectedCard.Id.Entry}");
				await CardPileCmd.Add(selectedCard, PileType.Hand);
				GD.Print($"[SovietRepairDepotPower] 已将 {selectedCard.Id.Entry} 加入手牌");
			}
		}
		else
		{
			GD.Print("[SovietRepairDepotPower] 未选择卡牌，不扣钱");
		}
	}
}