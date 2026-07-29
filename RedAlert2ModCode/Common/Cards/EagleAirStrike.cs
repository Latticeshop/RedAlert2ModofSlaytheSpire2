using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 飞鹰空袭 - 绝地战备攻击牌
/// 1费，Uncommon蓝卡
/// 效果：获得飞鹰空袭能力，对全部敌人造成伤害
/// </summary>
public sealed class EagleAirStrike : DesperateMeasureCardBase<EagleAirStrikePower>
{
	public EagleAirStrike() : base(1, CardRarity.Uncommon, TargetType.AllEnemies) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/EagleAirStrike.png";

	protected override CardValueStore.CardValues Values => CommonCardValues.EagleAirStrike;

	/// <summary>
	/// 空袭不需要目标锁定
	/// </summary>
	protected override bool NeedsTargetLock => false;

	protected override async Task<EagleAirStrikePower?> ApplyPower(Creature owner, bool isUpgraded)
	{
		return await EagleAirStrikePower.ApplyEagleAirStrike(owner, isUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}