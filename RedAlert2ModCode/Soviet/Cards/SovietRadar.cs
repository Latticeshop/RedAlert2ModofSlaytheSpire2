using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietRadar : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Radar;
	
	public SovietRadar() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png";
	
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.OrbitalReadiness.CreateHoverTip(),
		HoverTipFactory.FromCard<SpyPlane>()
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
			decimal requiredDollar = IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue;
			if (dollarPower == null || dollarPower.DollarValue < requiredDollar)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		GD.Print($"[SovietRadar] OnPlay 被调用");

		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			decimal dollarCost = IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue;
			dollarPower.AddDollar(-(int)dollarCost);
			GD.Print($"[SovietRadar] 扣除资金 {dollarCost}");
		}

		// 添加雷达能力（用于科技线检查），每次打出都增加层数
		await PowerCmd.Apply<SovietRadarPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[SovietRadar] 添加雷达能力");

		// 获得一张侦察机卡牌到手牌
		var spyPlaneModel = ModelDb.Card<SpyPlane>();
		var spyPlaneCard = Owner.Creature.CombatState.CreateCard(spyPlaneModel, Owner);
		await CardPileCmd.Add(spyPlaneCard, PileType.Hand, CardPilePosition.Bottom, this);
		GD.Print("[SovietRadar] 添加侦察机卡牌到手牌");

		await CardPileCmd.Draw(ctx, 1, Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["DollarNumber"].BaseValue = Values.DollarValueUpgraded;
	}
}