using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 绝地战备能力基类
/// 提供绝地战备能力的公共逻辑：目标验证、目标锁定、层数消耗、异常处理
/// 子类只需实现 ExecuteAttackEffect 来定义具体攻击效果
/// 
/// 目标选择优先级：
/// 1. StoredTarget（卡牌打出时存储的目标）→ 直接使用
/// 2. TargetLocked 敌人（保底回退）→ 若存在则使用
/// 3. 随机存活敌人 → 最终保底
/// </summary>
public abstract class DesperateMeasurePowerBase : PowerModel, IDesperateMeasurePower
{
	/// <summary>
	/// 能力类型
	/// </summary>
	public override PowerType Type => PowerType.Buff;

	/// <summary>
	/// 堆叠类型
	/// </summary>
	public override PowerStackType StackType => PowerStackType.Counter;

	/// <summary>
	/// 实例类型 - 子类可覆盖
	/// </summary>
	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>
	/// 当前伤害值
	/// </summary>
	public int CurrentDamage { get; set; }

	/// <summary>
	/// 是否升级
	/// </summary>
	public bool IsUpgraded { get; set; }

	/// <summary>
	/// 图标路径 - 子类提供
	/// </summary>
	public abstract string PackedIconPath { get; }

	/// <summary>
	/// 卡牌打出时存储的目标 - 优先级最高
	/// </summary>
	public Creature? StoredTarget { get; set; }

