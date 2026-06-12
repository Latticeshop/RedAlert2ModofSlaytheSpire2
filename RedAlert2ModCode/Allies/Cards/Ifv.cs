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
/// IFV - 攻击牌
/// 1费，本回合获得1点敏捷，造成2点伤害2次（升级后2点伤害4次），获得2点护盾
/// </summary>
public sealed class Ifv : CardModel
{
	public Ifv() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fvicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(2m, ValueProp.Move),
		new RepeatVar(2),
		new BlockVar(2m, ValueProp.Unpowered)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 本回合获得1点敏捷（临时，回合结束时自动扣除）
		await PowerCmd.Apply<IfvTemporaryDexterityPower>(Owner.Creature, 1, Owner.Creature, this);
		
		// 造成2点伤害2次
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		// 获得2点护盾
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		// 升级后攻击次数翻倍
		DynamicVars.Repeat.UpgradeValueBy(2);
	}
}
