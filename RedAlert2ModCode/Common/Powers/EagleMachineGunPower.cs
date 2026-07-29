using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using RedAlert2ModCode.Common.Utils;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 飞鹰机枪扫射能力 - 绝地战备
/// 效果：对目标锁定的敌人造成多次伤害
/// </summary>
public class EagleMachineGunPower : DesperateMeasurePowerBase
{
	private static readonly CardValueStore.CardValues Values = CommonPowerValues.EagleMachineGunPower;

	public override string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/EagleMachineGunPower.png";

	/// <summary>
	/// 攻击次数
	/// </summary>
	public int RepeatCount { get; private set; } = (int)Values.Repeat;

	protected override void UpdateDescriptionVars(LocString locString)
	{
		locString.Add("Damage", CurrentDamage);
		locString.Add("Repeat", RepeatCount);
	}

	/// <summary>
	/// 应用飞鹰机枪扫射能力
	/// </summary>
	public static async Task<EagleMachineGunPower?> ApplyEagleMachineGun(Creature owner, bool isUpgraded = false)
	{
		var power = await ApplyDesperateMeasurePower<EagleMachineGunPower>(owner, isUpgraded, (int)Values.Damage, (int)Values.DamageUpgraded);
		if (power != null)
		{
			power.RepeatCount = (int)Values.Repeat;
		}
		return power;
	}

	protected override async Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx)
	{
		await AttackSingleTarget(target, ctx, RepeatCount, "vfx/vfx_attack_slash");
	}
}