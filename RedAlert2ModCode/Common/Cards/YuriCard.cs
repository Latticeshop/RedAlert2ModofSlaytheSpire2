using System;
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
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Random;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Common.Cards;

public sealed class YuriCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Yuri;

	public YuriCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/yuriicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("YuriAttack", "Yuri");
		UnitVoiceHelper.PlayUnitVoice("Yuri", "Yuri");

		var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

		List<Type> unitPool = GetUnitPool(IsUpgraded);
		if (unitPool.Count == 0)
			return;

		Rng rng = Owner.RunState.Rng.CombatCardSelection;
		int randomIndex = rng.NextInt(unitPool.Count);
		Type selectedUnitType = unitPool[randomIndex];

		try
		{
			var template = (CardModel)typeof(ModelDb)
				.GetMethod("Card")
				.MakeGenericMethod(selectedUnitType)
				.Invoke(null, null);

			CardModel unitCard = Owner.Creature.CombatState.CreateCard(template, Owner);
			if (unitCard != null)
			{
				unitCard.AddKeyword(CardKeyword.Exhaust);
				await CardPileCmd.AddGeneratedCardToCombat(unitCard, PileType.Hand, Owner);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[YuriCard] 创建单位卡牌失败: {ex.Message}");
		}
	}

	internal static List<Type> GetUnitPool(bool includeT3)
	{
		List<Type> pool = new()
		{
			typeof(AmericanSoldier),
			typeof(AlliesDogSoldier),
			typeof(GuardianGi),
			typeof(RocketSoldier),
			typeof(AlliesEngineer),
			typeof(GrizzlyTank),
			typeof(Ifv),
			typeof(ChronoMiner),
			typeof(Intruder),
			typeof(BlackHawk),
			typeof(NightHawkChopper),
			typeof(Dolphin),
			typeof(AlliedTransportShip),
			typeof(Destroyer),
			typeof(Agisicon),
			typeof(Sniper),
			typeof(TankDestroyer),
			typeof(Conscript),
			typeof(SovietEngineer),
			typeof(SovietAttackDog),
			typeof(SovietFlakTrooper),
			typeof(RhinoTank),
			typeof(WarMiner),
			typeof(FlakTrack),
			typeof(TerrorDrone),
			typeof(SpyPlane),
			typeof(SovietTransportShip),
			typeof(FlakSubmarine),
			typeof(TyphoonSubmarine),
			typeof(GiantSquid),
			typeof(SovietTeslaTrooper),
			typeof(Desolator),
			typeof(TerrorMan),
			typeof(V3Rocket),
			typeof(DemolitionTruckCard),
			typeof(TeslaTank),
		};

		if (includeT3)
		{
			pool.AddRange(new List<Type>
			{
				typeof(ChronoLegionnaire),
				typeof(MirageTank),
				typeof(PrismTank),
				typeof(BattleFortress),
				typeof(AircraftCarrier),
				typeof(Kirov),
				typeof(ApocalypseTank),
				typeof(Dreadnought),
			});
		}

		return pool;
	}
}
