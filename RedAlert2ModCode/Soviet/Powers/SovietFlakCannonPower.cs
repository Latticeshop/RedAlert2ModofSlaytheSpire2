using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 防空炮能力 - 苏联防御建筑能力
/// 效果：回合开始时，每有一个攻击意图的敌人，获得一遍格挡
/// </summary>
public class SovietFlakCannonPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = SovietPowerValues.FlakCannonPower;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>
	/// 每次获得的格挡值
	/// </summary>
	public int BlockPerAttack { get; set; } = (int)Values.Block;

	/// <summary>
	/// 是否升级
	/// </summary>
	public bool IsUpgraded { get; set; } = false;

	public SovietFlakCannonPower()
	{
		GD.Print($"[SovietFlakCannonPower] 构造函数被调用 - BlockPerAttack={BlockPerAttack}");
	}

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Block", BlockPerAttack);
			return locString;
		}
	}

	/// <summary>
	/// 应用防空炮能力
	/// </summary>
	public static async Task ApplyFlakCannon(Creature owner, bool isUpgraded = false)
	{
		GD.Print($"[SovietFlakCannonPower] ApplyFlakCannon 被调用 - IsUpgraded={isUpgraded}");

		// 检查是否已有相同升级状态的防空炮能力
		var existingPower = owner.Powers
			.OfType<SovietFlakCannonPower>()
			.FirstOrDefault(p => p.IsUpgraded == isUpgraded);

		if (existingPower != null)
		{
			// 已有相同升级状态的能力，增加层数
			GD.Print($"[SovietFlakCannonPower] 发现相同升级状态的能力，增加层数 - 当前层数: {existingPower.Amount}");
			await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
			GD.Print($"[SovietFlakCannonPower] 增加后层数: {existingPower.Amount}");
			return;
		}

		var newPower = await PowerCmd.Apply<SovietFlakCannonPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.BlockPerAttack = (int)Values.Block + (isUpgraded ? (int)Values.BlockUpgraded : 0);
			newPower.IsUpgraded = isUpgraded;
			GD.Print($"[SovietFlakCannonPower] 创建成功 - BlockPerAttack={newPower.BlockPerAttack}, IsUpgraded={newPower.IsUpgraded}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		// 获取当前层数
		int stacks = (int)base.Amount;
		GD.Print($"[SovietFlakCannonPower] 回合开始触发 - 层数={stacks}");

		// 计算攻击意图的敌人数量
		int attackIntentCount = 0;
		foreach (var enemy in base.Owner.CombatState.Enemies.Where(e => e.IsAlive))
		{
			if (enemy.Monster?.NextMove?.Intents != null)
			{
				foreach (var intent in enemy.Monster.NextMove.Intents)
				{
					if (intent is AttackIntent)
					{
						attackIntentCount++;
						GD.Print($"[SovietFlakCannonPower] 发现敌人有攻击意图，当前计数: {attackIntentCount}");
						break;
					}
				}
			}
		}

		GD.Print($"[SovietFlakCannonPower] 攻击意图敌人总数: {attackIntentCount}");

		// 每一层能力，对每个攻击意图敌人获得一遍格挡
		for (int stack = 0; stack < stacks; stack++)
		{
			for (int i = 0; i < attackIntentCount; i++)
			{
				if (base.Owner != null)
				{
					GD.Print($"[SovietFlakCannonPower] 第{stack+1}层，第{i+1}个攻击意图敌人 - 获得 {BlockPerAttack} 点格挡");
					await CreatureCmd.GainBlock(base.Owner, (decimal)BlockPerAttack, ValueProp.Unpowered, null);
				}
			}
		}
	}
}