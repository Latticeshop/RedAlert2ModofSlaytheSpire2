using System;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军遗物池
/// </summary>
public class SovietRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "soviet";
    
    /// <summary>
    /// 生成该角色专属的遗物列表
    /// </summary>
    protected override RelicModel[] GenerateAllRelics()
    {
        return new RelicModel[]
        {
            ModelDb.Relic<RedAlert2ModCode.Common.Relics.DollarRelic>()
        };
    }
}