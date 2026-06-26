using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Utils;

public static class SovietTechTreeConfig
{
    public static BuildingTechTree CreateTechTree()
    {
        var buildings = new List<TechBuildingInfo>
        {
            new(typeof(NuclearReactor), TechLevel.T1),
            new(typeof(SovietBarracksCard), TechLevel.T1),
            new(typeof(SovietRefinery), TechLevel.T1, unlocksNextTech: true, powerType: typeof(SovietRefineryPower)),
            
            new(typeof(SovietWarFactory), TechLevel.T2, unlocksNextTech: true, powerType: typeof(SovietWarFactoryPower)),
            
            new(typeof(SovietShipyardCard), TechLevel.T3),
            new(typeof(SovietRadar), TechLevel.T3),
            new(typeof(SovietBattleLab), TechLevel.T3),
        };

        return new BuildingTechTree(buildings);
    }
}