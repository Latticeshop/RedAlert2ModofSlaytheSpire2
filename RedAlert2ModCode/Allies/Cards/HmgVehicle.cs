using System.Collections.Generic;
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
public sealed class HmgVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.HmgVehicle;

	public HmgVehicle() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type1.png";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvat2b_attack.wav";

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat),
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
		if (play.Target is not MegaCrit.Sts2.Core.Entities.Creatures.Creature target) return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this, play)
			.Targeting(target)
			.Execute(ctx);

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
	}
}
