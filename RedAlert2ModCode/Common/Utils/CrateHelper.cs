using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 箱子卡牌工具类 - 提供箱子相关的随机选择和辅助功能
/// </summary>
public static class CrateHelper
{
    private static readonly List<Func<CardModel>> _allCrateFactories = new()
    {
        () => ModelDb.Card<MoneyCrate>(),
        () => ModelDb.Card<VehicleCrate>(),
        () => ModelDb.Card<SoldierCrate>(),
        () => ModelDb.Card<AirForceCrate>(),
        () => ModelDb.Card<NavyCrate>(),
        () => ModelDb.Card<HealCrate>(),
        () => ModelDb.Card<FirepowerCrate>(),
        () => ModelDb.Card<SpeedCrate>(),
        () => ModelDb.Card<ArmorCrate>(),
        () => ModelDb.Card<UpgradeCrate>(),
        () => ModelDb.Card<StealthCrate>(),
        () => ModelDb.Card<ExplosionCrate>(),
        () => ModelDb.Card<SuperWeaponCrate>(),
        () => ModelDb.Card<OreCrate>(),
    };

    private static readonly Dictionary<Type, int> _crateWeights = new()
    {
        { typeof(MoneyCrate), 50 },
        { typeof(FirepowerCrate), 30 },
        { typeof(SpeedCrate), 30 },
        { typeof(ArmorCrate), 30 },
        { typeof(UpgradeCrate), 30},
        { typeof(VehicleCrate), 30 },
        { typeof(HealCrate), 20 },
        { typeof(SoldierCrate), 10 },
        { typeof(AirForceCrate), 10 },
        { typeof(NavyCrate), 10 },
        { typeof(StealthCrate), 30 },
        { typeof(ExplosionCrate), 20 },
        { typeof(OreCrate), 20 },
        { typeof(SuperWeaponCrate), 1 },
    };

    /// <summary>
    /// 从普通箱子池中随机选择一张卡牌（不包括RandomCrate自身）
    /// </summary>
    public static CardModel GetRandomCrateCard(Player owner, FlagManager.Faction faction, bool excludeRandom = true)
    {
        var candidates = _allCrateFactories.Select(f => f()).ToList();

        while (true)
        {
            int totalWeight = candidates.Sum(c => _crateWeights.GetValueOrDefault(c.GetType(), 1));
            // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
            int roll = owner.RunState.Rng.CombatCardSelection.NextInt(totalWeight);

            int accumulated = 0;
            foreach (var card in candidates)
            {
                int weight = _crateWeights.GetValueOrDefault(card.GetType(), 1);
                accumulated += weight;
                if (roll < accumulated)
                {
                    return card;
                }
            }
        }
    }

    /// <summary>
    /// 从随机箱子池中获得一张升级的箱子卡牌
    /// </summary>
    public static CardModel GetRandomUpgradedCrateCard(Player owner, FlagManager.Faction faction)
    {
        return GetRandomCrateCard(owner, faction);
    }

    /// <summary>
    /// 从Token箱子池中随机选择一张（隐身、爆炸、超武、矿石）
    /// </summary>
    public static CardModel GetRandomTokenCrateCard(Player owner)
    {
        var tokenTypes = new List<Type>
        {
            typeof(StealthCrate),
            typeof(ExplosionCrate),
            typeof(SuperWeaponCrate),
            typeof(OreCrate),
        };

        int totalWeight = tokenTypes.Sum(t => _crateWeights.GetValueOrDefault(t, 1));
        // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
        int roll = owner.RunState.Rng.CombatCardSelection.NextInt(totalWeight);

        int accumulated = 0;
        foreach (var type in tokenTypes)
        {
            int weight = _crateWeights.GetValueOrDefault(type, 1);
            accumulated += weight;
            if (roll < accumulated)
            {
                var method = typeof(ModelDb).GetMethod("Card", Type.EmptyTypes)
                    ?.MakeGenericMethod(type);
                return (CardModel)method?.Invoke(null, null);
            }
        }

        return ModelDb.Card<StealthCrate>();
    }
}
