using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 美国大兵 - 类似于打击的基础攻击牌
/// 1费3伤害两次，升级后4伤害两次
/// </summary>
public sealed class AmericanSoldier : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AmericanSoldier;
	
	public AmericanSoldier() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/american_soldier.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat)
	};

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Soldier.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}
