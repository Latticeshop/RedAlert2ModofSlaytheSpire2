using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Soviet.Relics;

namespace RedAlert2ModCode.Soviet;

public static class SovietCardRegistry
{
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<Conscript>(),
        () => ModelDb.Card<SovietEngineer>(),
        () => ModelDb.Card<SovietAttackDog>(),
        () => ModelDb.Card<SovietFlakTrooper>(), // 防空步兵 - T1基础单位
    };

    public static List<Func<CardModel>> RadarSoldiers { get; } = new()
    {
        () => ModelDb.Card<SovietTeslaTrooper>(),
        () => ModelDb.Card<Desolator>(),
        () => ModelDb.Card<TerrorMan>(),
        () => ModelDb.Card<CrazyIvanCard>(),
    };

    public static List<Func<CardModel>> RelicUnlockedSoldiers { get; } = new()
    {
        () => ModelDb.Card<ChronoIvanCard>(),
        () => ModelDb.Card<PsiCommandoCard>(),
    };

    public static List<Func<CardModel>> Vehicles { get; } = new()
    {
        () => ModelDb.Card<RhinoTank>(),
        () => ModelDb.Card<WarMiner>(),
        () => ModelDb.Card<FlakTrack>(),
        () => ModelDb.Card<TerrorDrone>(),
    };

    public static List<Func<CardModel>> HighTechVehicles { get; } = new()
    {
        () => ModelDb.Card<ApocalypseTank>(),
    };

    public static List<Func<CardModel>> RadarVehicles { get; } = new()
    {
        () => ModelDb.Card<V3Rocket>(),
        () => ModelDb.Card<DemolitionTruckCard>(),
        () => ModelDb.Card<TeslaTank>(),
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
        () => ModelDb.Card<SpyPlane>(),
        () => ModelDb.Card<Kirov>(),
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        () => ModelDb.Card<SovietTransportShip>(),
        () => ModelDb.Card<FlakSubmarine>(),
        () => ModelDb.Card<TyphoonSubmarine>(),
        () => ModelDb.Card<Dreadnought>(),
        () => ModelDb.Card<GiantSquid>(),
    };

    public static List<Func<CardModel>> BuildingCards { get; } = SovietCardValues.CreateBuildingCardFactories();

    public static List<Func<CardModel>> DefenseTowers { get; } = SovietCardValues.CreateDefenseTowerCardFactories();

    public static List<Func<CardModel>> PowerCards { get; } = CreatePowerCards();

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
        cards.Add(() => ModelDb.Card<SovietEarlyMining>());
        cards.Add(() => ModelDb.Card<IronCurtain>());
        cards.Add(() => ModelDb.Card<NuclearAttack>());
        cards.Add(() => ModelDb.Card<KitingCard>());
        cards.Add(() => ModelDb.Card<IndustrialPlantCard>());
        cards.Add(() => ModelDb.Card<MassProductionCard>());
        cards.Add(() => ModelDb.Card<ForceField>());
        cards.Add(() => ModelDb.Card<NuclearPlantCard>());
        cards.Add(() => ModelDb.Card<SupportCard>());
        cards.Add(() => ModelDb.Card<OrbitalGasStrike>());
        cards.Add(() => ModelDb.Card<Orbital120mm>());
        cards.Add(() => ModelDb.Card<Orbital380mm>());
        return cards;
    }

    public static List<Func<CardModel>> SpecialCards { get; } = CreateSpecialCards();

    /// <summary>
    /// 特殊单位卡（属于单位卡的特殊卡，不含 Paratrooper 伞兵——伞兵不属于单位卡）。
    /// </summary>
    public static List<Func<CardModel>> SpecialUnits { get; } = new()
    {
        () => ModelDb.Card<YuriCard>(),
        () => ModelDb.Card<YuriPrimeCard>(),
    };

    /// <summary>
    /// MCV 卡（既是装甲单位也是建筑，需要同时注册到单位列表和建筑列表）。
    /// </summary>
    public static List<Func<CardModel>> MobileConstructionVehicles { get; } = new()
    {
        () => ModelDb.Card<SovietMCV>(),
    };

    private static List<Func<CardModel>> CreateSpecialCards()
    {
        return new List<Func<CardModel>>
        {
            () => ModelDb.Card<Paratrooper>(),
            () => ModelDb.Card<YuriCard>(),
            () => ModelDb.Card<YuriPrimeCard>(),
        };
    }

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
            () => ModelDb.Card<SupportCard>(),
        };
    }

    public static List<CardModel> GetAllSoldiers()
    {
        List<CardModel> soldiers = Soldiers.Select(s => s()).ToList();
        soldiers.AddRange(GetAllRadarSoldiers());
        soldiers.AddRange(GetAllRelicUnlockedSoldiers());
        return soldiers;
    }

    public static List<CardModel> GetAllRadarSoldiers()
    {
        return RadarSoldiers.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllRelicUnlockedSoldiers()
    {
        return RelicUnlockedSoldiers.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllVehicles()
    {
        List<CardModel> vehicles = Vehicles.Select(s => s()).ToList();
        vehicles.AddRange(HighTechVehicles.Select(s => s()).ToList());
        vehicles.AddRange(RadarVehicles.Select(s => s()).ToList());
        return vehicles;
    }

    public static List<CardModel> GetAllAircraft()
    {
        return Aircraft.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllShips()
    {
        return Ships.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllUnits()
    {
        List<CardModel> units = new();
        units.AddRange(GetAllSoldiers());
        units.AddRange(GetAllVehicles());
        units.AddRange(GetAllAircraft());
        units.AddRange(GetAllShips());
        units.AddRange(SpecialUnits.Select(s => s()));
        units.AddRange(MobileConstructionVehicles.Select(s => s()));
        return units;
    }

    /// <summary>
    /// 获取所有单位卡类型（含特殊单位卡和 MCV，自动去重）。
    /// 供新叶/树叶膏药等需要判断"是否为单位卡"的逻辑使用。
    /// </summary>
    public static HashSet<Type> GetAllUnitTypes()
    {
        var types = new HashSet<Type>();
        foreach (var factory in Soldiers)
            types.Add(factory().GetType());
        foreach (var factory in RadarSoldiers)
            types.Add(factory().GetType());
        foreach (var factory in RelicUnlockedSoldiers)
            types.Add(factory().GetType());
        foreach (var factory in Vehicles)
            types.Add(factory().GetType());
        foreach (var factory in HighTechVehicles)
            types.Add(factory().GetType());
        foreach (var factory in RadarVehicles)
            types.Add(factory().GetType());
        foreach (var factory in Aircraft)
            types.Add(factory().GetType());
        foreach (var factory in Ships)
            types.Add(factory().GetType());
        foreach (var factory in SpecialUnits)
            types.Add(factory().GetType());
        foreach (var factory in MobileConstructionVehicles)
            types.Add(factory().GetType());
        return types;
    }

    /// <summary>
    /// 获取所有基础单位类型（T1/T2）- 从现有工厂列表动态推导
    /// </summary>
    public static List<Type> GetBasicUnitTypes()
    {
        List<Type> types = new();
        types.AddRange(Soldiers.Select(f => f().GetType()));
        types.AddRange(RadarSoldiers.Select(f => f().GetType()));
        types.AddRange(Vehicles.Select(f => f().GetType()));
        types.AddRange(RadarVehicles.Select(f => f().GetType()));
        types.AddRange(Aircraft.Select(f => f().GetType()));
        types.AddRange(Ships.Select(f => f().GetType()).Where(t => t != typeof(Dreadnought)));
        return types;
    }

    /// <summary>
    /// 获取所有T3单位类型（高科技单位）- 从现有工厂列表动态推导
    /// </summary>
    public static List<Type> GetT3UnitTypes()
    {
        List<Type> types = new();
        types.AddRange(HighTechVehicles.Select(f => f().GetType()));
        types.AddRange(RelicUnlockedSoldiers.Select(f => f().GetType()));
        types.AddRange(Ships.Select(f => f().GetType()).Where(t => t == typeof(Dreadnought)));
        return types;
    }

    public static List<CardModel> GetAllBuildingCards()
    {
        return BuildingCards.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllDefenseTowers()
    {
        return DefenseTowers.Select(s => s()).ToList();
    }

    public static List<System.Type> GetAllBuildingCardTypes()
    {
        return BuildingCards.Select(f => f().GetType()).ToList();
    }

    public static List<System.Type> GetAllDefenseTowerTypes()
    {
        return DefenseTowers.Select(f => f().GetType()).ToList();
    }

    public static List<CardModel> GetAllPowerCards()
    {
        return PowerCards.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllSpecialCards()
    {
        return SpecialCards.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllCards()
    {
        List<CardModel> cards = new();
        cards.AddRange(GetAllUnits());
        cards.AddRange(GetAllBuildingCards());
        cards.AddRange(GetAllPowerCards());
        cards.AddRange(GetAllSpecialCards());
        return cards;
    }

    public static List<CardModel> CreateSoldiers(Player owner)
    {
        List<CardModel> soldiers = Soldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
        
        if (HasRadarPower(owner.Creature))
        {
            soldiers.AddRange(RadarSoldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList());
        }
        
        if (HasChronoIvanRelic(owner))
        {
            soldiers.Add(owner.Creature.CombatState.CreateCard(ModelDb.Card<ChronoIvanCard>(), owner));
        }

        if (HasPsiCommandoRelic(owner))
        {
            soldiers.Add(owner.Creature.CombatState.CreateCard(ModelDb.Card<PsiCommandoCard>(), owner));
        }
        
        return soldiers;
    }

    public static List<CardModel> CreateVehicles(Player owner)
    {
        List<CardModel> vehicles = Vehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();

        if (HasBattleLabPower(owner.Creature))
        {
            vehicles.AddRange(CreateHighTechVehicles(owner));
        }

        if (HasRadarPower(owner.Creature))
        {
            vehicles.AddRange(CreateRadarVehicles(owner));
        }

        // 基洛夫（T3空军）：苏联空军由雷达解锁，基洛夫需要作战实验室（T3）后才能在重工生产
        if (HasRadarPower(owner.Creature) && HasBattleLabPower(owner.Creature))
        {
            vehicles.Add(owner.Creature.CombatState.CreateCard(ModelDb.Card<Kirov>(), owner));
            GD.Print("[SovietCardRegistry] 检测到雷达+作战实验室，重工加入基洛夫选项");
        }

        return vehicles;
    }

    public static List<CardModel> CreateHighTechVehicles(Player owner)
    {
        return HighTechVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateRadarVehicles(Player owner)
    {
        return RadarVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    /// <summary>
    /// 检查是否有雷达/空指部能力（T2科技）或作战实验室能力（T3科技）
    /// 作战实验室(T3)也能解锁T2单位
    /// </summary>
    public static bool HasRadarPower(Creature creature)
    {
        return creature.Powers.Any(p => p.GetType().Name == typeof(SovietRadarPower).Name) ||
               creature.Powers.Any(p => p.GetType().Name == typeof(RedAlert2ModCode.Allies.Powers.AlliedAirForceCommandPower).Name) ||
               creature.Powers.Any(p => p.GetType().Name == typeof(SovietBattleLabPower).Name);
    }

    public static bool HasBattleLabPower(Creature creature)
    {
        return creature.Powers.Any(p => p.GetType().Name == typeof(SovietBattleLabPower).Name);
    }

    public static bool HasChronoIvanRelic(Player owner)
    {
        return owner.Relics.Any(r => r is ChronoIvanRelic);
    }

    public static bool HasPsiCommandoRelic(Player owner)
    {
        return owner.Relics.Any(r => r is RedAlert2ModCode.Allies.Relics.PsiCommandoRelic);
    }

    public static List<CardModel> CreateRelicUnlockedSoldiers(Player owner)
    {
        return RelicUnlockedSoldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateAircraft(Player owner)
    {
        return Aircraft.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateShips(Player owner)
    {
        List<CardModel> ships = Ships.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
        
        if (!HasBattleLabPower(owner.Creature))
        {
            // 使用 Contains 来匹配完整的卡牌ID（如 RED_ALERT2_MOD_CARD_DREADNOUGHT）
            ships.RemoveAll(s => s.Id.Entry.Contains("DREADNOUGHT"));
        }
        
        return ships;
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

    #region T1/T2/T3 单位获取

    /// <summary>
    /// 获取所有T1单位（基础单位，开局即可生产）
    /// </summary>
    public static List<CardModel> GetT1Units()
    {
        List<CardModel> units = new();
        units.AddRange(Soldiers.Select(s => s()));
        units.AddRange(Vehicles.Select(s => s()));
        units.AddRange(Aircraft.Select(s => s()));
        units.AddRange(Ships.Select(s => s()).Where(c => !c.Id.Entry.Contains("DREADNOUGHT")));
        return units;
    }

    /// <summary>
    /// 获取所有T2单位（需要雷达塔解锁）
    /// </summary>
    public static List<CardModel> GetT2Units()
    {
        List<CardModel> units = new();
        units.AddRange(RadarSoldiers.Select(s => s()));
        units.AddRange(RadarVehicles.Select(s => s()));
        return units;
    }

    /// <summary>
    /// 获取所有T3单位（需要作战实验室解锁）
    /// </summary>
    public static List<CardModel> GetT3Units()
    {
        List<CardModel> units = new();
        units.AddRange(HighTechVehicles.Select(s => s()));
        units.AddRange(RelicUnlockedSoldiers.Select(s => s()));
        units.AddRange(Ships.Select(s => s()).Where(c => c.Id.Entry.Contains("DREADNOUGHT")));
        return units;
    }

    /// <summary>
    /// 创建T1单位卡牌列表
    /// </summary>
    public static List<CardModel> CreateT1Units(Player owner)
    {
        List<CardModel> units = new();
        units.AddRange(Soldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(Vehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(Aircraft.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(Ships.Select(s => owner.Creature.CombatState.CreateCard(s(), owner))
            .Where(c => !c.Id.Entry.Contains("DREADNOUGHT")));
        return units;
    }

    /// <summary>
    /// 创建T2单位卡牌列表（需要雷达塔解锁）
    /// </summary>
    public static List<CardModel> CreateT2Units(Player owner)
    {
        List<CardModel> units = new();
        units.AddRange(RadarSoldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(RadarVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        return units;
    }

    /// <summary>
    /// 创建T3单位卡牌列表（需要作战实验室解锁）
    /// </summary>
    public static List<CardModel> CreateT3Units(Player owner)
    {
        List<CardModel> units = new();
        units.AddRange(HighTechVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(RelicUnlockedSoldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)));
        units.AddRange(Ships.Select(s => owner.Creature.CombatState.CreateCard(s(), owner))
            .Where(c => c.Id.Entry.Contains("DREADNOUGHT")));
        return units;
    }

    /// <summary>
    /// 获取T1单位类型列表
    /// </summary>
    public static List<Type> GetT1UnitTypes()
    {
        List<Type> types = new();
        types.AddRange(Soldiers.Select(f => f().GetType()));
        types.AddRange(Vehicles.Select(f => f().GetType()));
        types.AddRange(Aircraft.Select(f => f().GetType()));
        types.AddRange(Ships.Select(f => f().GetType()).Where(t => t != typeof(Dreadnought)));
        return types;
    }

    /// <summary>
    /// 获取T2单位类型列表
    /// </summary>
    public static List<Type> GetT2UnitTypes()
    {
        List<Type> types = new();
        types.AddRange(RadarSoldiers.Select(f => f().GetType()));
        types.AddRange(RadarVehicles.Select(f => f().GetType()));
        return types;
    }

    #endregion
}
