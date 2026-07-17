using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet;

public static class SovietCardRegistry
{
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<Conscript>(),
        () => ModelDb.Card<SovietEngineer>(),
        () => ModelDb.Card<SovietAttackDog>(),
        () => ModelDb.Card<SovietFlakTrooper>(),
        () => ModelDb.Card<SovietTeslaTrooper>(),
        () => ModelDb.Card<Desolator>(),
    };

    public static List<Func<CardModel>> RadarSoldiers { get; } = new()
    {
        () => ModelDb.Card<SovietFlakTrooper>(),
        () => ModelDb.Card<SovietTeslaTrooper>(),
        () => ModelDb.Card<Desolator>(),
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
        () => ModelDb.Card<Kirov>(),
        () => ModelDb.Card<ApocalypseTank>(),
    };

    public static List<Func<CardModel>> RadarVehicles { get; } = new()
    {
        () => ModelDb.Card<V3Rocket>(),
        () => ModelDb.Card<DemolitionTruckCard>(),
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
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
        return cards;
    }

    public static List<Func<CardModel>> SpecialCards { get; } = CreateSpecialCards();

    private static List<Func<CardModel>> CreateSpecialCards()
    {
        return new List<Func<CardModel>>
        {
            () => ModelDb.Card<Paratrooper>(),
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
        };
    }

    public static List<CardModel> GetAllSoldiers()
    {
        return Soldiers.Select(s => s()).ToList();
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
        return units;
    }

    public static List<CardModel> GetAllBuildingCards()
    {
        return BuildingCards.Select(s => s()).ToList();
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
        return Soldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
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

        return vehicles;
    }

    public static List<CardModel> CreateHighTechVehicles(Player owner)
    {
        List<CardModel> highTechVehicles = HighTechVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
        
        List<CardModel> result = new();
        foreach (var vehicle in highTechVehicles)
        {
            if (vehicle.Id.Entry == "KIROV")
            {
                if (HasRadarPower(owner.Creature))
                {
                    result.Add(vehicle);
                }
            }
            else
            {
                result.Add(vehicle);
            }
        }
        
        return result;
    }

    public static List<CardModel> CreateRadarVehicles(Player owner)
    {
        return RadarVehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static bool HasRadarPower(Creature creature)
    {
        return creature.Powers.Any(p => p is SovietRadarPower);
    }

    public static bool HasBattleLabPower(Creature creature)
    {
        return creature.Powers.Any(p => p is SovietBattleLabPower);
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
            ships.RemoveAll(s => s.Id.Entry == "DREADNOUGHT");
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
}