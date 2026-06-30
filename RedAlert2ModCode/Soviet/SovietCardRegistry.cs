using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet;

public static class SovietCardRegistry
{
    // 苏军单位卡
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<Conscript>(),
        () => ModelDb.Card<SovietEngineer>(),
        () => ModelDb.Card<SovietAttackDog>(),
        () => ModelDb.Card<SovietFlakTrooper>(),
        () => ModelDb.Card<SovietTeslaTrooper>(),
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
    };

    public static List<Func<CardModel>> RadarVehicles { get; } = new()
    {
        () => ModelDb.Card<V3Rocket>(),
    };

    public static List<Func<CardModel>> Aircraft { get; } = new()
    {
    };

    public static List<Func<CardModel>> Ships { get; } = new()
    {
        () => ModelDb.Card<SovietTransportShip>(),
        () => ModelDb.Card<FlakSubmarine>(),
    };

    // 苏军建筑卡
	public static List<Func<CardModel>> BuildingCards { get; } = new()
	{
		() => ModelDb.Card<SovietBarracksCard>(),
		() => ModelDb.Card<SovietWarFactory>(),
		() => ModelDb.Card<SovietShipyardCard>(),
		() => ModelDb.Card<SovietRepairDepot>(),
		() => ModelDb.Card<SovietPillboxCard>(),
		() => ModelDb.Card<SovietFlakCannon>(),
		() => ModelDb.Card<SovietWallCard>(),
		() => ModelDb.Card<SovietFortifiedWall>(),
		() => ModelDb.Card<NuclearReactor>(),
		() => ModelDb.Card<SovietRefinery>(),
		() => ModelDb.Card<SovietMCV>(),
		() => ModelDb.Card<SovietBattleLab>(),
		() => ModelDb.Card<SovietRadar>(),
		() => ModelDb.Card<SovietTeslaCoilCard>(),
		() => ModelDb.Card<IronCurtainCard>(),
		() => ModelDb.Card<NuclearMissileSiloCard>(),
	};

    // 苏军技能卡 - 通过CommonCardRegistry获取公共共享卡（不含飞鹰战备系列）
	public static List<Func<CardModel>> PowerCards { get; } = CreatePowerCards();

	private static List<Func<CardModel>> CreatePowerCards()
	{
		var cards = CommonCardRegistry.GetAllPowerCardsForSoviet();
		cards.Add(() => ModelDb.Card<IronCurtain>());
		cards.Add(() => ModelDb.Card<NuclearAttack>());
		return cards;
	}

	// 苏军特殊卡 - 通过CommonCardRegistry获取公共卡
	public static List<Func<CardModel>> SpecialCards { get; } = CommonCardRegistry.GetAllSpecialCardsForBoth();

    /// <summary>
    /// 获取所有公共技能卡（用于动态生成）
    /// </summary>
    public static List<Func<CardModel>> GetSharedPowerCards()
    {
        return CommonCardRegistry.SharedPowerCards;
    }

    /// <summary>
    /// 获取所有单位卡（士兵）
    /// </summary>
    public static List<CardModel> GetAllSoldiers()
    {
        return Soldiers.Select(s => s()).ToList();
    }

    /// <summary>
    /// 获取所有单位卡（装甲）- 包含高科技单位和雷达单位
    /// </summary>
    public static List<CardModel> GetAllVehicles()
    {
        List<CardModel> vehicles = Vehicles.Select(s => s()).ToList();
        vehicles.AddRange(HighTechVehicles.Select(s => s()).ToList());
        vehicles.AddRange(RadarVehicles.Select(s => s()).ToList());
        return vehicles;
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
        List<CardModel> vehicles = Vehicles.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();

        if (HasBattleLabPower(owner.Creature))
        {
            vehicles.AddRange(CreateHighTechVehicles(owner));
        }

        if (HasRadarPower(owner.Creature))
        {
            vehicles.AddRange(CreateRadarVehicles(owner));
        }

        if (HasRepairDepotPower(owner.Creature))
        {
            vehicles.Add(owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietMCV>(), owner));
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

    public static bool HasRadarPower(Creature creature)
    {
        return creature.Powers.Any(p => p is SovietRadarPower);
    }

    public static bool HasBattleLabPower(Creature creature)
    {
        return creature.Powers.Any(p => p is SovietBattleLabPower);
    }

    public static bool HasRepairDepotPower(Creature creature)
    {
        return creature.Powers.Any(p => p is SovietRepairDepotPower);
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
