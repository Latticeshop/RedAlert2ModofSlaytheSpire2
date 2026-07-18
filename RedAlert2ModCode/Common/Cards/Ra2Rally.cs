using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Common.Cards;

public class Ra2Rally : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Ra2Rally;

	public Ra2Rally() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/rallyicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Unit.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		int cardsToCall = IsUpgraded 
			? (int)Values.MagicNumber + (int)Values.MagicNumberUpgraded 
			: (int)Values.MagicNumber;
		int cardsCalled = 0;

		GD.Print($"[Ra2Rally] 开始召集 {cardsToCall} 张单位卡");

		var unitTypes = GetUnitTypes();

		var drawPile = PileType.Draw.GetPile(Owner);
		var discardPile = PileType.Discard.GetPile(Owner);

		var drawPileUnits = drawPile.Cards
			.Where(c => unitTypes.Contains(c.GetType()))
			.ToList();

		GD.Print($"[Ra2Rally] 抽牌堆中有 {drawPileUnits.Count} 张单位卡");

		foreach (var card in drawPileUnits)
		{
			if (cardsCalled >= cardsToCall) break;
			await CardPileCmd.Add(card, PileType.Hand);
			cardsCalled++;
			GD.Print($"[Ra2Rally] 从抽牌堆找到单位卡: {card.Id.Entry}");
		}

		if (cardsCalled < cardsToCall)
		{
			var discardPileUnits = discardPile.Cards
				.Where(c => unitTypes.Contains(c.GetType()))
				.ToList();

			GD.Print($"[Ra2Rally] 弃牌堆中有 {discardPileUnits.Count} 张单位卡");

			foreach (var card in discardPileUnits)
			{
				if (cardsCalled >= cardsToCall) break;
				await CardPileCmd.Add(card, PileType.Hand);
				cardsCalled++;
				GD.Print($"[Ra2Rally] 从弃牌堆找到单位卡: {card.Id.Entry}");
			}
		}

		GD.Print($"[Ra2Rally] 成功召集 {cardsCalled} 张单位卡");
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}

	private List<System.Type> GetUnitTypes()
	{
		var unitTypes = new List<System.Type>();

		foreach (var soldierFunc in AlliedCardRegistry.Soldiers)
		{
			var card = soldierFunc();
			unitTypes.Add(card.GetType());
		}
		foreach (var soldierFunc in SovietCardRegistry.Soldiers)
		{
			var card = soldierFunc();
			unitTypes.Add(card.GetType());
		}

		foreach (var vehicleFunc in AlliedCardRegistry.Vehicles)
		{
			var card = vehicleFunc();
			unitTypes.Add(card.GetType());
		}
		foreach (var vehicleFunc in SovietCardRegistry.Vehicles)
		{
			var card = vehicleFunc();
			unitTypes.Add(card.GetType());
		}

		foreach (var aircraftFunc in AlliedCardRegistry.Aircraft)
		{
			var card = aircraftFunc();
			unitTypes.Add(card.GetType());
		}
		foreach (var aircraftFunc in SovietCardRegistry.Aircraft)
		{
			var card = aircraftFunc();
			unitTypes.Add(card.GetType());
		}

		foreach (var shipFunc in AlliedCardRegistry.Ships)
		{
			var card = shipFunc();
			unitTypes.Add(card.GetType());
		}
		foreach (var shipFunc in SovietCardRegistry.Ships)
		{
			var card = shipFunc();
			unitTypes.Add(card.GetType());
		}

		return unitTypes;
	}
}