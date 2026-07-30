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
            
            new(typeof(SovietWarFactory), TechLevel.T2, powerType: typeof(SovietWarFactoryPower)),
            new(typeof(SovietShipyardCard), TechLevel.T2),
            new(typeof(SovietRadar), TechLevel.T2, powerType: typeof(SovietRadarPower)),
            new(typeof(SovietTeslaCoilCard), TechLevel.T2),
            new(typeof(BattleBunkerCard), TechLevel.T2),
            
            new(typeof(SovietBattleLab), TechLevel.T3, requiredPowers: new[] { typeof(SovietRadarPower) }, powerType: typeof(SovietBattleLabPower)),
            new(typeof(NuclearPlantCard), TechLevel.T3, requiredPowers: new[] { typeof(SovietBattleLabPower) }),
            new(typeof(IndustrialPlantCard), TechLevel.T3, requiredPowers: new[] { typeof(SovietBattleLabPower) }, powerType: typeof(IndustrialPlantPower)),
            new(typeof(IronCurtainCard), TechLevel.T3, requiredPowers: new[] { typeof(SovietBattleLabPower) }, powerType: typeof(IronCurtainPower)),
            new(typeof(NuclearMissileSiloCard), TechLevel.T3, requiredPowers: new[] { typeof(SovietBattleLabPower) }, powerType: typeof(NuclearMissileSiloPower)),
        };

        return new BuildingTechTree(buildings);
    }
}