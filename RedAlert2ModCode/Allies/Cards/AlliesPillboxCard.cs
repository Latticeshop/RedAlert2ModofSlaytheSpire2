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

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 机枪碉堡 - 盟军防御建筑
/// 1费技能卡（普通白卡）
/// 效果：获得能力：每回合开始时对随机敌人造成2点伤害，自己获得5点防御
/// </summary>
public sealed class AlliesPillboxCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Pillbox;
	
	public AlliesPillboxCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/pillicon.png";

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

			// 检查是否拥有MCV能力（建造厂）
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
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[AlliesPillboxCard] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		GD.Print($"[AlliesPillboxCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 应用机枪碉堡能力
		await PillboxPower.ApplyPillbox(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		// 防御值升级不加
	}
}
