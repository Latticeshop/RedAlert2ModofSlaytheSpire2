using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class ChronoVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoVehicle;

	public ChronoVehicle() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type2.png";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichratta_attack.mp3";

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("ErasePercent", Values.MagicNumber),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		ModCardKeywords.Erase.CreateHoverTip(),
		HoverTipFactory.FromPower<ErasingPower>()
	];

	protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
	{
		if (play.Target is not Creature target) return;

		decimal erasePercent = DynamicVars["ErasePercent"].IntValue;
		int maxErase = 50;
		int eraseAmount = (int)Math.Ceiling(target.MaxHp * erasePercent / 100m);
		eraseAmount = Math.Min(eraseAmount, maxErase);

		var existingPower = target.Powers.OfType<ErasingPower>().FirstOrDefault();
		if (existingPower != null)
		{
			await PowerCmd.ModifyAmount(new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), existingPower, eraseAmount, Owner.Creature, this);
		}
		else
		{
			await PowerCmd.Apply<ErasingPower>(
				new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(),
				target,
				eraseAmount,
				Owner.Creature,
				this
			);

			await CreatureCmd.Stun(target);
		}

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["ErasePercent"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}
