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
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

public class PillboxPower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Pillbox;
	
	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同升级状态的叠加逻辑在 ApplyPillbox 中手动处理
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public int CurrentDamage { get; set; } = (int)Values.Damage;
	
	public int CurrentBlock { get; set; } = (int)Values.Block;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

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

        // 检查是否已有相同升级状态的机枪碉堡能力
        var existingPower = owner.Powers
            .OfType<PillboxPower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            // 已有相同升级状态的能力，增加层数
            GD.Print($"[PillboxPower] 发现相同升级状态的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[PillboxPower] 增加后层数: {existingPower.Amount}");
            return;
        }
		
		var newPower = await PowerCmd.Apply<PillboxPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
			newPower.CurrentBlock = (int)Values.Block;  // 防御值升级不加
            newPower.IsUpgraded = isUpgraded;
			GD.Print($"[PillboxPower] 创建成功 - Damage={newPower.CurrentDamage}, Block={newPower.CurrentBlock}, Repeat={Values.Repeat}, IsUpgraded={newPower.IsUpgraded}");
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player)
			return;

		int stacks = (int)base.Amount;
		GD.Print($"[PillboxPower] 回合结束触发 - 层数={stacks}, Damage={CurrentDamage}, Block={CurrentBlock}, Repeat={Values.Repeat}");

		var combatState = Owner?.CombatState;
		if (combatState == null)
			return;

		var enemies = combatState.Enemies.Where(static enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();
		
		var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
		for (int i = 0; i < stacks; i++)
		{
			for (int j = 0; j < Values.Repeat; j++)
			{
				if (enemies.Count > 0)
				{
					UnitVoiceHelper.PlayUnitVoice("Pillbox", "Allied");
					var randomIndex = rng?.NextInt(enemies.Count) ?? GD.RandRange(0, enemies.Count - 1);
					var randomEnemy = enemies[randomIndex];
					GD.Print($"[PillboxPower] 第{i+1}层第{j+1}次攻击 - 对敌人 {randomEnemy.Name} 造成 {CurrentDamage} 点伤害");
					
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
				GD.Print($"[PillboxPower] 第{i+1}次触发 - 获得 {CurrentBlock} 点护盾");
				await CreatureCmd.GainBlock(base.Owner, (decimal)CurrentBlock, ValueProp.Unpowered, null);
			}
		}
	}
}