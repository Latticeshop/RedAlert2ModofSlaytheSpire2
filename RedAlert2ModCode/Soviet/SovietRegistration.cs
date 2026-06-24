using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 注册苏军角色到游戏
/// 注意：使用BaseLib的PlaceholderCharacterModel后，角色会自动注册
/// 这里只需要注册卡池、遗物池、药水池
/// </summary>

/// <summary>
/// 注册苏军卡池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class SovietCardPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<SovietCardPool>()).Distinct();
    }
}

/// <summary>
/// 注册苏军遗物池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
public static class SovietRelicPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<RelicPoolModel> __result)
    {
        __result = __result.Append(ModelDb.RelicPool<SovietRelicPool>()).Distinct();
    }
}

/// <summary>
/// 注册苏军药水池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllPotionPools), MethodType.Getter)]
public static class SovietPotionPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<PotionPoolModel> __result)
    {
        __result = __result.Append(ModelDb.PotionPool<SovietPotionPool>()).Distinct();
    }
}