using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietRepairDepot : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.RepairDepot;
	private static readonly int BASE_COST = (int)Values.Cost;
	private static readonly int UPGRADED_COST = Values.CostUpgraded > 0 ? Values.CostUpgraded : BASE_COST;
	private static readonly int BASE_STATUS_COUNT = 2;
	private static readonly int UPGRADED_STATUS_COUNT = 3;

	public SovietRepairDepot() : base(BASE_COST, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/rfixicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.TechLevelT2.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue),
		new IntVar("StatusCount", BASE_STATUS_COUNT)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();

		var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietRepairDepot] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 添加维修厂能力（用于出售检测和重工MCV选项解锁）
		await PowerCmd.Apply<SovietRepairDepotPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[SovietRepairDepot] 添加苏联维修厂能力");

		int exhaustCount = base.IsUpgraded ? UPGRADED_STATUS_COUNT : BASE_STATUS_COUNT;

		// 手牌中没有可消耗的卡牌时，跳过选择界面（避免卡死）
		var handPile = PileType.Hand.GetPile(base.Owner);
		if (!handPile.Cards.Any(c => c != this))
		{
			GD.Print("[SovietRepairDepot] 手牌中没有可消耗的卡牌，跳过选择");
			return;
		}

		// 使用原版手牌选择UI，让玩家选择要消耗的卡牌（0到exhaustCount张）
		var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_SOVIET_REPAIR_DEPOT.select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", exhaustCount);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, exhaustCount)
		{
			RequireManualConfirmation = true
		};

		var selectedCards = (await CardSelectCmd.FromHand(
			ctx,
			base.Owner,
			prefs,
			c => c != this,
			this
		)).ToList();

		foreach (var card in selectedCards)
		{
			await CardPileCmd.Add(card, PileType.Exhaust);
			GD.Print($"[SovietRepairDepot] 消耗手牌: {card.Id.Entry}");
		}

		GD.Print($"[SovietRepairDepot] 共消耗 {selectedCards.Count} 张手牌");

	}

	protected override void OnUpgrade()
	{
		EnergyCost.SetCustomBaseCost(UPGRADED_COST);
		DynamicVars["StatusCount"].UpgradeValueBy(UPGRADED_STATUS_COUNT - BASE_STATUS_COUNT);
	}
}
