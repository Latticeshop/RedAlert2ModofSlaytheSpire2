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
using MegaCrit.Sts2.Core.Entities.Creatures;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class Desolator : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Desolator;

	public Desolator() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/desoicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("Poison", (int)Values.Damage),
		new IntVar("DeployPoison", (int)Values.Repeat)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		int poisonAmount = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
		int deployPoisonAmount = IsUpgraded ? (int)(Values.Repeat + Values.RepeatUpgraded) : (int)Values.Repeat;

		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "attack",
				Title = new LocString("card_keywords", "ui.desolator.attack_title").GetFormattedText(),
				Description = new LocString("card_keywords", "ui.desolator.attack_desc").GetFormattedText()
					.Replace("{Poison}", poisonAmount.ToString())
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = new LocString("card_keywords", "ui.desolator.deploy_title").GetFormattedText(),
				Description = new LocString("card_keywords", "ui.desolator.deploy_desc").GetFormattedText()
					.Replace("{DeployPoison}", deployPoisonAmount.ToString())
			}
		};

		var titleText = new LocString("card_keywords", "ui.desolator.title").GetFormattedText();
		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, titleText, options, FactionType.Soviet);

		if (selectedIndex == 0)
		{
			UnitVoiceHelper.PlayUnitVoice("Desolator", "Soviet");
			UnitVoiceHelper.PlayUnitVoice("DesolatorAttack", "Soviet");

			if (play.Target is Creature target)
			{
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, target, (decimal)poisonAmount, Owner.Creature, this);
			}
		}
		else
		{
			UnitVoiceHelper.PlayUnitVoice("DesolatorDeploy", "Soviet");
			UnitVoiceHelper.PlayUnitVoice("Desolator", "Soviet");

			var combatState = Owner.Creature.CombatState;
			List<Creature> allEnemies = combatState != null ? combatState.HittableEnemies.ToList() : new List<Creature>();
			foreach (var enemy in allEnemies)
			{
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, enemy, (decimal)deployPoisonAmount, Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Poison"].UpgradeValueBy((int)Values.DamageUpgraded);
		DynamicVars["DeployPoison"].UpgradeValueBy((int)Values.RepeatUpgraded);
	}
}