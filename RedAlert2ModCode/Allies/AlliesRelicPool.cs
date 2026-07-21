using System;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Relics;
using RedAlert2ModCode.Allies.Relics;
using RedAlert2ModCode.Soviet.Relics;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军遗物池
/// </summary>
public class AlliesRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "allies";
    
    /// <summary>
    /// 生成该角色专属的遗物列表
    /// </summary>
    protected override RelicModel[] GenerateAllRelics()
    {
        return new RelicModel[]
        {
            ModelDb.Relic<Common.Relics.DollarRelic>(),
            ModelDb.Relic<Common.Relics.DollarAncientRelic>(),
            ModelDb.Relic<USARelic>(),
            ModelDb.Relic<UKRelic>(),
            ModelDb.Relic<FranceRelic>(),
            ModelDb.Relic<GermanyRelic>(),
            ModelDb.Relic<SouthKoreaRelic>(),
        };
    }
}
