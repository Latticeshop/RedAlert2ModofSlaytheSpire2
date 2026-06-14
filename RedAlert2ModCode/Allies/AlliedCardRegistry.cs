using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

public static class AlliedCardRegistry
{
    // 盟军单位卡
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<AmericanSoldier>(),
        () => ModelDb.Card<DogSoldier>(),
        () => ModelDb.Card<RocketSoldier>(),
        () => ModelDb.Card<Engineer>()
    };

    public static List<Func<CardModel>> Vehicles { get; } = new()
    {
        () => ModelDb.Card<GrizzlyTank>(),
        () => ModelDb.Card<Ifv>(),
        () => ModelDb.Card<ChronoMiner>()
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
        () => ModelDb.Card<Intruder>()
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        // 待添加
    };

    // 盟军建筑卡
    public static List<Func<CardModel>> BuildingCards { get; } = new()
    {
        () => ModelDb.Card<BarracksCard>(),
        () => ModelDb.Card<AlliedWarFactory>(),
        () => ModelDb.Card<AlliedMCV>(),
        () => ModelDb.Card<PowerPlantCard>(),
        () => ModelDb.Card<AirForceCommand>(),
        () => ModelDb.Card<PrismTowerCard>()
    };

    // 盟军技能卡
    public static List<Func<CardModel>> PowerCards { get; } = new()
    {
        () => ModelDb.Card<AlliedWallCard>()
    };

    // 盟军特殊卡
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
    /// 根据拥有者创建卡牌列表
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

    /// <summary>
    /// 创建空军单位卡牌列表（用于空指部）
    /// </summary>
    public static List<CardModel> CreateAirUnits(Player owner)
    {
        return CreateAircraft(owner);
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
