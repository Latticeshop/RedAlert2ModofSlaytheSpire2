using System.Collections.Generic;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Utils;

public static class AlliedTechTreeConfig
{
    public static BuildingTechTree CreateTechTree()
    {
        var refinery = new TechBuildingInfo(typeof(AlliedRefinery), TechLevel.T1, powerType: typeof(AlliedRefineryPower));
        refinery.WithProductionUnlock();

        var buildings = new List<TechBuildingInfo>
        {
            // T1: 基地车解锁 - 核心建筑（科技树自动显示）
            new(typeof(PowerPlantCard), TechLevel.T1),
            new(typeof(AlliesBarracksCard), TechLevel.T1),
            refinery,
            
            // T2: 矿场解锁生产建筑（核心建筑，仅解锁生产选项，不升级科技等级）
            new(typeof(AlliedWarFactory), TechLevel.T2, powerType: typeof(AlliedWarFactoryPower)),
            new(typeof(AlliesShipyardCard), TechLevel.T2),
            new(typeof(AirForceCommand), TechLevel.T2, unlocksNextTech: true, powerType: typeof(AlliedAirForceCommandPower)),
            
            // T2: 空指部解锁后，作战实验室在 MCV 出现，打出后升级到 T3
            new(typeof(AlliedBattleLab), TechLevel.T2, unlocksNextTech: true, powerType: typeof(BattleLabPower), requiredPowers: new[] { typeof(AlliedAirForceCommandPower) }),
        };

        return new BuildingTechTree(buildings);
    }
}
