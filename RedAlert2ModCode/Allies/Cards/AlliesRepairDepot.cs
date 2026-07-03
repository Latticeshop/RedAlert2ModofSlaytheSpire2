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
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class AlliesRepairDepot : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RepairDepot;
	private static readonly int BASE_COST = (int)Values.Cost;
	private static readonly int UPGRADED_COST = Values.CostUpgraded > 0 ? Values.CostUpgraded : BASE_COST;
	private static readonly int BASE_STATUS_COUNT = 2;
	private static readonly int UPGRADED_STATUS_COUNT = 3;

	public AlliesRepairDepot() : base(BASE_COST, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

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
			GD.Print($"[AlliesRepairDepot] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		var exhaustPile = PileType.Exhaust.GetPile(Owner);
		if (exhaustPile != null && exhaustPile.Cards.Count > 0)
		{
			var selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(exhaustPile.Cards.ToList(), Owner);
			if (selectedCard != null)
			{
				await CardPileCmd.Add(selectedCard, PileType.Hand);
				GD.Print($"[AlliesRepairDepot] 从消耗牌堆选择卡牌加入手牌: {selectedCard.Id.Entry}");
			}
		}

		int statusCount = base.IsUpgraded ? UPGRADED_STATUS_COUNT : BASE_STATUS_COUNT;
		var handPile = PileType.Hand.GetPile(Owner);
		var statusCards = handPile?.Cards.Where(c => c.Rarity == CardRarity.Status).ToList() ?? new List<CardModel>();

		int actualCount = Math.Min(statusCards.Count, statusCount);
		if (actualCount > 0)
		{
			var selectedCards = statusCards.Take(actualCount).ToList();
			foreach (var card in selectedCards)
			{
				await CardPileCmd.Add(card, PileType.Exhaust);
				GD.Print($"[AlliesRepairDepot] 消耗状态牌: {card.Id.Entry}");
			}
		}
	}

	protected override void OnUpgrade()
	{
		EnergyCost.SetCustomBaseCost(UPGRADED_COST);
		DynamicVars["StatusCount"].UpgradeValueBy(UPGRADED_STATUS_COUNT - BASE_STATUS_COUNT);
	}
}
