using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Other;

public static class OtherCardRegistry
{
    // 其他阵营单位卡（如：利赛特、古巴、伊拉克等）
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        // 待添加
    };

    public static List<Func<CardModel>> Vehicles { get; } = new()
    {
        // 待添加
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
        // 待添加
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        // 待添加
    };

    // 其他阵营建筑卡
    public static List<Func<CardModel>> BuildingCards { get; } = new()
    {
        // 待添加
    };

    // 其他阵营技能卡
    public static List<Func<CardModel>> PowerCards { get; } = new()
    {
        // 待添加
    };

    // 其他阵营特殊卡
    public static List<Func<CardModel>> SpecialCards { get; } = new()
    {
        // 待添加
    };

    /// <summary>
    /// 获取所有单位卡（士兵）
    /// </summary>
    public static List<CardModel> GetAllSoldiers()
    {
        return Soldiers.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡（装甲）
    /// </summary>
    public static List<CardModel> GetAllVehicles()
    {
        return Vehicles.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡（飞机）
    /// </summary>
    public static List<CardModel> GetAllAircraft()
    {
        return Aircraft.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡（船只）
    /// </summary>
    public static List<CardModel> GetAllShips()
    {
        return Ships.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡
    /// </summary>
    public static List<CardModel> GetAllUnits()
    {
        List<CardModel> units = new();
        units.AddRange(GetAllSoldiers());
        units.AddRange(GetAllVehicles());
        units.AddRange(GetAllAircraft());
        units.AddRange(GetAllShips());
        return units;
    }

    /// <summary>
    /// 获取所有建筑卡
    /// </summary>
    public static List<CardModel> GetAllBuildingCards()
    {
        return BuildingCards.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有技能卡
    /// </summary>
    public static List<CardModel> GetAllPowerCards()
    {
        return PowerCards.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有特殊卡
    /// </summary>
    public static List<CardModel> GetAllSpecialCards()
    {
        return SpecialCards.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有卡牌
    /// </summary>
    public static List<CardModel> GetAllCards()
    {
        List<CardModel> cards = new();
        cards.AddRange(GetAllUnits());
        cards.AddRange(GetAllBuildingCards());
        cards.AddRange(GetAllPowerCards());
        cards.AddRange(GetAllSpecialCards());
        return cards;
    }

    /// <summary>
    /// 根据拥有者创建士兵卡牌列表
    /// </summary>
    public static List<CardModel> CreateSoldiers(Player owner)
    {
        return Soldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateVehicles(Player owner)
    {
        return Vehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateAircraft(Player owner)
    {
        return Aircraft.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateShips(Player owner)
    {
        return Ships.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateAllUnits(Player owner)
    {
        List<CardModel> units = new();
        units.AddRange(CreateSoldiers(owner));
        units.AddRange(CreateVehicles(owner));
        units.AddRange(CreateAircraft(owner));
        units.AddRange(CreateShips(owner));
        return units;
    }

    public static List<CardModel> CreateBuildingCards(Player owner)
    {
        return BuildingCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreatePowerCards(Player owner)
    {
        return PowerCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateSpecialCards(Player owner)
    {
        return SpecialCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateAllCards(Player owner)
    {
        List<CardModel> cards = new();
        cards.AddRange(CreateAllUnits(owner));
        cards.AddRange(CreateBuildingCards(owner));
        cards.AddRange(CreatePowerCards(owner));
        cards.AddRange(CreateSpecialCards(owner));
        return cards;
    }
}
