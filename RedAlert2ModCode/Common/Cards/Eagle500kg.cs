using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 飞鹰500kg - 绝地战备攻击牌
/// 3费，Rare金卡
/// 效果：获得飞鹰500kg能力，指定敌人获得目标锁定
/// </summary>
public sealed class Eagle500kg : DesperateMeasureCardBase<Eagle500kgPower>
{
	public Eagle500kg() : base(3, CardRarity.Rare, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/Eagle500kg.png";

	protected override CardValueStore.CardValues Values => CommonCardValues.Eagle500kg;

	/// <summary>
	/// 500kg 卡牌无伤害变量（伤害由能力描述显示）
	/// </summary>
	protected override bool ShowDamageVar => false;

	protected override async Task<Eagle500kgPower?> ApplyPower(Creature owner, bool isUpgraded)
	{
		return await Eagle500kgPower.ApplyEagle500kg(owner, isUpgraded);
	}

	/// <summary>
	/// 添加溅射悬停提示
	/// </summary>
	protected override void AddExtraHoverTips(List<IHoverTip> tips)
	{
		tips.Add(ModCardKeywords.Splash.CreateHoverTip()!);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy((int)Values.CostUpgraded);
	}
}