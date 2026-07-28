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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 防空炮 - 苏联防御建筑
/// 1费技能卡（白卡common）
/// 效果：获得能力：回合开始时，每有一个攻击意图的敌人，获得2点格挡（升级后3点）
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietFlakCannon : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakCannon;

	public SovietFlakCannon() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/flakicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.DefenseTower.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Unpowered),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietFlakCannon] 扣除资金 {Values.DollarValue}");
		}

		BuildingSoundHelper.PlayBuildingPlaceSound();
		UnitVoiceHelper.PlaySovietAaSfx();

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		GD.Print($"[SovietFlakCannon] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 应用防空炮能力
		await SovietFlakCannonPower.ApplyFlakCannon(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}