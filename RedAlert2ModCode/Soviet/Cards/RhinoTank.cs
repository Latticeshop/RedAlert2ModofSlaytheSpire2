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
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 犀牛坦克 - 类似于铁斩波的攻击牌
/// 1费5攻击7防御，升级后8攻击8防御，赋予1层易伤（升级后2层）
/// 对应盟军的灰熊坦克，苏军坦克更强大
/// </summary>
public sealed class RhinoTank : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = SovietCardValues.RhinoTank;
	
	public RhinoTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/htnkicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("VulnerableStacks", Values.MagicNumber)
	};

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		
		// 造成伤害
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		// 获得防御
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
		
		// 赋予易伤效果
		int vulnerableStacks = Values.MagicNumber + (IsUpgraded ? Values.MagicNumberUpgraded : 0);
		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, vulnerableStacks, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}