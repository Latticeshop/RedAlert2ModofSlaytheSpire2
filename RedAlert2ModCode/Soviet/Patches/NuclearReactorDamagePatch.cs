using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Soviet.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class NuclearReactorDamagePatch
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
        if (target == null || !target.IsAlive || result == null || result.UnblockedDamage <= 0)
            return;

        var reactorPowers = target.Powers?.OfType<NuclearReactorCorePower>().ToList();
        if (reactorPowers == null || reactorPowers.Count == 0)
            return;

        int eventHashCode = target.GetHashCode()
                            ^ RuntimeHelpers.GetHashCode(result)
                            ^ (dealer != null ? RuntimeHelpers.GetHashCode(dealer) : 0)
                            ^ (cardSource != null ? RuntimeHelpers.GetHashCode(cardSource) : 0);

        if (!_processedGlobalEvents.Add(eventHashCode))
            return;

        const int maxEvents = 4096;
        if (_processedGlobalEvents.Count > maxEvents)
            _processedGlobalEvents.Clear();

        foreach (var reactor in reactorPowers)
        {
            reactor.OnUnblockedDamageReceived((int)result.UnblockedDamage, eventHashCode);
        }
    }
}
