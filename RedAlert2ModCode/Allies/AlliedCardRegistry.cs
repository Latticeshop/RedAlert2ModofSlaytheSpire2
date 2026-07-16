using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;

namespace RedAlert2ModCode.Allies;

public static class AlliedCardRegistry
{
    // 盟军单位卡
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<AmericanSoldier>(),
        () => ModelDb.Card<AlliesDogSoldier>(),
        () => ModelDb.Card<GuardianGi>(),
        () => ModelDb.Card<RocketSoldier>(),
        () => ModelDb.Card<AlliesEngineer>(),
    };

    public static List<Func<CardModel>> Vehicles { get; } = new()
    {
        () => ModelDb.Card<GrizzlyTank>(),
        () => ModelDb.Card<Ifv>(),
        () => ModelDb.Card<ChronoMiner>()
    };

    /// <summary>雷达解锁装甲单位 - 需要空指部/雷达解锁</summary>
    public static List<Func<CardModel>> RadarVehicles { get; } = new()
    {
        () => ModelDb.Card<TankDestroyer>()
    };

    /// <summary>高科技(T3)装甲单位 - 需要作战实验室解锁</summary>
    public static List<Func<CardModel>> HighTechVehicles { get; } = new()
    {
        () => ModelDb.Card<MirageTank>(),
        () => ModelDb.Card<PrismTank>(),
        () => ModelDb.Card<BattleFortress>()
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
        () => ModelDb.Card<Intruder>(),
        () => ModelDb.Card<NightHawkChopper>()
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        () => ModelDb.Card<Dolphin>(),
        () => ModelDb.Card<AlliedTransportShip>(),
        () => ModelDb.Card<Destroyer>(),
        () => ModelDb.Card<Agisicon>()
    };

    /// <summary>高科技(T3)海军单位 - 需要作战实验室解锁</summary>
    public static List<Func<CardModel>> HighTechShips { get; } = new()
    {
        () => ModelDb.Card<AircraftCarrier>()
    };

    // 盟军建筑卡 - 从存储类获取
    public static List<Func<CardModel>> BuildingCards { get; } = AlliesCardValues.CreateBuildingCardFactories();

    // 盟军防御塔 - 从存储类获取
    public static List<Func<CardModel>> DefenseTowers { get; } = AlliesCardValues.CreateDefenseTowerCardFactories();

    // 盟军技能卡 
    public static List<Func<CardModel>> PowerCards { get; } = CreatePowerCards();

	// 盟军特殊卡 
    public static List<Func<CardModel>> SpecialCards { get; } = CreateSpecialCards();

    private static List<Func<CardModel>> CreatePowerCards()
    {
        var cards = new List<Func<CardModel>>();
        cards.Add(() => ModelDb.Card<SellMCV>());
        cards.Add(() => ModelDb.Card<SellBuildingCard>());
        cards.Add(() => ModelDb.Card<Ra2Rally>());
        cards.Add(() => ModelDb.Card<MineRaid>());
        cards.Add(() => ModelDb.Card<StopProductionCard>());
        cards.Add(() => ModelDb.Card<OilDerrickCard>());
        cards.Add(() => ModelDb.Card<GoldMineCard>());
        cards.Add(() => ModelDb.Card<GemMineCard>());
        cards.Add(() => ModelDb.Card<GoldMineColumnCard>());
        cards.Add(() => ModelDb.Card<F2A>());
        cards.Add(() => ModelDb.Card<EagleMachineGun>());
        cards.Add(() => ModelDb.Card<EagleAirStrike>());
        cards.Add(() => ModelDb.Card<Eagle500kg>());
        cards.Add(() => ModelDb.Card<AlliedEarlyMining>());
        cards.Add(() => ModelDb.Card<ChronoWarp>());
        cards.Add(() => ModelDb.Card<LightningStorm>());
        cards.Add(() => ModelDb.Card<StrategyTowerDefense>());
        cards.Add(() => ModelDb.Card<KitingCard>());
        cards.Add(() => ModelDb.Card<OreRefineryCard>());
        cards.Add(() => ModelDb.Card<ForceField>());
        return cards;
    }

    private static List<Func<CardModel>> CreateSpecialCards()
    {
        return new List<Func<CardModel>>
        {
            () => ModelDb.Card<Paratrooper>(),
        };
    }

    /// <summary>
    /// 获取所有公共技能卡（用于动态生成）
    /// </summary>
    public static List<Func<CardModel>> GetSharedPowerCards()
    {
        return new List<Func<CardModel>>
        {
            () => ModelDb.Card<SellMCV>(),
            () => ModelDb.Card<SellBuildingCard>(),
            () => ModelDb.Card<Ra2Rally>(),
            () => ModelDb.Card<MineRaid>(),
            () => ModelDb.Card<StopProductionCard>(),
            () => ModelDb.Card<OilDerrickCard>(),
            () => ModelDb.Card<GoldMineCard>(),
            () => ModelDb.Card<GemMineCard>(),
            () => ModelDb.Card<GoldMineColumnCard>(),
            () => ModelDb.Card<F2A>(),
        };
    }

    /// <summary>
    /// 获取所有盟军专属技能卡（用于动态生成）- 飞鹰战备系列 + 其他盟军专属卡
    /// </summary>
    public static List<Func<CardModel>> GetAlliedOnlyPowerCards()
    {
        return new List<Func<CardModel>>
        {
            () => ModelDb.Card<EagleMachineGun>(),
            () => ModelDb.Card<EagleAirStrike>(),
            () => ModelDb.Card<Eagle500kg>(),
            () => ModelDb.Card<AlliedEarlyMining>(),
            () => ModelDb.Card<ChronoWarp>(),
            () => ModelDb.Card<LightningStorm>(),
            () => ModelDb.Card<StrategyTowerDefense>(),
        };
    }

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
    /// 获取所有雷达解锁装甲单位 - 需要空指部解锁
    /// </summary>
    public static List<CardModel> GetAllRadarVehicles()
    {
        return RadarVehicles.Select(s => s()).ToList();
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
    /// 获取所有高科技海军单位
    /// </summary>
    public static List<CardModel> GetAllHighTechShips()
    {
        return HighTechShips.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡
    /// </summary>
    public static List<CardModel> GetAllUnits()
    {
        List<CardModel> units = new();
        units.AddRange(GetAllSoldiers());
        units.AddRange(GetAllVehicles());
        units.AddRange(GetAllRadarVehicles());
        units.AddRange(GetAllHighTechVehicles());
        units.AddRange(GetAllAircraft());
        units.AddRange(GetAllShips());
        units.AddRange(GetAllHighTechShips());
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
        
        // 检查是否有空指部能力，如果有则添加雷达解锁单位
        if (HasAirForceCommandPower(owner.Creature))
        {
            vehicles.AddRange(CreateRadarVehicles(owner));
        }
        
        // 检查是否有作战实验室能力，如果有则添加高科技单位
        if (HasBattleLabPower(owner.Creature))
        {
            vehicles.AddRange(CreateHighTechVehicles(owner));
        }
        
        return vehicles;
    }

    /// <summary>
    /// 创建雷达解锁装甲单位卡牌列表
    /// </summary>
    public static List<CardModel> CreateRadarVehicles(Player owner)
    {
        return RadarVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    /// <summary>
    /// 创建高科技(T3)装甲单位卡牌列表
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
    /// 检查是否有空指部能力
    /// </summary>
    public static bool HasAirForceCommandPower(Creature creature)
    {
        return creature.Powers.Any(p => p is AlliedAirForceCommandPower);
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
        units.AddRange(CreateVehicles(owner));
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