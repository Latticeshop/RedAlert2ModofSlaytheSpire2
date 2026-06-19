using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 爱国者导弹能力 - 盟军防御建筑能力
/// 效果：回合开始时，每有一个攻击意图的敌人，获得6点格挡（升级后9点）
/// </summary>
public class PatriotMissilePower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesPowerValues.PatriotMissilePower;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同升级状态的叠加逻辑在 ApplyPatriotMissile 中手动处理
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>
	/// 当前格挡值（每有一个攻击意图敌人获得的格挡）
	/// </summary>
	public int CurrentBlock { get; set; } = (int)Values.Block;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

	public PatriotMissilePower()
	{
		GD.Print($"[PatriotMissilePower] 构造函数被调用 - Block={CurrentBlock}");
	}

	/// <summary>
	/// 使用爱国者导弹卡牌的图标
	/// </summary>
	public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/samicon.png";

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Block", CurrentBlock);
			return locString;
		}
	}

	/// <summary>
	/// 应用爱国者导弹能力
	/// </summary>
	public static async Task ApplyPatriotMissile(Creature owner, bool isUpgraded = false)
	{
		GD.Print($"[PatriotMissilePower] ApplyPatriotMissile 被调用 - IsUpgraded={isUpgraded}");

        // 检查是否已有相同升级状态的爱国者导弹能力
        var existingPower = owner.Powers
            .OfType<PatriotMissilePower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            // 已有相同升级状态的能力，增加层数
            GD.Print($"[PatriotMissilePower] 发现相同升级状态的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[PatriotMissilePower] 增加后层数: {existingPower.Amount}");
            return;
        }

		var newPower = await PowerCmd.Apply<PatriotMissilePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentBlock = (int)Values.Block + (isUpgraded ? (int)Values.BlockUpgraded : 0);
            newPower.IsUpgraded = isUpgraded;
			GD.Print($"[PatriotMissilePower] 创建成功 - Block={newPower.CurrentBlock}, IsUpgraded={newPower.IsUpgraded}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		// 获取当前层数
		int stacks = (int)base.Amount;
		GD.Print($"[PatriotMissilePower] 回合开始触发 - 层数={stacks}, Block={CurrentBlock}");

		// 统计攻击意图的敌人数量
		int attackIntentCount = 0;
		var enemies = combatState.Enemies.Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();

		foreach (var enemy in enemies)
		{
			if (enemy.Monster?.NextMove?.Intents != null)
			{
				foreach (var intent in enemy.Monster.NextMove.Intents)
				{
					if (intent is AttackIntent)
					{
						attackIntentCount++;
						GD.Print($"[PatriotMissilePower] 发现敌人 {enemy.Name} 有攻击意图");
						break;  // 每个敌人只计算一次
					}
				}
			}
		}

		GD.Print($"[PatriotMissilePower] 攻击意图敌人数量: {attackIntentCount}");

		// 每有一个攻击意图的敌人，获得格挡（按层数循环）
		for (int i = 0; i < stacks; i++)
		{
			for (int j = 0; j < attackIntentCount; j++)
			{
				if (base.Owner != null)
				{
					GD.Print($"[PatriotMissilePower] 第{i+1}层第{j+1}个攻击敌人 - 获得 {CurrentBlock} 点格挡");
					await CreatureCmd.GainBlock(base.Owner, (decimal)CurrentBlock, ValueProp.Unpowered, null);
				}
			}
		}
	}
}