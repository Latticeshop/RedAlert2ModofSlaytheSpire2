using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class TerrorMan : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Terrorist;

	public TerrorMan() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/trsticon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Splash.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		AudioHelper.PlayRandomExplosionSound();

		Creature? target = play.Target as Creature;
		if (target == null)
		{
			GD.PrintErr("[TerrorMan] 目标不是Creature");
			return;
		}

		List<Creature> allEnemies = CombatState.HittableEnemies.ToList();
		List<Creature> otherEnemies = SplashDamageHelper.GetSplashTargets(target, allEnemies);

		decimal damage = IsUpgraded ? Values.Damage + Values.DamageUpgraded : Values.Damage;
		await DamageCmd.Attack(damage)
			.FromCard(this, play)
			.Targeting(target)
			.Execute(ctx);

		if (otherEnemies.Count > 0)
		{
			decimal splashDamage = SplashDamageHelper.CalculateSplashDamage(damage);
			foreach (Creature otherEnemy in otherEnemies)
			{
				await DamageCmd.Attack(splashDamage)
					.FromCard(this, play)
					.Targeting(otherEnemy)
					.Execute(ctx);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}