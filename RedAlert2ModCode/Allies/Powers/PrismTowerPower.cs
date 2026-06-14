using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 光棱塔能力
/// 效果：自己回合开始时对随机敌人造成5点伤害1次
/// 每次叠加光棱塔，伤害和次数都+1
/// </summary>
public class PrismTowerPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 当前光棱塔的基础等级（用于计算叠加效果）
    public int PrismTowerLevel { get; set; } = 1;
    
    // 当前累积的伤害增量（每次叠加时累积）
    public int DamageIncrement { get; set; } = 0;
    
    // 当前伤害值
    public int CurrentDamage { get; set; } = 5;
    
    // 当前攻击次数
    public int CurrentHits { get; set; } = 1;

    public PrismTowerPower()
    {
        GD.Print($"[PrismTowerPower] 构造函数被调用 - Level={PrismTowerLevel}, Damage={CurrentDamage}, Hits={CurrentHits}");
    }

    /// <summary>
    /// 本地化描述
    /// </summary>
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

    /// <summary>
    /// 应用光棱塔能力
    /// </summary>
    public static async Task ApplyPrismTower(Creature owner, int level, bool isUpgraded = false)
    {
        GD.Print($"[PrismTowerPower] ApplyPrismTower 被调用 - Level={level}, IsUpgraded={isUpgraded}");
        
        // 查找是否已有光棱塔能力
        var existingPower = owner.Powers.OfType<PrismTowerPower>().FirstOrDefault();
        
        if (existingPower != null)
        {
            // 叠加效果：累积伤害增量，未升级+2，升级后+5，次数+1
            int addIncrement = isUpgraded ? 5 : 2;
            existingPower.PrismTowerLevel += 1;
            existingPower.DamageIncrement += addIncrement;
            existingPower.CurrentDamage = 5 + existingPower.DamageIncrement;
            existingPower.CurrentHits = existingPower.PrismTowerLevel;
            GD.Print($"[PrismTowerPower] 叠加效果 - NewLevel={existingPower.PrismTowerLevel}, DamageIncrement={existingPower.DamageIncrement}, Damage={existingPower.CurrentDamage}, Hits={existingPower.CurrentHits}, AddedIncrement={addIncrement}");
        }
        else
        {
            // 首次应用，创建新能力
            var newPower = await PowerCmd.Apply<PrismTowerPower>(owner, 1m, owner, null);
            if (newPower != null)
            {
                newPower.PrismTowerLevel = 1;
                newPower.DamageIncrement = 0;
                newPower.CurrentDamage = 5;
                newPower.CurrentHits = 1;
                GD.Print($"[PrismTowerPower] 首次创建 - Level={newPower.PrismTowerLevel}, DamageIncrement={newPower.DamageIncrement}, Damage={newPower.CurrentDamage}, Hits={newPower.CurrentHits}");
            }
        }
    }

    /// <summary>
    /// 回合开始时触发伤害
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        GD.Print($"[PrismTowerPower] 回合开始触发 - Level={PrismTowerLevel}, Damage={CurrentDamage}, Hits={CurrentHits}");

        // 获取所有敌人
        var enemies = combatState.Enemies.Where(static enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive).ToList();
        if (enemies.Count == 0)
            return;

        // 对随机敌人造成伤害多次
        for (int i = 0; i < CurrentHits; i++)
        {
            // 随机选择一个敌人
            var randomEnemy = enemies[GD.RandRange(0, enemies.Count - 1)];
            
            GD.Print($"[PrismTowerPower] 对敌人 {randomEnemy.Name} 造成 {CurrentDamage} 点伤害");
            
            // 造成伤害 - 使用 CreatureCmd.Damage 而不是 DamageCmd.Attack
            await CreatureCmd.Damage(new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), 
                new System.Collections.Generic.List<Creature> { randomEnemy }, 
                (decimal)CurrentDamage, 
                MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, 
                base.Owner, 
                null);
        }
    }
}
