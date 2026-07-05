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

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietTeslaTrooper : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.TeslaTrooper;

	public SovietTeslaTrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/shkicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			var tips = new List<IHoverTip>();
			tips.Add(HoverTipFactory.FromOrb<LightningOrb>());

			if (Owner != null && Owner.Creature != null)
			{
				bool hasTeslaCoil = Owner.Creature.Powers.Any(p => p is SovietTeslaCoilPower);
				if (hasTeslaCoil)
				{
					tips.Add(ModCardKeywords.Deploy.CreateHoverTip());
					tips.Add(HoverTipFactory.FromCard<SovietTeslaCoilCard>());
				}
			}

			return tips;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("TeslaTrooper", "Soviet");

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		bool hasTeslaCoil = Owner.Creature.Powers.Any(p => p is SovietTeslaCoilPower);

		if (!hasTeslaCoil)
		{
			GD.Print("[SovietTeslaTrooper] 没有磁暴线圈能力，直接获得闪电球");
			await OrbCmd.Channel<LightningOrb>(ctx, Owner);
			return;
		}

		var options = new List<DeployChoiceScreen.ChoiceOption>
			{
				new DeployChoiceScreen.ChoiceOption
				{
					Id = "deploy",
					Title = new LocString("card_keywords", "ui.tesla_trooper.deploy_title"),
					Description = new LocString("card_keywords", "ui.tesla_trooper.deploy_desc")
				},
				new DeployChoiceScreen.ChoiceOption
				{
					Id = "orb",
					Title = new LocString("card_keywords", "ui.tesla_trooper.orb_title"),
					Description = new LocString("card_keywords", "ui.tesla_trooper.orb_desc")
				}
			};

			var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.tesla_trooper.title"), options, FactionType.Soviet);

		if (selectedIndex == 0)
		{
			AudioHelper.PlayTeslaTrooperChargeSound(Owner.Creature);
			await SovietTeslaCoilChargePower.ApplyCharge(Owner.Creature);
		}
		else
		{
			await OrbCmd.Channel<LightningOrb>(ctx, Owner);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
