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
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietTeslaTrooper : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.TeslaTrooper;

	public SovietTeslaTrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/shkicon.png";

	private const string AttackSoundPath = "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itesat2b-attack.mp3";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		HoverTipFactory.FromOrb<LightningOrb>(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipHelper.FromCardWithUpgrade<SovietTeslaCoilCard>(() => IsUpgraded)
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("TeslaTrooper", "Soviet");

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		bool hasTeslaCoil = Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(SovietTeslaCoilPower).Name);

		if (!hasTeslaCoil)
		{
			GD.Print("[SovietTeslaTrooper] 没有磁暴线圈能力，直接获得闪电球");
			UnitVoiceHelper.PlaySound(AttackSoundPath);
			await OrbCmd.Channel<LightningOrb>(ctx, Owner);
			return;
		}

		var options = new List<DeployChoiceScreen.ChoiceOption>
			{
				new DeployChoiceScreen.ChoiceOption
				{
					Id = "orb",
					Title = new LocString("card_keywords", "ui.tesla_trooper.orb_title"),
					Description = new LocString("card_keywords", "ui.tesla_trooper.orb_desc"),
					IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
				},
				new DeployChoiceScreen.ChoiceOption
				{
					Id = "deploy",
					Title = new LocString("card_keywords", "ui.tesla_trooper.deploy_title"),
					Description = new LocString("card_keywords", "ui.tesla_trooper.deploy_desc"),
					IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
				}
			};

			var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.tesla_trooper.title"), options, FactionType.Soviet);

		if (selectedIndex == 0)
		{
			UnitVoiceHelper.PlaySound(AttackSoundPath);
			await OrbCmd.Channel<LightningOrb>(ctx, Owner);
		}
		else
		{
			AudioHelper.PlayTeslaTrooperChargeSound(Owner.Creature);
			await SovietTeslaCoilChargePower.ApplyCharge(Owner.Creature);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
