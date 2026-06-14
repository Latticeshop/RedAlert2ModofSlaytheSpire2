using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 入侵者战机 - 攻击牌
/// 2费，造成13点伤害，赋予敌人1层易伤
/// 升级后：16点伤害，2层易伤，费用不变
/// </summary>
public sealed class Intruder : CardModel
{
	public Intruder() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/falcicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(13m, ValueProp.Move),
		new RepeatVar(1)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		// 赋予敌人易伤效果
		await PowerCmd.Apply<VulnerablePower>(play.Target, DynamicVars.Repeat.IntValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
		DynamicVars.Repeat.UpgradeValueBy(1);
	}
}
