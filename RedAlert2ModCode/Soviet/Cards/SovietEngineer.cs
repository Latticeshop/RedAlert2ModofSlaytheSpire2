using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietEngineer : CardModel
{
	private const int COST = 1;
	private const int BASE_CHOICE_COUNT = 2;
	private const int UPGRADED_CHOICE_COUNT = 1;

	public SovietEngineer() : base(COST, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/engnicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("ChoiceCount", BASE_CHOICE_COUNT)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(GetType(), "Soviet");
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		List<EngineerChoice> choices = EngineerChoiceHelper.GenerateRandomChoices(IsUpgraded, Owner);

		var selectedChoice = await EngineerChoiceScreen.ShowSelectionWithSync(choices, PortraitPath, Owner, FactionType.Soviet);

		if (selectedChoice != null)
		{
			await EngineerChoiceHelper.ExecuteChoice(ctx, selectedChoice, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["ChoiceCount"].UpgradeValueBy(UPGRADED_CHOICE_COUNT);
	}
}