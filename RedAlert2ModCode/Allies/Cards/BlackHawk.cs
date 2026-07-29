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
using MegaCrit.Sts2.Core.Entities.Creatures;
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

[RegisterCard(typeof(AlliesCardPool))]
public sealed class BlackHawk : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.BlackHawk;

	public BlackHawk() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/beagicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Aircraft.CreateHoverTip(),
		ModCardKeywords.DesperateMeasure.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		GD.Print("[BlackHawk] 卡牌打出开始 - 特殊战机（自身效果 + 飞鹰战备）");

		// 黑鹰战机为特殊战机：先触发飞鹰战备（消耗一层），随后继续执行自身攻击效果
		// 与入侵者/黄蜂等普通战机不同，飞鹰战备触发不会替换自身攻击
		bool desperateSuccess = await DesperateMeasures.TryExecuteDesperateMeasureAttack(Owner.Creature, play.Target, ctx);
		if (desperateSuccess)
		{
			GD.Print("[BlackHawk] 飞鹰战备触发成功，继续执行自身攻击效果");
		}

		// 自身攻击效果（无论飞鹰战备是否触发都执行）
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, play)
			.Targeting(play.Target)
			.Execute(ctx);

		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars.Repeat.IntValue, Owner.Creature, this);

		GD.Print("[BlackHawk] 卡牌打出完成");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
	}
}