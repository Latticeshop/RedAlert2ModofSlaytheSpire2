using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;

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

    /// <summary>高科技(T2)装甲单位 - 需要作战实验室解锁</summary>
    public static List<Func<CardModel>> HighTechVehicles { get; } = new()
    {
        () => ModelDb.Card<MirageTank>(),
        () => ModelDb.Card<PrismTank>()
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
        () => ModelDb.Card<Intruder>()
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        () => ModelDb.Card<Dolphin>(),
        () => ModelDb.Card<TransportShip>(),
        () => ModelDb.Card<Destroyer>(),
        () => ModelDb.Card<Agisicon>()
    };

    /// <summary>高科技(T2)海军单位 - 需要作战实验室解锁</summary>
    public static List<Func<CardModel>> HighTechShips { get; } = new()
    {
        () => ModelDb.Card<AircraftCarrier>()
    };

    // 盟军建筑卡
    public static List<Func<CardModel>> BuildingCards { get; } = new()
    {
        () => ModelDb.Card<BarracksCard>(),
        () => ModelDb.Card<AlliedWarFactory>(),
        () => ModelDb.Card<AlliedMCV>(),
        () => ModelDb.Card<PowerPlantCard>(),
        () => ModelDb.Card<AirForceCommand>(),
        () => ModelDb.Card<AlliedRefinery>(),
        () => ModelDb.Card<AlliedWallCard>(),
        () => ModelDb.Card<ShipyardCard>(),
        () => ModelDb.Card<BattleLab>()
    };

    // 盟军防御塔
    public static List<Func<CardModel>> DefenseTowers { get; } = new()
    {
        () => ModelDb.Card<PrismTowerCard>(),
        () => ModelDb.Card<PillboxCard>(),
        () => ModelDb.Card<PatriotMissile>()
    };

    // 盟军运转(技能)卡
    public static List<Func<CardModel>> PowerCards { get; } = new()
    {
        () => ModelDb.Card<SellMCV>(),
        () => ModelDb.Card<Ra2Rally>(),
        () => ModelDb.Card<StrategyTowerDefense>(),
        () => ModelDb.Card<OilDerrickCard>(),
        () => ModelDb.Card<StopProductionCard>(),
        () => ModelDb.Card<EagleMachineGun>(),
        () => ModelDb.Card<EagleAirStrike>(),
        () => ModelDb.Card<MassProduction>(),
        () => ModelDb.Card<GoldMineCard>(),
        () => ModelDb.Card<GemMineCard>(),
        () => ModelDb.Card<GoldMineColumnCard>(),
        () => ModelDb.Card<EarlyMining>()
    };

    // 盟军特殊卡
    public static List<Func<CardModel>> SpecialCards { get; } = new()
    {
        () => ModelDb.Card<Paratrooper>()
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
    /// 获取所有高科技(T2)装甲单位 - 需要作战实验室解锁
    /// </summary>
    public static List<CardModel> GetAllHighTechVehicles()
    {
        return HighTechVehicles.Select(s => s()).ToList();
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
    /// 获取所有防御塔卡
    /// </summary>
    public static List<CardModel> GetAllDefenseTowers()
    {
        return DefenseTowers.Select(s => s()).ToList();
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
        cards.AddRange(GetAllDefenseTowers());
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
        List<CardModel> vehicles = Vehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
        
        // 检查是否有作战实验室能力，如果有则添加高科技单位
        if (HasBattleLabPower(owner.Creature))
        {
            vehicles.AddRange(CreateHighTechVehicles(owner));
        }
        
        // 检查是否有修理厂能力，如果有则添加盟军基地车
        if (HasRepairDepotPower(owner.Creature))
        {
            vehicles.Add(owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedMCV>(), owner));
        }
        
        return vehicles;
    }

    /// <summary>
    /// 创建高科技(T2)装甲单位卡牌列表
    /// </summary>
    public static List<CardModel> CreateHighTechVehicles(Player owner)
    {
        return HighTechVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    /// <summary>
    /// 检查是否有作战实验室能力
    /// </summary>
    public static bool HasBattleLabPower(Creature creature)
    {
        return creature.Powers.Any(p => p is BattleLabPower);
    }

    /// <summary>
    /// 检查是否有修理厂能力
    /// </summary>
    public static bool HasRepairDepotPower(Creature creature)
    {
        return creature.Powers.Any(p => p is RepairDepotPower);
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
        List<CardModel> ships = Ships.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
        
        // 检查是否有作战实验室能力，如果有则添加高科技海军单位
        if (HasBattleLabPower(owner.Creature))
        {
            ships.AddRange(CreateHighTechShips(owner));
        }
        
        return ships;
    }

    /// <summary>
    /// 创建高科技(T2)海军单位卡牌列表
    /// </summary>
    public static List<CardModel> CreateHighTechShips(Player owner)
    {
        return HighTechShips.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateAllUnits(Player owner)
    {
        List<CardModel> units = new();
        units.AddRange(CreateSoldiers(owner));
        units.AddRange(CreateVehicles(owner));  // CreateVehicles已包含高科技单位筛选逻辑
        units.AddRange(CreateAircraft(owner));
        units.AddRange(CreateShips(owner));
        return units;
    }

    public static List<CardModel> CreateBuildingCards(Player owner)
    {
        return BuildingCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateDefenseTowers(Player owner)
    {
        return DefenseTowers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
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
        cards.AddRange(CreateDefenseTowers(owner));
        cards.AddRange(CreatePowerCards(owner));
        cards.AddRange(CreateSpecialCards(owner));
        return cards;
    }
}
