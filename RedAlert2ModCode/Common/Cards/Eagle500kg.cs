using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Combat;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using Godot;
namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 飞鹰500kg - 绝地战备攻击牌（公共）
/// 3费，Rare金卡
/// 效果：获得飞鹰500kg能力，指定一名敌人获得目标锁定
/// 描述：[gold]绝地战备[/gold]。↑→↓↓↓。[gold]溅射[/gold]。
/// </summary>

public sealed class Eagle500kg : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Eagle500kg;

	public Eagle500kg() : base((int)Values.Cost, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
    public override CardPoolModel VisualCardPool => Pool;

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Eagle500kg.png";

            protected override List<DynamicVar> CanonicalVars => new()
	{
		// 绝地战备卡牌没有常规数值变量
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.DesperateMeasure.CreateHoverTip()!,
		HoverTipFactory.FromPower<TargetLockedPower>(),
		ModCardKeywords.Splash.CreateHoverTip()!
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print("[Eagle500kg] 卡牌打出开始");

		// 1. 获得飞鹰500kg能力
		var eagle500kgPower = await Eagle500kgPower.ApplyEagle500kg(Owner.Creature, IsUpgraded);
		if (eagle500kgPower != null)
		{
			GD.Print($"[Eagle500kg] 成功获得飞鹰500kg能力 - Damage={eagle500kgPower.CurrentDamage}");
		}
		else
		{
			GD.PrintErr("[Eagle500kg] 获得飞鹰500kg能力失败");
		}

		// 2. 指定一名敌人获得目标锁定debuff
		if (play.Target != null && play.Target.IsAlive)
		{
			await TargetLockedManager.ApplyTargetLocked(play.Target, Owner.Creature, this);
		}
		else
		{
			GD.PrintErr("[Eagle500kg] 没有有效目标");
		}

		GD.Print("[Eagle500kg] 卡牌打出完成");
	}

	protected override void OnUpgrade()
	{
		// 升级后费用降低（从数值存储获取）
		EnergyCost.UpgradeBy((int)Values.CostUpgraded);
		GD.Print($"[Eagle500kg] 卡牌升级 - 费用降低 {Values.CostUpgraded}");
	}
}