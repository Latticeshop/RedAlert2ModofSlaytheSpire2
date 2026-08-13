using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 飞鹰500kg能力 - 绝地战备
/// 效果：对目标锁定的敌人造成50点伤害并溅射
/// 使用夸张的轰击+燃烧动画效果
/// </summary>
public class Eagle500kgPower : DesperateMeasurePowerBase
{
	private static readonly CardValueStore.CardValues Values = CommonPowerValues.Eagle500kgPower;

	public override string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/Eagle500kgPower.png";

	/// <summary>
	/// 应用飞鹰500kg能力
	/// 独立叠层：相同伤害值叠加层数，不同伤害值独立存在
	/// </summary>
	public static async Task<Eagle500kgPower?> ApplyEagle500kg(Creature owner, bool isUpgraded = false)
	{
		int damage = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;

		var existingPower = owner.Powers.OfType<Eagle500kgPower>()
			.FirstOrDefault(p => p.CurrentDamage == damage);

		if (existingPower != null)
		{
			await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
			GD.Print($"[Eagle500kgPower] 叠加到已存在的500kg能力，层数: {existingPower.Amount}，Damage: {damage}");
			return existingPower;
		}

		var power = await PowerCmd.Apply<Eagle500kgPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (power != null)
		{
			power.CurrentDamage = damage;
			power.IsUpgraded = isUpgraded;
			GD.Print($"[Eagle500kgPower] 创建成功 - Damage={damage}, IsUpgraded={isUpgraded}, Amount={power.Amount}");
		}
		return power;
	}

	/// <summary>
	/// 播放轰击+燃烧特效
	/// </summary>
	private void PlayBombardmentAndFireEffect(Creature target)
	{
		try
		{
			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");
			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bloody_impact");

			var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
			if (fireVfx != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
			}

			GD.Print("[Eagle500kgPower] 轰击+燃烧特效播放完成");
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[Eagle500kgPower] 播放特效失败: {ex.Message}");
		}
	}

	/// <summary>
	/// 播放溅射特效
	/// </summary>
	private void PlaySplashEffect(Creature target)
	{
		try
		{
			var fireVfx = NFireBurningVfx.Create(target, 1f, goingRight: true);
			if (fireVfx != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
			}

			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_coin_explosion_small");
			GD.Print($"[Eagle500kgPower] 溅射特效播放完成 - 目标: {target.Name}");
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[Eagle500kgPower] 播放溅射特效失败: {ex.Message}");
		}
	}

	/// <summary>
	/// 播放核弹攻击同款爆炸音效（复用核弹井 nuclear_explosion.wav）
	/// </summary>
	private void PlayNuclearExplosionSound()
	{
		try
		{
			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Name = "Eagle500kgExplosion";
			var root = Engine.GetMainLoop() as SceneTree;
			if (root != null)
			{
				root.Root.AddChild(audioPlayer);
				var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_explosion.wav");
				if (soundFile != null)
				{
					audioPlayer.Stream = soundFile;
					audioPlayer.VolumeDb = -5;
					audioPlayer.Play();
					GD.Print("[Eagle500kgPower] 播放核弹爆炸音效");
				}
			}
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[Eagle500kgPower] 播放核弹爆炸音效失败: {ex.Message}");
		}
	}

	protected override async Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx)
	{
		// 0. 播放核弹攻击同款爆炸音效
		PlayNuclearExplosionSound();

		// 1. 播放轰击+燃烧特效
		PlayBombardmentAndFireEffect(target);
		await Cmd.Wait(0.3f);

		// 2. 造成50点伤害
		await MegaCrit.Sts2.Core.Commands.CreatureCmd.Damage(ctx, target, (decimal)CurrentDamage, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null, null);
		GD.Print($"[Eagle500kgPower] 对 {target.Name} 造成 {CurrentDamage} 点伤害");

		// 3. 溅射伤害
		var otherEnemies = SplashDamageHelper.GetSplashTargets(target, CombatState?.HittableEnemies ?? new System.Collections.Generic.List<Creature>());
		if (otherEnemies.Count > 0)
		{
			decimal splashDamage = SplashDamageHelper.CalculateSplashDamage((decimal)CurrentDamage);
			GD.Print($"[Eagle500kgPower] 溅射伤害 = {splashDamage}");

			foreach (Creature otherEnemy in otherEnemies)
			{
				PlaySplashEffect(otherEnemy);
				await Cmd.Wait(0.15f);

				await MegaCrit.Sts2.Core.Commands.CreatureCmd.Damage(ctx, otherEnemy, splashDamage, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null, null);
				GD.Print($"[Eagle500kgPower] 对 {otherEnemy.Name} 造成 {splashDamage} 点溅射伤害");
			}
		}
	}
}
