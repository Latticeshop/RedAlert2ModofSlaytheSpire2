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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class SpyPlane : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SpyPlane;

	public SpyPlane() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/spypicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("Dexterity", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Aircraft.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("SpyPlaneEngine", "Soviet");
		UnitVoiceHelper.PlayUnitVoice("SpyPlaneSnap", "Soviet");

		int dexterity = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
		await PowerCmd.Apply<SpyPlaneTemporaryDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, dexterity, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Dexterity"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}