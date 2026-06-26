using System.Collections.Generic;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Utils;

public static class AlliedTechTreeConfig
{
    public static BuildingTechTree CreateTechTree()
    {
        var buildings = new List<TechBuildingInfo>
        {
            new(typeof(PowerPlantCard), TechLevel.T1),
            new(typeof(AlliesBarracksCard), TechLevel.T1),
            new(typeof(AlliedRefinery), TechLevel.T1, unlocksNextTech: true, powerType: typeof(AlliedRefineryPower)),
            
            new(typeof(AlliedWarFactory), TechLevel.T2, unlocksNextTech: true, powerType: typeof(AlliedWarFactoryPower)),
            
            new(typeof(AirForceCommand), TechLevel.T3),
            new(typeof(AlliesShipyardCard), TechLevel.T3),
            new(typeof(AlliedBattleLab), TechLevel.T3),
        };

        return new BuildingTechTree(buildings);
    }
}