	/// <summary>
	/// 描述文本
	/// </summary>
	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			UpdateDescriptionVars(locString);
			return locString;
		}
	}

	/// <summary>
	/// 更新描述中的动态变量 - 子类可覆盖添加额外变量
	/// </summary>
	protected virtual void UpdateDescriptionVars(LocString locString)
	{
		locString.Add("Damage", CurrentDamage);
	}

	/// <summary>
	/// 是否需要目标锁定（回退机制）
	/// 子类可覆盖：空袭等AOE卡设为false
	/// </summary>
	protected virtual bool NeedsTargetLock => true;

	/// <summary>
	/// 是否为AOE能力（对全体敌人生效，不需要目标）
	/// </summary>
	protected virtual bool IsAoeAttack => false;

	/// <summary>
	/// 回合开始触发 - 核心入口
	/// 目标解析优先级：StoredTarget → TargetLocked → 随机敌人
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != CombatSide.Player || Owner == null)
			return;

		var ctx = new ThrowingPlayerChoiceContext();

		// AOE模式：直接对全体敌人生效
		if (IsAoeAttack)
		{
			GD.Print($"[{GetType().Name}] AOE模式 - 对全体敌人生效, Amount={Amount}");
			await ExecuteAoeAttack(ctx, combatState);
			await ConsumeOrRemove(ctx);
			return;
		}

		// 单体模式：解析目标
		Creature? target = ResolveTarget(combatState);

		if (target == null)
		{
			GD.Print($"[{GetType().Name}] 无有效目标，跳过");
			return;
		}

		GD.Print($"[{GetType().Name}] 回合开始触发 - Target={target.Name}, Damage={CurrentDamage}, Amount={Amount}");

		// 执行攻击
		await ExecuteAttackEffect(target, ctx);

		// 消耗层数
		await ConsumeOrRemove(ctx);
	}

	/// <summary>
	/// 解析目标 - 优先级：StoredTarget → TargetLocked → 随机存活敌人
	/// </summary>
	private Creature? ResolveTarget(ICombatState combatState)
	{
		// 优先级1：使用卡牌打出时存储的目标
		if (StoredTarget != null && StoredTarget.IsAlive)
		{
			GD.Print($"[{GetType().Name}] 使用存储的目标: {StoredTarget.Name}");
			return StoredTarget;
		}

		// 优先级2：查找目标锁定敌人（保底）
		if (NeedsTargetLock)
		{
			var targetLocked = combatState.Enemies
				.FirstOrDefault(e => e.Side == CombatSide.Enemy && e.IsAlive &&
									e.Powers.Any(p => p is TargetLockedPower));

			if (targetLocked != null)
			{
				GD.Print($"[{GetType().Name}] 使用目标锁定敌人: {targetLocked.Name}");
				return targetLocked;
			}
		}

		// 优先级3：随机选择存活敌人（最终保底）
		var aliveEnemies = combatState.Enemies
			.Where(e => e.Side == CombatSide.Enemy && e.IsAlive)
			.ToList();

		if (aliveEnemies.Count > 0)
		{
			var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
			var randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
			var randomEnemy = aliveEnemies[randomIndex];
			GD.Print($"[{GetType().Name}] 随机选择敌人: {randomEnemy.Name}");
			return randomEnemy;
		}

		GD.Print($"[{GetType().Name}] 没有存活的敌人");
		return null;
	}

	/// <summary>
	/// 执行AOE攻击（对全体敌人）
	/// </summary>
	protected async Task ExecuteAoeAttack(PlayerChoiceContext ctx, ICombatState combatState)
	{
		var allEnemies = combatState.Enemies
			.Where(e => e.Side == CombatSide.Enemy && e.IsAlive)
			.ToList();

		if (!allEnemies.Any())
			return;

		foreach (var enemy in allEnemies)
		{
			await ExecuteAttackEffect(enemy, ctx);
		}
	}

	/// <summary>
	/// 子类实现：具体攻击效果
	/// </summary>
	protected abstract Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx);

	/// <summary>
	/// 执行绝地战备攻击（模板方法）- 保留给外部调用
	/// 战机触发时调用，目标优先级：战机目标 → 目标锁定 → 随机敌人
	/// AOE 能力则对全体敌人生效，不需要目标
	/// </summary>
	public async Task<bool> ExecuteDesperateMeasureAttack(Creature target, PlayerChoiceContext ctx)
	{
		try
		{
			if (base.Owner == null)
			{
				GD.Print($"[{GetType().Name}] Owner 为空");
				return false;
			}

			var combatState = CombatState;
			if (combatState == null)
			{
				GD.Print($"[{GetType().Name}] CombatState 为空");
				return false;
			}

			ctx ??= new ThrowingPlayerChoiceContext();

			// AOE 模式：对全体敌人生效，不需要目标解析
			if (IsAoeAttack)
			{
				GD.Print($"[{GetType().Name}] 绝地战备AOE攻击 - 对全体敌人, Amount={Amount}");
				await ExecuteAoeAttack(ctx, combatState);
				await ConsumeOrRemove(ctx);
				return true;
			}

			// 单体模式：目标优先级 战机目标 → 目标锁定 → 随机敌人
			Creature? resolved = (target != null && target.IsAlive) ? target : null;

			// 优先级2：目标锁定敌人（战机目标无效时回退）
			if (resolved == null && NeedsTargetLock)
			{
				resolved = combatState.Enemies
					.FirstOrDefault(e => e.Side == CombatSide.Enemy && e.IsAlive &&
										e.Powers.Any(p => p is TargetLockedPower));
				if (resolved != null)
					GD.Print($"[{GetType().Name}] 战机目标无效，使用目标锁定敌人: {resolved.Name}");
			}

			// 优先级3：随机存活敌人（最终保底）
			if (resolved == null)
			{
				var aliveEnemies = combatState.Enemies
					.Where(e => e.Side == CombatSide.Enemy && e.IsAlive)
					.ToList();
				if (aliveEnemies.Count > 0)
				{
					var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
					var randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
					resolved = aliveEnemies[randomIndex];
					GD.Print($"[{GetType().Name}] 战机目标无效，随机选择敌人: {resolved.Name}");
				}
			}

			if (resolved == null)
			{
				GD.Print($"[{GetType().Name}] 无有效目标，绝地战备未触发");
				return false;
			}

			GD.Print($"[{GetType().Name}] 开始执行绝地战备攻击 - Target={resolved.Name}, Damage={CurrentDamage}, Amount={Amount}");

			await ExecuteAttackEffect(resolved, ctx);

			await ConsumeOrRemove(ctx);

			GD.Print($"[{GetType().Name}] 绝地战备攻击成功");
			return true;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[{GetType().Name}] 执行绝地战备攻击失败: {ex.Message}\n{ex.StackTrace}");
			return false;
		}
	}

	/// <summary>
	/// 消耗或移除能力
	/// </summary>
	protected async Task ConsumeOrRemove(PlayerChoiceContext ctx)
	{
		int currentAmount = (int)base.Amount;
		if (currentAmount > 1)
		{
			await PowerCmd.ModifyAmount(ctx, this, -1m, base.Owner, null);
			GD.Print($"[{GetType().Name}] 消耗一层，剩余层数: {currentAmount - 1}");
		}
		else
		{
			await PowerCmd.Remove(this);
			GD.Print($"[{GetType().Name}] 能力已消耗完毕，移除");
		}
	}

	/// <summary>
	/// 通用攻击辅助：对单个目标造成多次伤害
	/// </summary>
	protected async Task AttackSingleTarget(Creature target, PlayerChoiceContext ctx, int repeatCount, string vfxPath)
	{
		if (!string.IsNullOrEmpty(vfxPath))
		{
			VfxCmd.PlayOnCreatureCenter(target, vfxPath);
		}

		for (int i = 0; i < repeatCount; i++)
		{
			await Cmd.Wait(0.1f);
			await CreatureCmd.Damage(ctx, target, (decimal)CurrentDamage, ValueProp.Move, null, null);
			GD.Print($"[{GetType().Name}] 第 {i + 1}/{repeatCount} 次攻击 - 对 {target.Name} 造成 {CurrentDamage} 点伤害");
		}
	}

	/// <summary>
	/// 通用攻击辅助：对所有敌人造成伤害
	/// </summary>
	protected async Task AttackAllEnemies(PlayerChoiceContext ctx, string vfxPath)
	{
		var combatState = CombatState;
		if (combatState == null) return;

		var allEnemies = combatState.HittableEnemies
			.Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
			.ToList();

		if (allEnemies.Count == 0) return;

		foreach (var enemy in allEnemies)
		{
			if (!string.IsNullOrEmpty(vfxPath))
			{
				VfxCmd.PlayOnCreatureCenter(enemy, vfxPath);
			}

			await Cmd.Wait(0.1f);
			await CreatureCmd.Damage(ctx, enemy, (decimal)CurrentDamage, ValueProp.Move, null, null);
			GD.Print($"[{GetType().Name}] 对 {enemy.Name} 造成 {CurrentDamage} 点伤害");
		}
	}

	/// <summary>
	/// 通用攻击辅助：对目标造成伤害并溅射
	/// </summary>
	protected async Task AttackWithSplash(Creature target, PlayerChoiceContext ctx, string mainVfxPath, string splashVfxPath, decimal splashDamageRatio)
	{
		if (!string.IsNullOrEmpty(mainVfxPath))
		{
			VfxCmd.PlayOnCreatureCenter(target, mainVfxPath);
		}

		await Cmd.Wait(0.3f);

		await CreatureCmd.Damage(ctx, target, (decimal)CurrentDamage, ValueProp.Move, null, null);
		GD.Print($"[{GetType().Name}] 对 {target.Name} 造成 {CurrentDamage} 点伤害");

		var otherEnemies = SplashDamageHelper.GetSplashTargets(target, CombatState?.HittableEnemies ?? new List<Creature>());
		if (otherEnemies.Count > 0)
		{
			decimal splashDamage = SplashDamageHelper.CalculateSplashDamage((decimal)CurrentDamage);
			GD.Print($"[{GetType().Name}] 溅射伤害 = {splashDamage}");

			foreach (Creature otherEnemy in otherEnemies)
			{
				if (!string.IsNullOrEmpty(splashVfxPath))
				{
					VfxCmd.PlayOnCreatureCenter(otherEnemy, splashVfxPath);
				}
				await Cmd.Wait(0.15f);
				await CreatureCmd.Damage(ctx, otherEnemy, splashDamage, ValueProp.Move, null, null);
				GD.Print($"[{GetType().Name}] 对 {otherEnemy.Name} 造成 {splashDamage} 点溅射伤害");
			}
		}
	}

	/// <summary>
	/// 通用 Apply 方法：为子类提供统一的能力创建/堆叠逻辑
	/// </summary>
	public static async Task<TPower?> ApplyDesperateMeasurePower<TPower>(Creature owner, bool isUpgraded, int baseDamage, int damageUpgraded)
		where TPower : DesperateMeasurePowerBase
	{
		int finalDamage = baseDamage + (isUpgraded ? damageUpgraded : 0);

		var existingPower = owner.Powers
			.OfType<TPower>()
			.FirstOrDefault(p => p.CurrentDamage == finalDamage);

		if (existingPower != null)
		{
			GD.Print($"[{typeof(TPower).Name}] 发现相同伤害值的能力，增加层数 - 伤害={finalDamage}, 当前层数={existingPower.Amount}");
			await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
			GD.Print($"[{typeof(TPower).Name}] 层数增加完成 - 新层数={existingPower.Amount}");
			return existingPower;
		}

		GD.Print($"[{typeof(TPower).Name}] 未发现相同伤害值的能力，创建新能力 - 伤害={finalDamage}");
		var power = await PowerCmd.Apply<TPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (power != null)
		{
			power.CurrentDamage = finalDamage;
			power.IsUpgraded = isUpgraded;
			GD.Print($"[{typeof(TPower).Name}] 创建成功 - Damage={power.CurrentDamage}, IsUpgraded={power.IsUpgraded}, Amount={power.Amount}");
		}
		return power;
	}
}
