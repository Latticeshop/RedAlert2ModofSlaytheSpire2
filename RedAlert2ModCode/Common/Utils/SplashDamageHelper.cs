#nullable enable

using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace RedAlert2ModCode.Common.Utils;

public static class SplashDamageHelper
{
    private const decimal SplashRatio = 0.5m;

    public static decimal CalculateSplashDamage(decimal mainDamage)
    {
        return mainDamage * SplashRatio;
    }

    public static List<Creature> GetSplashTargets(Creature target, IReadOnlyList<Creature> allEnemies)
    {
        if (target == null || allEnemies == null)
            return new List<Creature>();

        return allEnemies.Where(e => e != target).ToList();
    }
}