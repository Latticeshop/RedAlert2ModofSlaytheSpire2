using System;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军遗物池
/// </summary>
public class AlliesRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "allies";
    
    /// <summary>
    /// 生成该角色专属的遗物列表
    /// 目前返回空列表，后续可以添加盟军专属遗物
    /// </summary>
    protected override RelicModel[] GenerateAllRelics()
    {
        return Array.Empty<RelicModel>();
    }
}
