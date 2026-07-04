#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class SovietPillboxPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietPillbox;
	
	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public int CurrentDamage { get; set; } = (int)Values.Damage;
	
	public int CurrentBlock { get; set; } = (int)Values.Block;

    public bool IsUpgraded { get; set; } = false;

	public SovietPillboxPower()
	{
		GD.Print($"[SovietPillboxPower] 构造函数被调用 - Damage={CurrentDamage}, Block={CurrentBlock}");
	}

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

	public static async Task ApplySovietPillbox(Creature owner, bool isUpgraded = false)
	{
		GD.Print($"[SovietPillboxPower] ApplySovietPillbox 被调用 - IsUpgraded={isUpgraded}");

        var existingPower = owner.Powers
            .OfType<SovietPillboxPower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            GD.Print($"[SovietPillboxPower] 发现相同升级状态的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[SovietPillboxPower] 增加后层数: {existingPower.Amount}");
            return;
        }
		
		var newPower = await PowerCmd.Apply<SovietPillboxPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
			newPower.CurrentBlock = (int)Values.Block;
            newPower.IsUpgraded = isUpgraded;
			GD.Print($"[SovietPillboxPower] 创建成功 - Damage={newPower.CurrentDamage}, Block={newPower.CurrentBlock}, Repeat={Values.Repeat}, IsUpgraded={newPower.IsUpgraded}");
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		int stacks = (int)base.Amount;
		GD.Print($"[SovietPillboxPower] 回合开始触发 - 层数={stacks}, Damage={CurrentDamage}, Block={CurrentBlock}, Repeat={Values.Repeat}");

		var enemies = combatState.Enemies.Where(static enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();
		
		var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
		for (int i = 0; i < stacks; i++)
		{
			for (int j = 0; j < Values.Repeat; j++)
			{
				if (enemies.Count > 0)
				{
					var randomIndex = rng?.NextInt(enemies.Count) ?? GD.RandRange(0, enemies.Count - 1);
					var randomEnemy = enemies[randomIndex];
					GD.Print($"[SovietPillboxPower] 第{i+1}层第{j+1}次攻击 - 对敌人 {randomEnemy.Name} 造成 {CurrentDamage} 点伤害");
					
					await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), 
						new List<Creature> { randomEnemy }, 
						(decimal)CurrentDamage, 
						ValueProp.Unpowered, 
						base.Owner, 
						null);
				}
			}

			if (base.Owner != null)
			{
				GD.Print($"[SovietPillboxPower] 第{i+1}次触发 - 获得 {CurrentBlock} 点护盾");
				await CreatureCmd.GainBlock(base.Owner, (decimal)CurrentBlock, ValueProp.Unpowered, null);
			}
		}
	}
}