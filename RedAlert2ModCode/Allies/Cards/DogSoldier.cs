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
/// 军犬 - 攻击牌
/// 0费，造成3点伤害，赋予一层虚弱
/// 升级后：4点伤害，2层虚弱，费用不变
/// </summary>
public sealed class DogSoldier : CardModel
{
	public DogSoldier() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/adogicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(3m, ValueProp.Move),
		new RepeatVar(1)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		await PowerCmd.Apply<WeakPower>(play.Target, DynamicVars.Repeat.IntValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
		DynamicVars.Repeat.UpgradeValueBy(1);
	}
}
