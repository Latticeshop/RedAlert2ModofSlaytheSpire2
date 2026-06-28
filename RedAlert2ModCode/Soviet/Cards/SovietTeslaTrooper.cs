using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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

	public SovietTeslaTrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/shkicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.FromOrb<LightningOrb>()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("TeslaTrooper", "Soviet");

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
		}

		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = "部署",
				Description = "给磁暴线圈充能，下次伤害提升50%"
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "orb",
				Title = "生成闪电球",
				Description = "获得一个闪电球"
			}
		};

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, "选择磁暴步兵的行动", options, FactionType.Soviet);

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
