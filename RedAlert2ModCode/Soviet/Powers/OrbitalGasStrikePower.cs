using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 轨道毒气能力 - 轨道战备能力
/// 回合开始时对全体敌人赋予中毒层数
/// </summary>
public class OrbitalGasStrikePower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.OrbitalGasStrikePower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentPoison { get; set; } = (int)Values.MagicNumber;

    public bool IsUpgraded { get; set; } = false;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Orbital/OrbitalGasStrike.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Poison", CurrentPoison);
            return locString;
        }
    }

    public static async Task<OrbitalGasStrikePower?> ApplyOrbitalGasStrike(Creature owner, bool isUpgraded = false)
    {
        int poison = (int)Values.MagicNumber + (isUpgraded ? (int)Values.MagicNumberUpgraded : 0);

        var existingPower = owner.Powers.OfType<OrbitalGasStrikePower>().FirstOrDefault(p => p.CurrentPoison == poison);

        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[OrbitalGasStrikePower] 叠加到已存在的毒气能力，层数: {existingPower.Amount}，Poison: {poison}");
            return existingPower;
        }

        var newPower = await PowerCmd.Apply<OrbitalGasStrikePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (newPower != null)
        {
            newPower.CurrentPoison = poison;
            newPower.IsUpgraded = isUpgraded;
            GD.Print($"[OrbitalGasStrikePower] 创建成功 - Poison={poison}, IsUpgraded={isUpgraded}");
        }
        return newPower;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        int stacks = (int)base.Amount;
        GD.Print($"[OrbitalGasStrikePower] 回合开始触发 - 层数={stacks}, Poison={CurrentPoison}");

        for (int i = 0; i < stacks; i++)
        {
            var allEnemies = combatState.Enemies
                .Where(e => e.Side == CombatSide.Enemy && e.IsAlive)
                .ToList();

            if (allEnemies.Count == 0)
            {
                GD.Print("[OrbitalGasStrikePower] 没有存活的敌人，跳过");
                break;
            }

            foreach (var enemy in allEnemies)
            {
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(
                    new ThrowingPlayerChoiceContext(), enemy, (decimal)CurrentPoison, Owner, null);
            }

            GD.Print($"[OrbitalGasStrikePower] 第{i + 1}次触发 - 对全体敌人赋予 {CurrentPoison} 层中毒");
        }

        await PowerCmd.Remove(this);
    }
}