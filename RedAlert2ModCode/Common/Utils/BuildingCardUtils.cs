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

    /// <summary>
    /// 牌组建筑的科技等级需求映射（仅用于 MCV 选择面板过滤）。
    /// 核心科技建筑（TechTreeConfig 中的建筑）不使用此映射，它们在科技等级达标后自动显示。
    /// 非核心建筑（防御塔、超武、围墙等）仅当牌组中存在且科技等级达标时才显示。
    /// </summary>
    private static readonly Dictionary<System.Type, TechLevel> _deckBuildingTechLevelMap = new()
    {
        // === 盟军牌组建筑 ===
        { typeof(AlliedWallCard), TechLevel.T1 },
        { typeof(FortifiedWall), TechLevel.T1 },
        { typeof(AlliesPillboxCard), TechLevel.T1 },
        { typeof(AlliesRepairDepot), TechLevel.T2 },
        { typeof(PrismTowerCard), TechLevel.T2 },
        { typeof(PatriotMissile), TechLevel.T2 },
        { typeof(GrandCannon), TechLevel.T2 },
        { typeof(WeatherController), TechLevel.T3 },
        { typeof(ChronoSphere), TechLevel.T3 },
        { typeof(OreRefineryCard), TechLevel.T3 },

        // === 苏军牌组建筑 ===
        { typeof(SovietWallCard), TechLevel.T1 },
        { typeof(SovietFortifiedWall), TechLevel.T1 },
        { typeof(SovietPillboxCard), TechLevel.T1 },
        { typeof(BattleBunkerCard), TechLevel.T1 },
        { typeof(SovietRepairDepot), TechLevel.T2 },
        { typeof(SovietTeslaCoilCard), TechLevel.T2 },
        { typeof(SovietFlakCannon), TechLevel.T2 },
        { typeof(NuclearPlantCard), TechLevel.T3 },
        { typeof(IndustrialPlantCard), TechLevel.T3 },
        { typeof(IronCurtainCard), TechLevel.T3 },
        { typeof(NuclearMissileSiloCard), TechLevel.T3 },
    };

    /// <summary>
    /// 盟军牌组建筑类型集合（用于阵营过滤）。
    /// </summary>
    private static readonly HashSet<System.Type> _alliedDeckBuildingTypes = new()
    {
        typeof(AlliedWallCard), typeof(FortifiedWall),
        typeof(AlliesPillboxCard), typeof(AlliesRepairDepot),
        typeof(PrismTowerCard), typeof(PatriotMissile), typeof(GrandCannon),
        typeof(WeatherController), typeof(ChronoSphere), typeof(OreRefineryCard),
    };

    /// <summary>
    /// 苏军牌组建筑类型集合（用于阵营过滤）。
    /// </summary>
    private static readonly HashSet<System.Type> _sovietDeckBuildingTypes = new()
    {
        typeof(SovietWallCard), typeof(SovietFortifiedWall),
        typeof(SovietPillboxCard), typeof(BattleBunkerCard),
        typeof(SovietRepairDepot), typeof(SovietTeslaCoilCard), typeof(SovietFlakCannon),
        typeof(NuclearPlantCard), typeof(IndustrialPlantCard),
        typeof(IronCurtainCard), typeof(NuclearMissileSiloCard),
    };

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

    /// <summary>
    /// 获取牌组建筑的科技等级需求。
    /// 用于 MCV 选择面板判断是否应显示该建筑选项。
    /// </summary>
    /// <returns>科技等级，或 null 表示不是牌组建筑（可能是核心科技建筑）</returns>
    public static TechLevel? GetDeckBuildingTechLevel(System.Type cardType)
    {
        return _deckBuildingTechLevelMap.TryGetValue(cardType, out var level) ? level : null;
    }

    /// <summary>
    /// 判断卡牌是否为牌组建筑（非核心科技建筑）。
    /// 牌组建筑需要满足科技等级需求才会在 MCV 中显示。
    /// </summary>
    public static bool IsDeckBuildingCard(System.Type cardType)
    {
        return _deckBuildingTechLevelMap.ContainsKey(cardType);
    }

    /// <summary>
    /// 判断牌组建筑是否属于指定阵营。
    /// 用于 MCV 选择面板过滤，盟军MCV只能造盟军建筑，苏军MCV只能造苏军建筑。
    /// </summary>
    public static bool IsDeckBuildingOfFaction(System.Type cardType, FactionType faction)
    {
        if (faction == FactionType.Allied)
            return _alliedDeckBuildingTypes.Contains(cardType);
        if (faction == FactionType.Soviet)
            return _sovietDeckBuildingTypes.Contains(cardType);
        return false;
    }
}