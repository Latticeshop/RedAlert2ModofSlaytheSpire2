using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

public class PillboxPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Pillbox;
	
	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;

	public int CurrentDamage { get; set; } = (int)Values.Damage;
	
	public int CurrentBlock { get; set; } = (int)Values.Block;

	public PillboxPower()
	{
		GD.Print($"[PillboxPower] 构造函数被调用 - Damage={CurrentDamage}, Block={CurrentBlock}");
	}

	/// <summary>
	/// 使用机枪碉堡卡牌的图标
	/// 注意：Icon属性使用的是PackedIconPath，所以必须重写这个属性
	/// </summary>
	public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/pillicon.png";

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Damage", CurrentDamage);
			locString.Add("Block", CurrentBlock);
			return locString;
		}
	}

	public static async Task ApplyPillbox(Creature owner, bool isUpgraded = false)
	{
		GD.Print($"[PillboxPower] ApplyPillbox 被调用 - IsUpgraded={isUpgraded}");
		
		var newPower = await PowerCmd.Apply<PillboxPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
			newPower.CurrentBlock = (int)Values.Block + (isUpgraded ? (int)Values.BlockUpgraded : 0);
			GD.Print($"[PillboxPower] 创建成功 - Damage={newPower.CurrentDamage}, Block={newPower.CurrentBlock}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		// 获取当前层数，按层数循环触发
		int stacks = (int)base.Amount;
		GD.Print($"[PillboxPower] 回合开始触发 - 层数={stacks}, Damage={CurrentDamage}, Block={CurrentBlock}");

		var enemies = combatState.Enemies.Where(static enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();
		
		for (int i = 0; i < stacks; i++)
		{
			if (enemies.Count > 0)
			{
				var randomEnemy = enemies[GD.RandRange(0, enemies.Count - 1)];
				GD.Print($"[PillboxPower] 第{i+1}次触发 - 对敌人 {randomEnemy.Name} 造成 {CurrentDamage} 点伤害");
				
				await CreatureCmd.Damage(new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), 
					new List<Creature> { randomEnemy }, 
					(decimal)CurrentDamage, 
					MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, 
					base.Owner, 
					null);
			}

			if (base.Owner != null)
			{
				GD.Print($"[PillboxPower] 第{i+1}次触发 - 获得 {CurrentBlock} 点护盾");
				await CreatureCmd.GainBlock(base.Owner, (decimal)CurrentBlock, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
			}
		}
	}
}