using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public sealed class ChronoCommandos : ChronoCardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.ChronoCommandos;

	public ChronoCommandos() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/other/ccomicon.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[0];

	protected override List<IHoverTip> GetExtraHoverTips()
	{
		return new List<IHoverTip>
		{
			ModCardKeywords.Soldier.CreateHoverTip(),
			ModCardKeywords.Infiltrator.CreateHoverTip(),
			ModCardKeywords.Deploy.CreateHoverTip()
		};
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat),
		new IntVar("DeployDamage", Values.MagicNumber),
		new IntVar("DollarNumber", Values.DollarValue),
		new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		if (play.Target is not Creature target) return;

		PlayChronoMoveSound();
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allied");

		bool isAttackIntent = IsAttackIntent(target);

		if (!isAttackIntent)
		{
			UnitVoiceHelper.PlayUnitVoice("ChronoCommandosC4", "Allied");

			int deployDamage = DynamicVars["DeployDamage"].IntValue;
			await DamageCmd.Attack(deployDamage)
				.FromCard(this, play)
				.Targeting(target)
				.Execute(ctx);
			GD.Print($"[ChronoCommandos] 部署效果触发，对非攻击意图敌人造成 {deployDamage} 点伤害");
		}
		else
		{
			UnitVoiceHelper.PlayUnitVoice("ChronoCommandosAttack", "Allied");

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.WithHitCount(DynamicVars.Repeat.IntValue)
				.FromCard(this, play)
				.Targeting(target)
				.Execute(ctx);
			GD.Print($"[ChronoCommandos] 攻击效果触发，造成 {DynamicVars.Damage.BaseValue} 点伤害 {DynamicVars.Repeat.IntValue} 次");
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars["DeployDamage"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}

	private bool IsAttackIntent(Creature target)
	{
		if (target.Monster?.NextMove?.Intents != null)
		{
			return target.Monster.NextMove.Intents.Any(intent => intent is AttackIntent);
		}
		return false;
	}

	private void PlayChronoMoveSound()
	{
		CommonSoundHelper.PlayChronoMoveSound();
	}
}
