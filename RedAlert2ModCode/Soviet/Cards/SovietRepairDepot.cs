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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军维修厂 - 建筑卡
/// 2费能力卡（升级后1费），回合开始时花费$1000从消耗牌堆选择一张牌加入弃牌堆
/// </summary>
public sealed class SovietRepairDepot : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.RepairDepot;
	private static readonly int BASE_COST = (int)Values.Cost;
	private static readonly int UPGRADED_COST = Values.CostUpgraded > 0 ? Values.CostUpgraded : BASE_COST;

	public SovietRepairDepot() : base(BASE_COST, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/rfixicon.png";

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

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue),
		new IntVar("DollarCost", (int)SovietPowerValues.RepairDepotPower.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();

		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietRepairDepot] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		GD.Print($"[SovietRepairDepot] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		await SovietRepairDepotPower.ApplyRepairDepot(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.SetCustomBaseCost(UPGRADED_COST);
	}
}
