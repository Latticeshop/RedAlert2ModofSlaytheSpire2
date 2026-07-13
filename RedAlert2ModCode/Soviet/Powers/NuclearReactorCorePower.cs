#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class NuclearReactorCorePower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.NuclearReactorCorePower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentEnergy { get; set; } = (int)Values.MagicNumber;
    public int CurrentHealth { get; set; } = (int)Values.Damage;
    public bool IsUpgraded { get; set; } = false;

    private bool _isExploding = false;
    private readonly HashSet<int> _processedDamageEventIds = new();

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nrcticon.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            int energyAmount = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : CurrentEnergy;
            int healthAmount = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
            locString.Add("Energy", energyAmount);
            locString.Add("Health", healthAmount);
            locString.Add("CurrentHealth", CurrentHealth);
            locString.Add("Poison", (int)Values.Repeat);
            return locString;
        }
    }

    public static async Task<NuclearReactorCorePower?> ApplyNuclearReactorCore(Creature owner, bool isUpgraded = false)
    {
        var power = await PowerCmd.Apply<NuclearReactorCorePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentEnergy = isUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
            power.CurrentHealth = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
            power.IsUpgraded = isUpgraded;
            GD.Print($"[NuclearReactorCorePower] 新建独立实例，升级={isUpgraded}，能量={power.CurrentEnergy}，血量={power.CurrentHealth}");
        }
        return power;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner != null && Owner.IsAlive)
        {
            await PlayerCmd.GainEnergy(CurrentEnergy, Owner.Player);
            GD.Print($"[NuclearReactorCorePower] 回合开始获得 {CurrentEnergy} 点能量");
        }
    }

    public void OnUnblockedDamageReceived(int unblockedDamage, int eventHashCode)
    {
        if (Owner == null || !Owner.IsAlive || unblockedDamage <= 0)
            return;

        if (_isExploding)
        {
            GD.Print($"[NuclearReactorCorePower] 爆炸进行中，忽略伤害 {unblockedDamage}");
            return;
        }

        if (!_processedDamageEventIds.Add(eventHashCode))
        {
            GD.Print($"[NuclearReactorCorePower] 同一次伤害已处理过，忽略重复事件 {eventHashCode:X8}");
            return;
        }

        CurrentHealth -= unblockedDamage;
        GD.Print($"[NuclearReactorCorePower] 受到 {unblockedDamage} 点未格挡伤害，当前血量: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            _ = ExplodeAsync();
        }
    }

    private async Task ExplodeAsync()
    {
        _isExploding = true;
        try
        {
            try
            {
                AudioHelper.PlayRandomExplosionSound();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[NuclearReactorCorePower] 播放爆炸音效失败: {ex.Message}");
            }

            var combatState = Owner!.CombatState;
            if (combatState == null)
                return;

            int poisonAmount = (int)Values.Repeat;

            var allEnemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            foreach (var enemy in allEnemies)
            {
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(new ThrowingPlayerChoiceContext(), new List<Creature> { enemy }, (decimal)poisonAmount, Owner, null);
            }

            var allPlayers = combatState.PlayerCreatures.Where(p => p.IsAlive).ToList();
            foreach (var player in allPlayers)
            {
                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(new ThrowingPlayerChoiceContext(), new List<Creature> { player }, (decimal)poisonAmount, Owner, null);
            }

            GD.Print($"[NuclearReactorCorePower] 爆炸！对全体敌人和玩家赋予 {poisonAmount} 层中毒");

            if (Amount > 1)
            {
                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1m, Owner, null);

                int maxHealth = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
                CurrentHealth = maxHealth;
                _processedDamageEventIds.Clear();
                _isExploding = false;
                GD.Print($"[NuclearReactorCorePower] 移除一层能力，剩余层数: {Amount}，重置血量为 {CurrentHealth}，允许接受后续伤害");
            }
            else
            {
                await PowerCmd.Remove(this);
                GD.Print("[NuclearReactorCorePower] 移除最后一层能力");
            }
        }
        finally
        {
            if (Owner == null || !Owner.Powers.Contains(this))
            {
                _isExploding = true;
            }
        }
    }
}
