using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using Godot;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 绝地战备卡牌基类
/// 提供绝地战备卡牌的公共逻辑：卡池、悬停提示、打出流程
/// 子类只需提供能力创建方法、数值引用和肖像路径
/// </summary>
public abstract class DesperateMeasureCardBase<TPower> : CardModel, IDesperateMeasureCard where TPower : DesperateMeasurePowerBase
{
	private readonly TargetType _targetType;

	/// <summary>
	/// 构造函数
	/// </summary>
	protected DesperateMeasureCardBase(int cost, CardRarity rarity, TargetType target)
		: base(cost, CardType.Attack, rarity, target)
	{
		_targetType = target;
	}

	/// <summary>
	/// 数值引用 - 用于显示变量（伤害、次数等）
	/// </summary>
	protected abstract CardValueStore.CardValues Values { get; }

	/// <summary>
	/// 是否需要目标锁定 - 基于目标类型自动判断
	/// </summary>
	protected virtual bool NeedsTargetLock => _targetType == TargetType.AnyEnemy;

	/// <summary>
	/// 是否在描述中显示伤害变量
	/// </summary>
	protected virtual bool ShowDamageVar => Values.Damage > 0;

	/// <summary>
	/// 是否在描述中显示次数变量
	/// </summary>
	protected virtual bool ShowRepeatVar => Values.Repeat > 0;

	/// <summary>
	/// 运行时卡池
	/// </summary>
	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	/// <summary>
	/// 视觉卡池
	/// </summary>
	public override CardPoolModel VisualCardPool => Pool;

	/// <summary>
	/// 默认动态变量
	/// </summary>
	protected override List<DynamicVar> CanonicalVars
	{
		get
		{
			var vars = new List<DynamicVar>();
			if (ShowDamageVar)
			{
				vars.Add(new DamageVar(Values.Damage + (IsUpgraded ? Values.DamageUpgraded : 0m), ValueProp.Move));
			}
			if (ShowRepeatVar)
			{
				vars.Add(new RepeatVar(Values.Repeat));
			}
			return vars;
		}
	}

	/// <summary>
	/// 默认悬停提示 - 自动添加绝地战备关键词
	/// </summary>
	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			var tips = new List<IHoverTip>
			{
				ModCardKeywords.DesperateMeasure.CreateHoverTip()!
			};

			if (NeedsTargetLock)
			{
				tips.Add(HoverTipFactory.FromPower<TargetLockedPower>());
			}

			AddExtraHoverTips(tips);
			return tips;
		}
	}

	/// <summary>
	/// 子类添加额外悬停提示（如 Splash）
	/// </summary>
	protected virtual void AddExtraHoverTips(List<IHoverTip> tips) { }

	/// <summary>
	/// 子类实现：创建并应用能力
	/// </summary>
	protected abstract Task<TPower?> ApplyPower(Creature owner, bool isUpgraded);

	/// <summary>
	/// 卡牌打出流程：应用能力 → 存储目标 → 指向性卡牌赋予目标锁定
	/// </summary>
	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[{GetType().Name}] 卡牌打出开始");

		var power = await ApplyPower(Owner.Creature, IsUpgraded);

		if (power != null)
		{
			GD.Print($"[{GetType().Name}] 成功获得绝地战备能力 - Damage={power.CurrentDamage}");

			// 存储卡牌打出时的目标到能力（优先级最高）
			if (play.Target != null && play.Target.IsAlive)
			{
				power.StoredTarget = play.Target;
				GD.Print($"[{GetType().Name}] 存储目标到能力: {play.Target.Name}");

				// 指向性卡牌（非AOE）：赋予目标锁定 debuff
				if (NeedsTargetLock)
				{
					await TargetLockedManager.ApplyTargetLocked(play.Target, Owner.Creature, this);
					GD.Print($"[{GetType().Name}] 赋予目标锁定: {play.Target.Name}");
				}
			}
		}
		else
		{
			GD.PrintErr($"[{GetType().Name}] 获得绝地战备能力失败");
			return;
		}

		GD.Print($"[{GetType().Name}] 卡牌打出完成");
	}
}