using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Soviet.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class SovietEarlyMiningPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/early_mining_soviet_power.png";
    public new AbstractModel OriginModel => ModelDb.Card<SovietEarlyMining>();

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MiningMultiplierPercent", 80)
    };

    public float GetMiningMultiplier()
    {
        return DynamicVars["MiningMultiplierPercent"].IntValue / 100f;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner?.Side)
        {
            await PowerCmd.Remove(this);
            GD.Print("[SovietEarlyMiningPower] 回合开始，移除提前倒矿debuff");
        }
    }
}