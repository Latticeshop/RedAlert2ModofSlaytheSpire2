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
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using Godot;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 入侵者战机 - 攻击牌
/// 2费，造成13点伤害，赋予敌人1层易伤
/// 升级后：16点伤害，2层易伤，费用不变
/// 如果有绝地战备能力，替换攻击效果
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Intruder : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Intruder;
	
	public Intruder() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/falcicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Aircraft.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		GD.Print("[Intruder] 卡牌打出开始");

		// 尝试执行绝地战备攻击（消耗一层）
		bool desperateSuccess = await DesperateMeasures.TryExecuteDesperateMeasureAttack(Owner.Creature, play.Target, ctx);
		if (desperateSuccess)
		{
			GD.Print("[Intruder] 绝地战备攻击成功，跳过普通攻击");
			return;  // 绝地战备已执行，跳过普通攻击
		}

		// 普通攻击流程
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, play)
			.Targeting(play.Target)
			.Execute(ctx);
		
		// 赋予敌人易伤效果
		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars.Repeat.IntValue, Owner.Creature, this);
		
		GD.Print("[Intruder] 卡牌打出完成");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
	}
}
