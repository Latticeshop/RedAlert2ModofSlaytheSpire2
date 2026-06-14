using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 光棱塔 - 防御建筑技能牌
/// 2费，获得光棱塔能力，效果为回合开始时对随机敌人造成伤害
/// 升级后费用降低为1费
/// </summary>
public sealed class PrismTowerCard : CardModel
{
	public PrismTowerCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/prisicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(5m, ValueProp.Move),
		new RepeatVar(1)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		GD.Print($"[PrismTowerCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 计算当前光棱塔的等级（已有光棱塔数量+1）
		int currentLevel = GetPrismTowerLevel() + 1;
		
		GD.Print($"[PrismTowerCard] 当前光棱塔等级: {currentLevel}");

		// 应用光棱塔能力
		await PrismTowerPower.ApplyPrismTower(Owner.Creature, currentLevel, base.IsUpgraded);
	}

	/// <summary>
	/// 获取当前已有的光棱塔等级
	/// </summary>
	private int GetPrismTowerLevel()
	{
		if (Owner?.Creature?.Powers == null)
			return 0;
		
		var existingPower = Owner.Creature.Powers.OfType<PrismTowerPower>().FirstOrDefault();
		return existingPower?.PrismTowerLevel ?? 0;
	}

	protected override void OnUpgrade()
	{
		// 升级效果：费用从2降低到1
	}
}
