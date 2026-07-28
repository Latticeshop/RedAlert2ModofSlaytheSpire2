using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class RadVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RadVehicle;

	public RadVehicle() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type2.png";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesat1a_radiation.mp3";

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
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
		if (play.Target is not MegaCrit.Sts2.Core.Entities.Creatures.Creature target) return;

		decimal poisonAmount = DynamicVars["Poison"].IntValue;
		await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, target, poisonAmount, Owner.Creature, this);

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Poison"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}
