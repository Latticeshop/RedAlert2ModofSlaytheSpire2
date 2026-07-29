using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 飞鹰机枪扫射 - 绝地战备攻击牌
/// 1费，Common蓝卡
/// 效果：获得飞鹰机枪扫射能力
/// </summary>
public sealed class EagleMachineGun : DesperateMeasureCardBase<EagleMachineGunPower>
{
	public EagleMachineGun() : base(1, CardRarity.Common, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/EagleMachineGun.png";

	protected override CardValueStore.CardValues Values => CommonCardValues.EagleMachineGun;

	protected override async Task<EagleMachineGunPower?> ApplyPower(Creature owner, bool isUpgraded)
	{
		return await EagleMachineGunPower.ApplyEagleMachineGun(owner, isUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}