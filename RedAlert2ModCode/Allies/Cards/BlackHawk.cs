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

namespace RedAlert2ModCode.Allies.Cards;

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
		GD.Print("[BlackHawk] 卡牌打出开始");

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);

		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars.Repeat.IntValue, Owner.Creature, this);

		await ExecuteDesperateMeasureExtra(Owner.Creature, play.Target, ctx);

		GD.Print("[BlackHawk] 卡牌打出完成");
	}

	private async Task ExecuteDesperateMeasureExtra(Creature player, Creature target, PlayerChoiceContext ctx)
	{
		var desperateMeasure = DesperateMeasures.GetFirstDesperateMeasure(player);
		if (desperateMeasure != null && desperateMeasure is IDesperateMeasurePower dmPower)
		{
			GD.Print($"[BlackHawk] 额外触发飞鹰战备: {desperateMeasure.GetType().Name}");
			await dmPower.ExecuteDesperateMeasureAttack(target, ctx);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
	}
}