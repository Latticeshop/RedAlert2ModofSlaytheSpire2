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
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 火箭飞行兵 - 攻击牌
/// 0费，对一个敌人造成1点伤害2次，并在本回合获得两点敏捷
/// 升级后：对所有敌人造成1点伤害2次，并在本回合获得两点敏捷
/// </summary>
public sealed class RocketSoldier : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RocketSoldier;
	
	public RocketSoldier() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/jjeticon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Soldier.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
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
		
		// 本回合获得敏捷（临时，回合结束时自动扣除）
		await PowerCmd.Apply<RocketSoldierTemporaryDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, Values.MagicNumber, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		// 升级后改为对全体敌人攻击（通过 IsUpgraded 判断）
	}
}
