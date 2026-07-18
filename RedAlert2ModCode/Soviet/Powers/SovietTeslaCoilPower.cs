using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Powers;

public class SovietTeslaCoilPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.TeslaCoilCard;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public int TotalDamage { get; set; } = (int)Values.Damage;

	public SovietTeslaCoilPower()
	{
	}

	public override LocString Description
	{
		get
		{
			var chargePower = Owner?.Powers.OfType<SovietTeslaCoilChargePower>().FirstOrDefault();
			int chargeLevel = chargePower != null ? (int)chargePower.Amount : 0;
			float damageMultiplier = 1.0f + (chargeLevel * 0.5f);
			int finalDamage = (int)(TotalDamage * damageMultiplier);

			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Damage", finalDamage);
			return locString;
		}
	}

	public static async Task ApplyTeslaCoil(Creature owner, bool isUpgraded = false)
	{
		var existingPower = owner.Powers.OfType<SovietTeslaCoilPower>().FirstOrDefault();

		if (existingPower != null)
		{
			int addDamage = isUpgraded ? (int)Values.Stars : (int)Values.Damage;
			existingPower.TotalDamage += addDamage;
			GD.Print($"[TeslaCoilPower] 叠加效果 - TotalDamage={existingPower.TotalDamage}");
			await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
		}
		else
		{
			var newPower = await PowerCmd.Apply<SovietTeslaCoilPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
			if (newPower != null)
			{
				newPower.TotalDamage = isUpgraded ? (int)Values.Stars : (int)Values.Damage;
				GD.Print($"[TeslaCoilPower] 创建能力 - TotalDamage={newPower.TotalDamage}");
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player)
			return;

		var combatState = Owner?.CombatState;
		if (combatState == null)
			return;

		var enemies = combatState.Enemies.Where(e => e.Side == CombatSide.Enemy && e.IsAlive).ToList();
		if (enemies.Count == 0)
			return;

		var chargePower = Owner.Powers.OfType<SovietTeslaCoilChargePower>().FirstOrDefault();
		int chargeLevel = chargePower != null ? (int)chargePower.Amount : 0;

		float damageMultiplier = 1.0f + (chargeLevel * 0.5f);
		int finalDamage = (int)(TotalDamage * damageMultiplier);

		GD.Print($"[TeslaCoilPower] 回合结束触发 - BaseDamage={TotalDamage}, ChargeLevel={chargeLevel}, Multiplier={damageMultiplier}, FinalDamage={finalDamage}");

		if (chargeLevel > 0 && chargePower != null)
		{
			await PowerCmd.Remove(chargePower);
			GD.Print($"[TeslaCoilPower] 移除充能能力");
		}

		var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
		var randomIndex = rng?.NextInt(enemies.Count) ?? GD.RandRange(0, enemies.Count - 1);
		var randomEnemy = enemies[randomIndex];

		AudioHelper.PlayTeslaCoilChargeSound(Owner);
		await Task.Delay(500);
		AudioHelper.PlayTeslaCoilAttackSound(Owner);

		VfxCmd.PlayOnCreature(randomEnemy, "vfx/vfx_attack_lightning");

		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
			new List<Creature> { randomEnemy },
			(decimal)finalDamage,
			ValueProp.Unpowered,
			base.Owner,
			null);
	}
}
