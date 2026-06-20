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
using RedAlert2ModCode.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 修理厂能力 - 盟军建筑能力
/// 效果：回合开始时，花费$1000资金从消耗牌堆选择一张牌加入弃牌堆
/// </summary>
public class RepairDepotPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesPowerValues.RepairDepotPower;

	/// <summary>
	/// 根据停产状态动态返回能力类型
	/// 生产中 -> Buff（绿色数字）
	/// 停产 -> Debuff（红色数字）
	/// </summary>
	public override PowerType Type => IsStopped ? PowerType.Debuff : PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	/// <summary>
    /// 当前资金花费（每回合花费的资金）
    /// </summary>
    public int CurrentDollarCost { get; set; } = (int)Values.DollarValue;

    /// <summary>
    /// 是否停产
    /// </summary>
    public bool IsStopped { get; set; } = false;

    /// <summary>
    /// 是否升级（升级后资金花费降低）
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

	/// <summary>
	/// 当前可选牌数
	/// </summary>
	public int CurrentCardCount { get; set; } = 1;

	public RepairDepotPower()
	{
		GD.Print($"[RepairDepotPower] 构造函数被调用 - DollarCost={CurrentDollarCost}, CardCount={CurrentCardCount}");
	}

	/// <summary>
	/// 使用修理厂卡牌的图标
	/// </summary>
	public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/fixicon.png";

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("DollarCost", CurrentDollarCost);
			locString.Add("CardCount", CurrentCardCount);
			
			// 如果停产，添加已停产标记
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

	/// <summary>
	/// 应用修理厂能力
	/// </summary>
	public static async Task ApplyRepairDepot(Creature owner, bool isUpgraded = false, bool isStopped = false)
	{
		GD.Print($"[RepairDepotPower] ApplyRepairDepot 被调用 - IsUpgraded={isUpgraded}, IsStopped={isStopped}");

		// 检查是否已有修理厂能力
		var existingPower = owner.Powers
			.OfType<RepairDepotPower>()
			.FirstOrDefault();

		if (existingPower != null)
		{
			// 已有能力，增加资金花费和可选牌数，保持一层
			GD.Print($"[RepairDepotPower] 发现已有能力，增加资金和牌数 - 当前资金: {existingPower.CurrentDollarCost}, 当前牌数: {existingPower.CurrentCardCount}");
			
			// 增加资金花费（每层+1000）
			existingPower.CurrentDollarCost += (int)Values.DollarValue;
			// 增加可选牌数（每层+1）
			existingPower.CurrentCardCount += 1;
			
			GD.Print($"[RepairDepotPower] 增加后资金: {existingPower.CurrentDollarCost}, 牌数: {existingPower.CurrentCardCount}");
			
			// 触发叠加动画反馈
			await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
			GD.Print($"[RepairDepotPower] 触发叠加动画");
			return;
		}

		var newPower = await PowerCmd.Apply<RepairDepotPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentDollarCost = (int)Values.DollarValue;  // 固定花费1000，不随升级变化
			newPower.CurrentCardCount = 1;
			newPower.IsUpgraded = isUpgraded;
			newPower.IsStopped = isStopped;
			GD.Print($"[RepairDepotPower] 创建成功 - DollarCost={newPower.CurrentDollarCost}, CardCount={newPower.CurrentCardCount}, IsUpgraded={newPower.IsUpgraded}, IsStopped={newPower.IsStopped}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		// 如果已停产，不执行修理
		if (IsStopped)
		{
			GD.Print($"[RepairDepotPower] 已停产，跳过修理");
			return;
		}

		GD.Print($"[RepairDepotPower] 回合开始触发 - DollarCost={CurrentDollarCost}, CardCount={CurrentCardCount}");

		// 获取消耗牌堆
		var exhaustPile = PileType.Exhaust.GetPile(Owner.Player);
		if (exhaustPile == null || exhaustPile.Cards.Count == 0)
		{
			GD.Print("[RepairDepotPower] 消耗牌堆为空，跳过");
			return;
		}

		GD.Print($"[RepairDepotPower] 消耗牌堆中有 {exhaustPile.Cards.Count} 张牌");

		// 检查资金是否足够（至少需要一层的花费）
		var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower == null)
		{
			GD.Print("[RepairDepotPower] 没有资金能力，跳过");
			return;
		}

		// 计算单张牌的基础花费（每层+500）
		int baseCostPerCard = (int)Values.DollarValue;
		GD.Print($"[RepairDepotPower] 单张牌基础花费: {baseCostPerCard}");

		// 根据资金计算最大可选牌数
		// 逻辑：资金 $1800，4层修理厂（$2000-4张），应该能选择3张（$1500）
		int maxAffordableCards = dollarPower.DollarValue / baseCostPerCard;
		GD.Print($"[RepairDepotPower] 资金 {dollarPower.DollarValue}，可负担 {maxAffordableCards} 张牌");

		// 实际可选牌数 = min(资金可负担, 当前层数(牌数), 牌堆中牌数)
		int actualMaxCards = Math.Min(Math.Min(maxAffordableCards, CurrentCardCount), exhaustPile.Cards.Count);
		GD.Print($"[RepairDepotPower] 实际最大可选牌数: {actualMaxCards}");

		if (actualMaxCards <= 0)
		{
			GD.Print($"[RepairDepotPower] 资金不足，跳过 - 当前资金: {dollarPower.DollarValue}, 单张花费: {baseCostPerCard}");
			return;
		}

		// 显示卡牌选择界面（支持多选，至少选1张，最多选实际最大牌数）
		var selectedCards = await CardSelectionScreen.ShowMultiSelection(exhaustPile.Cards.ToList(), actualMaxCards, 1);

		if (selectedCards != null && selectedCards.Count > 0)
		{
			GD.Print($"[RepairDepotPower] 选择了 {selectedCards.Count} 张卡牌");

			// 根据实际选择的牌数计算费用并扣除（不展示扣钱动画）
			int actualCost = selectedCards.Count * baseCostPerCard;
			dollarPower.AddDollar(-actualCost);
			GD.Print($"[RepairDepotPower] 扣除资金 {actualCost}（{selectedCards.Count} 张 × {baseCostPerCard}）");

			// 将选中的卡牌从消耗牌堆移动到弃牌堆
			foreach (var selectedCard in selectedCards)
			{
				GD.Print($"[RepairDepotPower] 选择了卡牌: {selectedCard.Id.Entry}");
				await CardPileCmd.Add(selectedCard, PileType.Discard);
				GD.Print($"[RepairDepotPower] 已将 {selectedCard.Id.Entry} 加入弃牌堆");
			}
		}
		else
		{
			GD.Print("[RepairDepotPower] 未选择卡牌，不扣钱");
		}
	}
}