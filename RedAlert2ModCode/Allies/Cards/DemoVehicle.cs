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
public sealed class DemoVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.DemoVehicle;

	public DemoVehicle() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type2.png";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemdiea_explosion.mp3";

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new IntVar("Poison", Values.MagicNumber),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>()
	];

	protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
	{
		var combatState = Owner.Creature.CombatState;
		if (combatState == null) return;

		decimal damage = DynamicVars.Damage.BaseValue;
		decimal poisonAmount = DynamicVars["Poison"].IntValue;

		var allEnemies = combatState.HittableEnemies.ToList();
		foreach (var enemy in allEnemies)
		{
			await CreatureCmd.Damage(ctx, enemy, damage, ValueProp.Move, this, play);
			await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, enemy, poisonAmount, Owner.Creature, this);
		}

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Poison"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}
