using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 修理厂 - 盟军建筑
/// 2费技能卡（蓝卡uncommon，升级后1费）
/// 效果：获得能力：回合开始时，花费$1000资金从消耗牌堆选择一张牌加入弃牌堆
/// </summary>
public sealed class RepairDepot : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RepairDepot;
	private static readonly int BASE_COST = (int)Values.Cost;
	private static readonly int UPGRADED_COST = Values.CostUpgraded > 0 ? Values.CostUpgraded : BASE_COST;

	public RepairDepot() : base(BASE_COST, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fixicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue),
		new IntVar("DollarCost", (int)AlliesPowerValues.RepairDepotPower.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[RepairDepot] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		GD.Print($"[RepairDepot] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 应用修理厂能力
		await RepairDepotPower.ApplyRepairDepot(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		// 升级效果：能量消耗从2费降低到1费
		EnergyCost.SetCustomBaseCost(UPGRADED_COST);
	}
}