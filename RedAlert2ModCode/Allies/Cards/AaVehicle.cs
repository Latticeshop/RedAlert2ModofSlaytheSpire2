using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AaVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AaVehicle;

	public AaVehicle() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type1.png";

	protected override IEnumerable<string> AttackSoundPaths => UnitVoiceHelper.SovietAaSfxPaths;

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip()
	];

	protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
	{
		var combatState = Owner.Creature.CombatState;
		if (combatState == null) return;

		var allEnemies = combatState.HittableEnemies.ToList();
		foreach (var enemy in allEnemies)
		{
			await CreatureCmd.Damage(ctx, enemy, DynamicVars.Damage.BaseValue, ValueProp.Move, this, play);
		}

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}
