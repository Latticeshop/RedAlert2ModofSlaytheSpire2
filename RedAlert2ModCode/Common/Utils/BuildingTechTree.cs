using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Common.Utils;

public enum TechLevel
{
    None,
    T1,
    T2,
    T3
}

public class TechBuildingInfo
{
    public System.Type BuildingType { get; set; }
    public TechLevel RequiredTech { get; set; }
    public bool UnlocksNextTech { get; set; }
    public System.Type? PowerType { get; set; }
    
    public TechBuildingInfo(System.Type buildingType, TechLevel requiredTech, bool unlocksNextTech = false, System.Type? powerType = null)
    {
        BuildingType = buildingType;
        RequiredTech = requiredTech;
        UnlocksNextTech = unlocksNextTech;
        PowerType = powerType;
    }
}

public class BuildingTechTree
{
    public TechLevel CurrentTechLevel { get; private set; } = TechLevel.T1;
    
    private readonly Dictionary<System.Type, TechBuildingInfo> _buildingTechMap = new();
    
    public BuildingTechTree(IEnumerable<TechBuildingInfo> buildings)
    {
        foreach (var building in buildings)
        {
            _buildingTechMap[building.BuildingType] = building;
        }
    }
    
    public bool IsBuildingUnlocked(System.Type buildingType)
    {
        if (!_buildingTechMap.TryGetValue(buildingType, out var info))
        {
            return false;
        }
        return CurrentTechLevel >= info.RequiredTech;
    }
    
    public bool IsBuildingCardUnlocked(CardModel card)
    {
        return IsBuildingUnlocked(card.GetType());
    }
    
    public void UnlockTechFromPowers(IEnumerable<object> powers)
    {
        foreach (var power in powers)
        {
            var powerType = power.GetType();
            var buildingInfo = _buildingTechMap.Values.FirstOrDefault(info => info.PowerType == powerType);
            if (buildingInfo != null && buildingInfo.UnlocksNextTech)
            {
                CurrentTechLevel = buildingInfo.RequiredTech switch
                {
                    TechLevel.T1 => TechLevel.T2,
                    TechLevel.T2 => TechLevel.T3,
                    _ => CurrentTechLevel
                };
            }
        }
    }
    
    public List<TechBuildingInfo> GetUnlockedBuildings()
    {
        return _buildingTechMap.Values
            .Where(info => CurrentTechLevel >= info.RequiredTech)
            .ToList();
    }
    
    public List<System.Type> GetUnlockedBuildingTypes()
    {
        return GetUnlockedBuildings()
            .Select(info => info.BuildingType)
            .ToList();
    }
}