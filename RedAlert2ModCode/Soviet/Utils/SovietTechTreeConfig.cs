using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Utils;

public static class SovietTechTreeConfig
{
    public static BuildingTechTree CreateTechTree()
    {
        var refinery = new TechBuildingInfo(typeof(SovietRefinery), TechLevel.T1, powerType: typeof(SovietRefineryPower));
        refinery.WithProductionUnlock();

        var buildings = new List<TechBuildingInfo>
        {
            // T1: 基地车解锁 - 核心建筑（科技树自动显示）
            new(typeof(NuclearReactor), TechLevel.T1),
            new(typeof(SovietBarracksCard), TechLevel.T1),
            refinery,
            
            // T2: 矿场解锁生产建筑（核心建筑，仅解锁生产选项，不升级科技等级）
            new(typeof(SovietWarFactory), TechLevel.T2, powerType: typeof(SovietWarFactoryPower)),
            new(typeof(SovietShipyardCard), TechLevel.T2),
            new(typeof(SovietRadar), TechLevel.T2, unlocksNextTech: true, powerType: typeof(SovietRadarPower)),
            
            // T2: 雷达解锁后，作战实验室在 MCV 出现，打出后升级到 T3
            new(typeof(SovietBattleLab), TechLevel.T2, unlocksNextTech: true, powerType: typeof(SovietBattleLabPower), requiredPowers: new[] { typeof(SovietRadarPower) }),
        };

        return new BuildingTechTree(buildings);
    }
}
