// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.DeckConfig;

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
    /// 判断卡牌是否为箱子卡
    /// </summary>
    public static bool IsCrate(CardModel card) => card != null && CrateTypes.Contains(card.GetType());

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
    /// 应用卡池奖励模式，返回过滤/拼接后的卡牌列表：
    ///   None      → base + 公共卡（排除箱子）
    ///   AllCrates → 仅箱子卡
    ///   AddCrates → base + 公共卡（含箱子）
    /// </summary>
    public static IEnumerable<CardModel> ApplyCrateMode(
        CardPoolModel pool,
        IEnumerable<CardModel> baseCards,
        IEnumerable<CardModel> commonCards)
    {
        var commonList = commonCards.Where(c => c != null).ToList();
        var crateCards = commonList.Where(IsCrate).ToList();
        var nonCrateCommon = commonList.Where(c => !IsCrate(c)).ToList();

        return GetCrateMode(pool) switch
        {
            CratePoolMode.AllCrates => crateCards,
            CratePoolMode.AddCrates => baseCards.Concat(commonList),
            _ => baseCards.Concat(nonCrateCommon),
        };
    }

    /// <summary>
    /// 通过卡池实例反查所属角色ID（带缓存）
    /// </summary>
    private static string? FindCharacterIdForPool(CardPoolModel pool)
    {
        if (pool == null) return null;
        if (_poolToCharacterIdCache.TryGetValue(pool.GetType(), out var cached)) return cached;

        try
        {
            foreach (var character in ModelDb.AllCharacters)
            {
                try
                {
                    if (ReferenceEquals(character.CardPool, pool) || character.CardPool?.GetType() == pool.GetType())
                    {
                        string id = character.Id.Entry;
                        _poolToCharacterIdCache[pool.GetType()] = id;
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
