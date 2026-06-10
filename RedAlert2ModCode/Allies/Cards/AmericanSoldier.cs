using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 美国大兵 - 类似于打击的基础攻击牌
/// 1费3伤害两次，升级后4伤害两次
/// </summary>
public sealed class AmericanSoldier : CardModel
{
	public AmericanSoldier() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/american_soldier.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(3m, ValueProp.Move),
		new RepeatVar(2)
	};

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
	}
}