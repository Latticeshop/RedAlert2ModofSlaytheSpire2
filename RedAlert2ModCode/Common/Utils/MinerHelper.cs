using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 矿场检测工具 - 统一判断单位是否拥有任意矿场能力
/// 盟军/苏联矿车统一使用此方法，确保拥有任意矿场（盟军/苏联矿场）时都能加钱
/// </summary>
public static class MinerHelper
{
    /// <summary>
    /// 是否拥有任意矿场能力（盟军矿场、苏联矿场）
    /// </summary>
    public static bool HasAnyRefinery(Creature creature)
    {
        return creature.Powers.Any(p => p is AlliedRefineryPower
                                     || p is SovietRefineryPower);
    }
}
