using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军哨戒炮 - 防御建筑卡
/// 1费，回合开始时对敌人造成伤害2次，获得防御（升级不加）
/// </summary>
public sealed class SovietPillboxCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietPillbox;
	
	public SovietPillboxCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/plticon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.DefenseTower.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Unpowered),
		new BlockVar(Values.Block, ValueProp.Unpowered),
		new IntVar("DollarNumber", Values.DollarValue),
		new IntVar("Repeat", Values.Repeat)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietPillboxCard] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		GD.Print($"[SovietPillboxCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		await PillboxPower.ApplyPillbox(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}