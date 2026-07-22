using System.Collections.Generic;
using System;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Common.Utils;

public static class BuildingCardUtils
{
    private static readonly Lazy<HashSet<System.Type>> _allBuildingTypes = new(BuildAllBuildingTypes);

    private static HashSet<System.Type> BuildAllBuildingTypes()
    {
        var types = new HashSet<System.Type>();
        types.UnionWith(AlliedCardRegistry.GetAllBuildingCardTypes());
        types.UnionWith(AlliedCardRegistry.GetAllDefenseTowerTypes());
        types.UnionWith(SovietCardRegistry.GetAllBuildingCardTypes());
        types.UnionWith(SovietCardRegistry.GetAllDefenseTowerTypes());

        types.Remove(typeof(AlliedMCV));
        types.Remove(typeof(SovietMCV));

        return types;
    }

    public static bool IsBuildingCard(System.Type cardType)
    {
        return _allBuildingTypes.Value.Contains(cardType);
    }
}