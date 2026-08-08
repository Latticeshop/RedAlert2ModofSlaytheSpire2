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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliesRepairDepot : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RepairDepot;
	private static readonly int BASE_COST = (int)Values.Cost;
	private static readonly int UPGRADED_COST = Values.CostUpgraded > 0 ? Values.CostUpgraded : BASE_COST;
	public AlliesRepairDepot() : base(BASE_COST, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fixicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
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
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();

		var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[AlliesRepairDepot] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 添加维修厂能力（用于出售检测和重工MCV选项解锁）
		await PowerCmd.Apply<AlliedRepairDepotPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[AlliesRepairDepot] 添加盟军维修厂能力");

		var exhaustPile = PileType.Exhaust.GetPile(Owner);
		if (exhaustPile != null && exhaustPile.Cards.Count > 0)
		{
			// 复用原版牌堆选择UI（同“指导”等卡牌）
			var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_ALLIES_REPAIR_DEPOT.select_prompt");
			var prefs = new CardSelectorPrefs(selectPrompt, 1, 1)
			{
				RequireManualConfirmation = true
			};

			var selectedCards = (await CardSelectCmd.FromCombatPile(ctx, exhaustPile, Owner, prefs, null)).ToList();
			var selectedCard = selectedCards.FirstOrDefault();
			if (selectedCard != null)
			{
				await CardPileCmd.Add(selectedCard, PileType.Hand);
				GD.Print($"[AlliesRepairDepot] 从消耗牌堆选择卡牌加入手牌: {selectedCard.Id.Entry}");
			}
		}

	}

	protected override void OnUpgrade()
	{
		EnergyCost.SetCustomBaseCost(UPGRADED_COST);
	}
}
