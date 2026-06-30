using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 策略：塔防能力
/// 效果：回合开始时，如果拥有光棱塔能力，获得残影（原版能力）
/// </summary>
public class StrategyTowerDefensePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;

	public StrategyTowerDefensePower()
	{
		GD.Print("[StrategyTowerDefensePower] 构造函数被调用");
	}

	public override LocString Description
	{
		get
		{
			return new LocString("powers", "STRATEGY_TOWER_DEFENSE_POWER.description");
		}
	}

	/// <summary>
	/// 回合开始时检查并应用效果
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		// 检查是否拥有光棱塔能力
		var prismTowerPower = Owner.Powers.OfType<PrismTowerPower>().FirstOrDefault();
		if (prismTowerPower != null)
		{
			GD.Print("[StrategyTowerDefensePower] 拥有光棱塔能力，获得残影");
			
			await PowerCmd.Apply<BlurPower>(new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
			GD.Print("[StrategyTowerDefensePower] 成功获得残影");
		}
		
		await base.AfterSideTurnStart(side, participants, combatState);
	}
}
