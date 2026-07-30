using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Allies.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// 伪装伤害追踪补丁
/// 1. 玩家造成伤害时标记DamageDealtTrackerPower
/// 2. 若玩家有CamouflagePower：移除伪装（伪装自带无实体效果，移除即失去效果）
/// </summary>
[HarmonyPatch]
public static class CamouflageDamagePatch
{
    private static MethodBase TargetMethod()
    {
        return typeof(RelicModel).GetMethod("AfterDamageReceived",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[]
            {
                typeof(PlayerChoiceContext),
                typeof(Creature),
                typeof(DamageResult),
                typeof(ValueProp),
                typeof(Creature),
                typeof(CardModel)
            },
            null);
    }

    private static readonly HashSet<int> _processedGlobalEvents = new();

    private static async void Postfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == null || dealer.Side != CombatSide.Player)
            return;

        if (result == null || result.UnblockedDamage <= 0)
            return;

        int eventHashCode = RuntimeHelpers.GetHashCode(target)
                            ^ RuntimeHelpers.GetHashCode(result)
                            ^ RuntimeHelpers.GetHashCode(dealer)
                            ^ (cardSource != null ? RuntimeHelpers.GetHashCode(cardSource) : 0);

        if (!_processedGlobalEvents.Add(eventHashCode))
            return;

        const int maxEvents = 4096;
        if (_processedGlobalEvents.Count > maxEvents)
            _processedGlobalEvents.Clear();

        GD.Print($"[CamouflageDamagePatch] 玩家造成伤害: {result.UnblockedDamage}");

        // 1. 标记本回合已造成伤害
        var existingTracker = dealer.Powers?.OfType<DamageDealtTrackerPower>().FirstOrDefault();
        if (existingTracker == null)
        {
            GD.Print("[CamouflageDamagePatch] 添加伤害追踪标记");
            await PowerCmd.Apply<DamageDealtTrackerPower>(choiceContext, dealer, 1m, dealer, null);
        }

        // 2. 若玩家有伪装能力，移除伪装（内置无实体效果，无需额外处理）
        var camouflagePowers = dealer.Powers?.OfType<CamouflagePower>().ToList();
        if (camouflagePowers != null && camouflagePowers.Count > 0)
        {
            GD.Print("[CamouflageDamagePatch] 玩家有伪装能力，移除伪装");
            foreach (var camo in camouflagePowers)
            {
                await PowerCmd.Remove(camo);
            }
        }
    }
}
