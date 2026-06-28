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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

public class PrismTowerPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesPowerValues.PrismTowerPower;
	
	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;
    
	public int PrismTowerLevel { get; set; } = 1;
    
	public int DamageIncrement { get; set; } = 0;
    
	public int CurrentDamage { get; set; } = (int)Values.Damage;
    
	public int CurrentHits { get; set; } = (int)Values.Repeat;

	public PrismTowerPower()
	{
		GD.Print($"[PrismTowerPower] 构造函数被调用 - Level={PrismTowerLevel}, Damage={CurrentDamage}, Hits={CurrentHits}");
	}

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Damage", CurrentDamage);
			locString.Add("Repeat", CurrentHits);
			return locString;
		}
	}

	public static async Task ApplyPrismTower(Creature owner, int level, bool isUpgraded = false)
	{
		GD.Print($"[PrismTowerPower] ApplyPrismTower 被调用 - Level={level}, IsUpgraded={isUpgraded}");
		
		var existingPower = owner.Powers.OfType<PrismTowerPower>().FirstOrDefault();
		
		if (existingPower != null)
		{
			int addIncrement = isUpgraded ? (int)(Values.Stars + Values.StarsUpgraded) : (int)Values.Stars;
			existingPower.PrismTowerLevel += 1;
			existingPower.DamageIncrement += addIncrement;
			existingPower.CurrentDamage = (int)Values.Damage + existingPower.DamageIncrement;
			existingPower.CurrentHits = existingPower.PrismTowerLevel;
			GD.Print($"[PrismTowerPower] 叠加效果 - NewLevel={existingPower.PrismTowerLevel}, DamageIncrement={existingPower.DamageIncrement}, Damage={existingPower.CurrentDamage}, Hits={existingPower.CurrentHits}, AddedIncrement={addIncrement}");
			
			await CreatureCmd.TriggerAnim(owner, "Cast", 0.3f);
		}
		else
		{
			var newPower = await PowerCmd.Apply<PrismTowerPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
			if (newPower != null)
			{
				newPower.PrismTowerLevel = 1;
				newPower.DamageIncrement = 0;
				newPower.CurrentDamage = (int)Values.Damage;
				newPower.CurrentHits = (int)Values.Repeat;
				GD.Print($"[PrismTowerPower] 首次创建 - Level={newPower.PrismTowerLevel}, DamageIncrement={newPower.DamageIncrement}, Damage={newPower.CurrentDamage}, Hits={newPower.CurrentHits}");
			}
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		GD.Print($"[PrismTowerPower] 回合开始触发 - Level={PrismTowerLevel}, Damage={CurrentDamage}, Hits={CurrentHits}");

		var enemies = combatState.Enemies.Where(static enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();
		if (enemies.Count == 0)
			return;

		List<Creature> targetList = new List<Creature>();
		for (int i = 0; i < CurrentHits; i++)
		{
			var randomEnemy = enemies[GD.RandRange(0, enemies.Count - 1)];
			targetList.Add(randomEnemy);
		}

		if (targetList.Count > 0)
		{
			AudioHelper.PlayPrismTowerChargeSound(Owner);
			await Task.Delay(500);
			AudioHelper.PlayPrismTowerAttackSound(Owner);

			NSweepingBeamVfx? beamVfx = NSweepingBeamVfx.Create(Owner, targetList);
			if (beamVfx != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChild(beamVfx);
				GD.Print("[PrismTowerPower] 射线动画播放成功");
			}
		}

		for (int i = 0; i < targetList.Count; i++)
		{
			var enemy = targetList[i];
			GD.Print($"[PrismTowerPower] 对敌人 {enemy.Name} 造成 {CurrentDamage} 点伤害");
			
			await CreatureCmd.Damage(new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), 
				new System.Collections.Generic.List<Creature> { enemy }, 
				(decimal)CurrentDamage, 
				MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, 
				base.Owner, 
				null);
		}
	}
}
