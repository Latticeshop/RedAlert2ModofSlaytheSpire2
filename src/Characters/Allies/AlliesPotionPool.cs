using System;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Potions;
using System.Collections.Generic;

namespace Ra2Mod.Characters.Allies;

/// <summary>
/// 盟军药水池
/// </summary>
public class AlliesPotionPool : PotionPoolModel
{
    public override string EnergyColorName => "allies";
    
    /// <summary>
    /// 生成该角色专属的药水列表
    /// 目前返回空列表，后续可以添加盟军专属药水
    /// </summary>
    protected override PotionModel[] GenerateAllPotions()
    {
        return Array.Empty<PotionModel>();
    }
}
