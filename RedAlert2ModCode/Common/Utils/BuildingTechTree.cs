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
    public bool UnlocksProduction { get; set; }
    public System.Type? PowerType { get; set; }
    public List<System.Type> RequiredPowers { get; set; } = new();
    
    public TechBuildingInfo(System.Type buildingType, TechLevel requiredTech, 
        bool unlocksNextTech = false, System.Type powerType = null, 
        params System.Type[] requiredPowers)
    {
        BuildingType = buildingType;
        RequiredTech = requiredTech;
        UnlocksNextTech = unlocksNextTech;
        PowerType = powerType;
        if (requiredPowers != null)
        {
            RequiredPowers = requiredPowers.ToList();
        }
    }
    
    /// <summary>
    /// 用于标记核心生产建筑：获取该能力后解锁下一级核心生产建筑（不升级科技等级）。
    /// 例如：矿场能力解锁重工/空指部/船厂，但不解锁T2科技等级。
    /// </summary>
    public TechBuildingInfo WithProductionUnlock()
    {
        UnlocksProduction = true;
        return this;
    }
}

public class BuildingTechTree
{
    public TechLevel CurrentTechLevel { get; private set; } = TechLevel.T1;
    
    private readonly Dictionary<System.Type, TechBuildingInfo> _buildingTechMap = new();
    private readonly HashSet<System.Type> _unlockedPowerTypes = new();
    private readonly HashSet<System.Type> _productionUnlockedBuildingTypes = new();
    
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
        
        if (!IsCoreBuildingUnlocked(info))
        {
            return false;
        }
        
        foreach (var requiredPower in info.RequiredPowers)
        {
            if (!_unlockedPowerTypes.Contains(requiredPower))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool IsCoreBuildingUnlocked(TechBuildingInfo info)
    {
        // T1 核心建筑始终可用
        if (info.RequiredTech == TechLevel.T1)
        {
            return true;
        }
        
        // T2 核心建筑：生产解锁 OR 科技等级达标+能力需求满足
        if (info.RequiredTech == TechLevel.T2)
        {
            if (_productionUnlockedBuildingTypes.Contains(info.BuildingType))
            {
                return true;
            }
            
            return CurrentTechLevel >= TechLevel.T2 && HasAllRequiredPowers(info);
        }
        
        // T3 核心建筑：科技等级T3且能力需求满足
        if (info.RequiredTech == TechLevel.T3)
        {
            return CurrentTechLevel >= TechLevel.T3 && HasAllRequiredPowers(info);
        }
        
        return false;
    }
    
    private bool HasAllRequiredPowers(TechBuildingInfo info)
    {
        foreach (var requiredPower in info.RequiredPowers)
        {
            if (!_unlockedPowerTypes.Contains(requiredPower))
            {
                return false;
            }
        }
        return true;
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
            _unlockedPowerTypes.Add(powerType);
            
            var buildingInfo = _buildingTechMap.Values.FirstOrDefault(info => info.PowerType == powerType);
            if (buildingInfo == null) continue;
            
            // 处理核心生产解锁（矿场 → 重工、船厂、空指部）
            if (buildingInfo.UnlocksProduction)
            {
                // 解锁所有 T2 核心建筑的生产标记（排除需要额外能力的建筑，如作战实验室）
                foreach (var t2Building in _buildingTechMap.Values.Where(b => b.RequiredTech == TechLevel.T2 && !b.RequiredPowers.Any()))
                {
                    _productionUnlockedBuildingTypes.Add(t2Building.BuildingType);
                }
            }
            
            // 处理科技等级升级（空指部→T2, 作战实验室→T3）
            if (buildingInfo.UnlocksNextTech)
            {
                // 基于当前科技等级升级，确保每次只升一级
                CurrentTechLevel = CurrentTechLevel switch
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
            .Where(info => IsBuildingUnlocked(info.BuildingType))
            .ToList();
    }
    
    public List<System.Type> GetUnlockedBuildingTypes()
    {
        return GetUnlockedBuildings()
            .Select(info => info.BuildingType)
            .ToList();
    }
    
    /// <summary>
    /// 获取当前已解锁核心建筑的类型集合。
    /// 用于 MCV 选项面板过滤核心建筑（非牌组建筑）。
    /// </summary>
    public HashSet<System.Type> GetUnlockedCoreBuildingTypes()
    {
        var types = new HashSet<System.Type>();
        
        // T1 核心建筑始终解锁
        foreach (var b in _buildingTechMap.Values.Where(b => b.RequiredTech == TechLevel.T1))
        {
            types.Add(b.BuildingType);
        }
        
        // T2 核心建筑：生产解锁 OR 科技等级达标+能力需求满足
        foreach (var b in _buildingTechMap.Values.Where(b => b.RequiredTech == TechLevel.T2))
        {
            if (_productionUnlockedBuildingTypes.Contains(b.BuildingType))
            {
                types.Add(b.BuildingType);
            }
            else if (CurrentTechLevel >= TechLevel.T2 && HasAllRequiredPowers(b))
            {
                types.Add(b.BuildingType);
            }
        }
        
        // T3 核心建筑：科技等级T3且能力需求满足
        if (CurrentTechLevel >= TechLevel.T3)
        {
            foreach (var b in _buildingTechMap.Values.Where(b => b.RequiredTech == TechLevel.T3 && HasAllRequiredPowers(b)))
            {
                types.Add(b.BuildingType);
            }
        }
        
        return types;
    }
}
