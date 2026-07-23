using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
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
public sealed class TeslaTank : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.TeslaTank;

	public TeslaTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/Tesla_Tank.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("LightningOrb", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("TeslaTank", "Soviet");
		UnitVoiceHelper.PlayUnitVoice("TeslaTankAttack", "Soviet");

		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

		await OrbCmd.AddSlots(Owner, 1);

		int lightningOrbAmount = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
		for (int i = 0; i < lightningOrbAmount; i++)
		{
			await OrbCmd.Channel<MegaCrit.Sts2.Core.Models.Orbs.LightningOrb>(ctx, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Block"].UpgradeValueBy(Values.BlockUpgraded);
		DynamicVars["LightningOrb"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}