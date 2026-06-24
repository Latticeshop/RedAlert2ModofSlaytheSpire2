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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 防空履带车 - 攻击牌
/// 对应盟军的IFV，1费，本回合获得1点敏捷，造成5点伤害1次（升级后7点），获得2点护盾
/// </summary>
public sealed class FlakTrack : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakTrack;
	
	public FlakTrack() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/htkicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat),
		new BlockVar(2, ValueProp.Unpowered)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		
		// 本回合获得敏捷（临时，回合结束时自动扣除）
		await PowerCmd.Apply<SovietFlakTrackDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
		
		// 造成伤害
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		// 获得护盾
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}