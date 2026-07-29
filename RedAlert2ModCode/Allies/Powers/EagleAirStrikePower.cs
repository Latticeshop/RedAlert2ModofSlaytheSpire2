using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 飞鹰空袭能力 - 绝地战备
/// 效果：对全部敌人造成伤害
/// AOE模式：不需要目标，直接对全体敌人生效
/// </summary>
public class EagleAirStrikePower : DesperateMeasurePowerBase
{
	private static readonly CardValueStore.CardValues Values = AlliesPowerValues.EagleAirStrikePower;

	public override string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/EagleAirStrikePower.png";

	/// <summary>
	/// 空袭为AOE能力，不需要目标
	/// </summary>
	protected override bool IsAoeAttack => true;

	/// <summary>
	/// 不需要目标锁定回退
	/// </summary>
	protected override bool NeedsTargetLock => false;

	/// <summary>
	/// 应用飞鹰空袭能力
	/// </summary>
	public static async Task<EagleAirStrikePower?> ApplyEagleAirStrike(Creature owner, bool isUpgraded = false)
	{
		var power = await ApplyDesperateMeasurePower<EagleAirStrikePower>(owner, isUpgraded, (int)Values.Damage, (int)Values.DamageUpgraded);
		return power;
	}

	/// <summary>
	/// 对单个敌人造成伤害（基类AOE模式会自动对所有敌人调用此方法）
	/// </summary>
	protected override async Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx)
	{
		VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_blunt");
		await Cmd.Wait(0.1f);
		await CreatureCmd.Damage(ctx, target, (decimal)CurrentDamage, ValueProp.Move, null, null);
		GD.Print($"[EagleAirStrikePower] 对 {target.Name} 造成 {CurrentDamage} 点伤害");
	}
}
