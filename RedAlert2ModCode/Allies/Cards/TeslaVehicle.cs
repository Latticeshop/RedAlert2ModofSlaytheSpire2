using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class TeslaVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.TeslaVehicle;

	public TeslaVehicle() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_type2.png";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesatta_attack.mp3";

	protected override string ActionKeyName => "attack";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("OrbCount", Values.MagicNumber),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.FromOrb<LightningOrb>()
	];

	protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
	{
		decimal orbCount = DynamicVars["OrbCount"].IntValue;
		for (int i = 0; i < orbCount; i++)
		{
			await OrbCmd.Channel<LightningOrb>(ctx, Owner);
		}

		await ConsumeEffectWithExhaust();
	}

	protected override void OnUpgrade()
	{
		DynamicVars["OrbCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}
