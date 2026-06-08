using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace Ra2Mod.Characters.Allies;

/// <summary>
/// 注册盟军角色到游戏
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
public static class AlliesCharacterRegistrationPatch
{
    static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        // 添加盟军角色到角色列表
        __result = __result.Append(new Allies()).Distinct();
    }
}

/// <summary>
/// 注册盟军卡池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class AlliesCardPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<AlliesCardPool>()).Distinct();
    }
}

/// <summary>
/// 注册盟军遗物池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
public static class AlliesRelicPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<RelicPoolModel> __result)
    {
        __result = __result.Append(ModelDb.RelicPool<AlliesRelicPool>()).Distinct();
    }
}

/// <summary>
/// 注册盟军药水池
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllPotionPools), MethodType.Getter)]
public static class AlliesPotionPoolRegistrationPatch
{
    static void Postfix(ref IEnumerable<PotionPoolModel> __result)
    {
        __result = __result.Append(ModelDb.PotionPool<AlliesPotionPool>()).Distinct();
    }
}
