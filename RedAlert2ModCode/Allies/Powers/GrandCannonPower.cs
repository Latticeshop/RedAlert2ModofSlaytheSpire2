using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class GrandCannonPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GrandCannon;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentDamage { get; set; } = (int)Values.Damage;

    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/gcanicon.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Damage", CurrentDamage);
            return locString;
        }
    }

    public static async Task ApplyGrandCannon(Creature owner, bool isUpgraded = false)
    {
        var existingPower = owner.Powers
            .OfType<GrandCannonPower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            return;
        }

        var newPower = await PowerCmd.Apply<GrandCannonPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (newPower != null)
        {
            newPower.CurrentDamage = (int)Values.Damage + (isUpgraded ? (int)Values.DamageUpgraded : 0);
            newPower.IsUpgraded = isUpgraded;
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (Owner == null) return;

        int stacks = (int)base.Amount;
        GD.Print($"[GrandCannonPower] 回合开始触发 - 层数={stacks}, Damage={CurrentDamage}");

        for (int i = 0; i < stacks; i++)
        {
            var targetLockedEnemies = combatState.Enemies
                .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive &&
                               enemy.Powers.Any(p => p is TargetLockedPower))
                .ToList();

            GD.Print($"[GrandCannonPower] 第{i+1}次攻击 - 发现 {targetLockedEnemies.Count} 个目标锁定敌人");

            if (targetLockedEnemies.Count == 0)
            {
                GD.Print("[GrandCannonPower] 没有目标锁定的敌人，随机选择一个敌人");
                var aliveEnemies = combatState.Enemies
                    .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
                    .ToList();

                if (aliveEnemies.Count > 0)
                {
                    var rng = Owner.Player?.RunState?.Rng?.CombatCardSelection;
                    var randomIndex = rng?.NextInt(aliveEnemies.Count) ?? GD.RandRange(0, aliveEnemies.Count - 1);
                    var randomEnemy = aliveEnemies[randomIndex];

                    await TargetLockedManager.ApplyTargetLocked(randomEnemy, Owner, null);
                    UnitVoiceHelper.PlayUnitVoice("GrandCannonRotate", "Allied");
                    GD.Print($"[GrandCannonPower] 已为 {randomEnemy.Name} 赋予目标锁定，播放转向音效");
                }
            }
            else
            {
                var target = targetLockedEnemies.First();
                UnitVoiceHelper.PlayUnitVoice("GrandCannonAttack", "Allied");
                GD.Print($"[GrandCannonPower] 向 {target.Name} 开火，造成 {CurrentDamage} 点伤害");

                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(),
                    new List<Creature> { target },
                    (decimal)CurrentDamage,
                    MegaCrit.Sts2.Core.ValueProps.ValueProp.Move,
                    Owner,
                    null);
            }
        }
    }
}
