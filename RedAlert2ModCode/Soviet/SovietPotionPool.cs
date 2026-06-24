using System;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Potions;
using System.Collections.Generic;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军药水池
/// </summary>
public class SovietPotionPool : PotionPoolModel
{
    public override string EnergyColorName => "soviet";
    
    /// <summary>
    /// 生成该角色专属的药水列表
    /// 目前返回空列表，后续可以添加苏军专属药水
    /// </summary>
    protected override PotionModel[] GenerateAllPotions()
    {
        return Array.Empty<PotionModel>();
    }
}