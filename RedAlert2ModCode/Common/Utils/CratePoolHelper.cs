// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.DeckConfig;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 卡池奖励箱子模式工具类 - 控制箱子卡是否/如何进入角色的卡牌奖励范围
/// 卡池 AllCards 即游戏卡牌奖励范围（如盟军 AlliesCardPool）。
/// </summary>
public static class CratePoolHelper
{
    /// <summary>
    /// 所有箱子卡类型
    /// </summary>
    private static readonly HashSet<Type> CrateTypes = new()
    {
        typeof(ArmorCrate), typeof(UpgradeCrate), typeof(ExplosionCrate),
        typeof(SuperWeaponCrate), typeof(OreCrate), typeof(StealthCrate),
        typeof(SpeedCrate), typeof(FirepowerCrate), typeof(HealCrate),
        typeof(VehicleCrate), typeof(SoldierCrate), typeof(AirForceCrate),
        typeof(NavyCrate), typeof(MoneyCrate), typeof(RandomCrate),
    };

    /// <summary>
    /// 卡池类型 -> 角色ID 缓存（避免每次 AllCards 访问都遍历角色）
    /// </summary>
    private static readonly Dictionary<Type, string> _poolToCharacterIdCache = new();

    /// <summary>
    /// 注册了箱子卡的卡池类型（当前：盟军/苏军共用公共卡列表）。
    /// 新增注册箱子卡的卡池时，在这里补充对应类型。
    /// </summary>
    private static readonly Type[] CratePoolTypes =
    {
        typeof(AlliesCardPool),
        typeof(SovietCardPool),
    };

    /// <summary>
    /// 判断卡牌是否为箱子卡
    /// </summary>
    public static bool IsCrate(CardModel card) => card != null && CrateTypes.Contains(card.GetType());

    /// <summary>
    /// 获取所有箱子卡牌模型（不受卡池奖励模式过滤影响，用于卡牌库等完整展示）。
    /// </summary>
    public static IEnumerable<CardModel> GetAllCrateCards()
    {
        var cards = new List<CardModel>();
        foreach (var type in CrateTypes)
        {
            try
            {
                var method = typeof(ModelDb).GetMethod("Card", Type.EmptyTypes)?.MakeGenericMethod(type);
                if (method?.Invoke(null, null) is CardModel card)
                {
                    cards.Add(card);
                }
            }
            catch { }
        }
        return cards;
    }

    /// <summary>
    /// 获取注册了箱子卡的角色ID集合。
    /// </summary>
    public static IEnumerable<string> GetCrateOwnerCharacterIds()
    {
        foreach (var poolType in CratePoolTypes)
        {
            string? id = FindCharacterIdForPoolType(poolType);
            if (!string.IsNullOrEmpty(id)) yield return id;
        }
    }

    /// <summary>
    /// 根据卡池查找其所属角色的卡池奖励模式
    /// </summary>
    public static CratePoolMode GetCrateMode(CardPoolModel pool)
    {
        string? characterId = FindCharacterIdForPool(pool);
        if (string.IsNullOrEmpty(characterId)) return CratePoolMode.None;
        try { return ModConfigManager.GetCharacterConfig(characterId).CratePoolMode; }
        catch { return CratePoolMode.None; }
    }

    /// <summary>
    /// 应用卡池奖励模式，返回卡牌列表：
    ///   箱子卡属于角色默认卡池，始终包含在卡池中（商店、事件、默认奖励都走完整池）；
    ///   "奖励只有箱子"（AllCrates）不再修改池本身，而是由补丁在
    ///   战斗结束卡牌奖励生成时（CardCreationOptions.ForRoom）用过滤器收窄。
    /// </summary>
    public static IEnumerable<CardModel> ApplyCrateMode(
        CardPoolModel pool,
        IEnumerable<CardModel> baseCards,
        IEnumerable<CardModel> commonCards)
    {
        var commonList = commonCards.Where(c => c != null).ToList();
        return baseCards.Concat(commonList);
    }

    /// <summary>
    /// 通过卡池实例反查所属角色ID（带缓存）
    /// </summary>
    private static string? FindCharacterIdForPool(CardPoolModel pool)
    {
        if (pool == null) return null;
        return FindCharacterIdForPoolType(pool.GetType());
    }

    private static string? FindCharacterIdForPoolType(Type poolType)
    {
        if (poolType == null) return null;
        if (_poolToCharacterIdCache.TryGetValue(poolType, out var cached)) return cached;

        try
        {
            foreach (var character in ModelDb.AllCharacters)
            {
                try
                {
                    if (character.CardPool?.GetType() == poolType)
                    {
                        string id = character.Id.Entry;
                        _poolToCharacterIdCache[poolType] = id;
                        return id;
                    }
                }
                catch { }
            }
        }
        catch { }

        return null;
    }
}
