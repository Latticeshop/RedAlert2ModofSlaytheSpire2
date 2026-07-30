#nullable enable

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
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 作战实验室 - 盟军建筑卡
/// 0费能力卡，解锁高级兵种，价格2000
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliedBattleLab : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AlliedBattleLab;

	public AlliedBattleLab() : base((int)Values.Cost, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/techicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		HoverTipFactory.FromCard<ForceField>()
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

			// 检查资金是否足够
			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			decimal requiredDollar = IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue;
			if (dollarPower == null || dollarPower.DollarValue < requiredDollar)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print("[AlliedBattleLab] OnPlay 被调用");
		BuildingSoundHelper.PlayBuildingPlaceSound();

		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			decimal dollarCost = IsUpgraded ? Values.DollarValueUpgraded : Values.DollarValue;
			dollarPower.AddDollar(-(int)dollarCost);
			GD.Print($"[AlliedBattleLab] 扣除资金 {dollarCost}");
		}

		// 获得作战实验室能力
		await PowerCmd.Apply<BattleLabPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
		
		GD.Print("[AlliedBattleLab] 已获得作战实验室能力");

		// 添加一张带消耗效果的力场护盾到手牌
		var forceFieldTemplate = ModelDb.Card<ForceField>();
		var forceFieldCard = Owner.Creature.CombatState.CreateCard(forceFieldTemplate, Owner);
		forceFieldCard.AddKeyword(CardKeyword.Exhaust);
		await CardPileCmd.AddGeneratedCardToCombat(forceFieldCard, PileType.Hand, Owner);
		GD.Print("[AlliedBattleLab] 已添加力场护盾到手牌");
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["DollarNumber"].BaseValue = Values.DollarValueUpgraded;
	}
}