using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using System.Collections.Generic;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 火箭飞行兵 - 攻击牌
/// 0费，对一个敌人造成1点伤害2次，并在本回合获得两点敏捷
/// 升级后：对所有敌人造成1点伤害2次，并在本回合获得两点敏捷
/// </summary>
public sealed class RocketSoldier : CardModel
{
	public RocketSoldier() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/jjeticon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(1m, ValueProp.Move),
		new RepeatVar(2)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		if (IsUpgraded)
		{
			// 升级后：对所有敌人造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.WithHitCount(DynamicVars.Repeat.IntValue)
				.FromCard(this)
				.TargetingAllOpponents(Owner.Creature.CombatState)
				.Execute(ctx);
		}
		else
		{
			// 升级前：对单个敌人造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.WithHitCount(DynamicVars.Repeat.IntValue)
				.FromCard(this)
				.Targeting(play.Target)
				.Execute(ctx);
		}
		
		// 本回合获得两点敏捷（临时，回合结束时自动扣除）
		await PowerCmd.Apply<RocketSoldierTemporaryDexterityPower>(Owner.Creature, 2, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		// 升级后改为对全体敌人攻击（通过 IsUpgraded 判断）
	}
}