using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace RedAlert2ModCode.Common.Utils;

public class CustomKeyword
{
    public string Id { get; }
    public LocString Title { get; }
    public LocString Description { get; }

    public CustomKeyword(string id, LocString title, LocString description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    public string GetCardText()
    {
        return $"[gold]{Title.GetFormattedText()}.[/gold]";
    }

    public IHoverTip CreateHoverTip()
    {
        return new HoverTip(Title, Description);
    }
}

public static class CustomKeywordManager
{
    private static readonly Dictionary<string, CustomKeyword> _keywords = new();

    public static void RegisterKeyword(CustomKeyword keyword)
    {
        if (!_keywords.ContainsKey(keyword.Id))
        {
            _keywords[keyword.Id] = keyword;
        }
    }

    public static CustomKeyword? GetKeyword(string id)
    {
        _keywords.TryGetValue(id, out var keyword);
        return keyword;
    }

    public static IEnumerable<CustomKeyword> AllKeywords => _keywords.Values;
}

public static class ModCardKeywords
{
    public static readonly CustomKeyword Mcv = new(
        "MCV",
        new LocString("card_keywords", "mcv.title"),
        new LocString("card_keywords", "mcv.description")
    );

    public static readonly CustomKeyword Soldier = new(
        "SOLDIER",
        new LocString("card_keywords", "soldier.title"),
        new LocString("card_keywords", "soldier.description")
    );

    public static readonly CustomKeyword Vehicle = new(
        "VEHICLE",
        new LocString("card_keywords", "vehicle.title"),
        new LocString("card_keywords", "vehicle.description")
    );

    public static readonly CustomKeyword Aircraft = new(
        "AIRCRAFT",
        new LocString("card_keywords", "aircraft.title"),
        new LocString("card_keywords", "aircraft.description")
    );

    public static readonly CustomKeyword Navy = new(
        "NAVY",
        new LocString("card_keywords", "navy.title"),
        new LocString("card_keywords", "navy.description")
    );

    public static readonly CustomKeyword Building = new(
        "BUILDING",
        new LocString("card_keywords", "building.title"),
        new LocString("card_keywords", "building.description")
    );

    public static readonly CustomKeyword DefenseTower = new(
        "DEFENSE_TOWER",
        new LocString("card_keywords", "defense_tower.title"),
        new LocString("card_keywords", "defense_tower.description")
    );

    public static readonly CustomKeyword ProductionQueue = new(
        "PRODUCTION_QUEUE",
        new LocString("card_keywords", "production_queue.title"),
        new LocString("card_keywords", "production_queue.description")
    );

    public static readonly CustomKeyword Unit = new(
        "UNIT",
        new LocString("card_keywords", "Ra2_unit.title"),
        new LocString("card_keywords", "Ra2_unit.description")
    );

    public static readonly CustomKeyword StrategyTowerDefense = new(
        "STRATEGY_TOWER_DEFENSE",
        new LocString("card_keywords", "strategy_tower_defense.title"),
        new LocString("card_keywords", "strategy_tower_defense.description")
    );

    public static readonly CustomKeyword AlliedBattleLab = new(
        "ALLIED_BATTLE_LAB",
        new LocString("card_keywords", "allied_battle_lab.title"),
        new LocString("card_keywords", "allied_battle_lab.description")
    );

    public static readonly CustomKeyword SovietBattleLab = new(
        "SOVIET_BATTLE_LAB",
        new LocString("card_keywords", "soviet_battle_lab.title"),
        new LocString("card_keywords", "soviet_battle_lab.description")
    );

    public static readonly CustomKeyword Splash = new(
        "SPLASH",
        new LocString("card_keywords", "Ra2_splash.title"),
        new LocString("card_keywords", "Ra2_splash.description")
    );

    public static readonly CustomKeyword TargetLocked = new(
        "TARGET_LOCKED",
        new LocString("card_keywords", "target_locked.title"),
        new LocString("card_keywords", "target_locked.description")
    );

    public static readonly CustomKeyword Hornet = new(
        "HORNET",
        new LocString("card_keywords", "hornet.title"),
        new LocString("card_keywords", "hornet.description")
    );

    public static readonly CustomKeyword DesperateMeasure = new(
        "DESPERATE_MEASURE",
        new LocString("card_keywords", "desperate_measure.title"),
        new LocString("card_keywords", "desperate_measure.description")
    );

    public static readonly CustomKeyword GoldMine = new(
        "GOLD_MINE",
        new LocString("card_keywords", "gold_mine.title"),
        new LocString("card_keywords", "gold_mine.description")
    );

    public static readonly CustomKeyword GemMine = new(
        "GEM_MINE",
        new LocString("card_keywords", "gem_mine.title"),
        new LocString("card_keywords", "gem_mine.description")
    );

    public static readonly CustomKeyword GoldMineColumn = new(
        "GOLD_MINE_COLUMN",
        new LocString("card_keywords", "gold_mine_column.title"),
        new LocString("card_keywords", "gold_mine_column.description")
    );

    public static readonly CustomKeyword SuperWeapon = new(
        "SUPER_WEAPON",
        new LocString("card_keywords", "super_weapon.title"),
        new LocString("card_keywords", "super_weapon.description")
    );

    public static readonly CustomKeyword AlliedSuperWeapon = new(
        "ALLIED_SUPER_WEAPON",
        new LocString("card_keywords", "allied_super_weapon.title"),
        new LocString("card_keywords", "allied_super_weapon.description")
    );

    public static readonly CustomKeyword SovietSuperWeapon = new(
        "SOVIET_SUPER_WEAPON",
        new LocString("card_keywords", "soviet_super_weapon.title"),
        new LocString("card_keywords", "soviet_super_weapon.description")
    );

    public static readonly CustomKeyword Deploy = new(
        "DEPLOY",
        new LocString("card_keywords", "Ra2_deploy.title"),
        new LocString("card_keywords", "Ra2_deploy.description")
    );

    public static readonly CustomKeyword BuildingTechTree = new(
        "BUILDING_TECH_TREE",
        new LocString("card_keywords", "building_tech_tree.title"),
        new LocString("card_keywords", "building_tech_tree.description")
    );

    public static readonly CustomKeyword OrbitalReadiness = new(
        "ORBITAL_READINESS",
        new LocString("card_keywords", "orbital_readiness.title"),
        new LocString("card_keywords", "orbital_readiness.description")
    );

    public static readonly CustomKeyword TerrorDrone = new(
        "TERROR_DRONE",
        new LocString("card_keywords", "terror_drone.title"),
        new LocString("card_keywords", "terror_drone.description")
    );

    public static readonly CustomKeyword SteelFlood = new(
		"STEEL_FLOOD",
		new LocString("card_keywords", "steel_flood.title"),
		new LocString("card_keywords", "steel_flood.description")
	);

	public static readonly CustomKeyword Chrono = new(
		"CHRONO",
		new LocString("card_keywords", "chrono.title"),
		new LocString("card_keywords", "chrono.description")
	);

	public static readonly CustomKeyword Miner = new(
		"MINER",
		new LocString("card_keywords", "miner.title"),
		new LocString("card_keywords", "miner.description")
	);

	public static readonly CustomKeyword Garrison = new(
		"GARRISON",
		new LocString("card_keywords", "garrison.title"),
		new LocString("card_keywords", "garrison.description")
	);

	public static readonly CustomKeyword Erase = new(
		"ERASE",
		new LocString("card_keywords", "erase.title"),
		new LocString("card_keywords", "erase.description")
	);

	public static void Initialize()
    {
        CustomKeywordManager.RegisterKeyword(Mcv);
        CustomKeywordManager.RegisterKeyword(Soldier);
        CustomKeywordManager.RegisterKeyword(Vehicle);
        CustomKeywordManager.RegisterKeyword(Aircraft);
        CustomKeywordManager.RegisterKeyword(Navy);
        CustomKeywordManager.RegisterKeyword(Building);
        CustomKeywordManager.RegisterKeyword(DefenseTower);
        CustomKeywordManager.RegisterKeyword(ProductionQueue);
        CustomKeywordManager.RegisterKeyword(Unit);
        CustomKeywordManager.RegisterKeyword(StrategyTowerDefense);
        CustomKeywordManager.RegisterKeyword(AlliedBattleLab);
        CustomKeywordManager.RegisterKeyword(SovietBattleLab);
        CustomKeywordManager.RegisterKeyword(Splash);
        CustomKeywordManager.RegisterKeyword(TargetLocked);
        CustomKeywordManager.RegisterKeyword(Hornet);
        CustomKeywordManager.RegisterKeyword(DesperateMeasure);
        CustomKeywordManager.RegisterKeyword(GoldMine);
        CustomKeywordManager.RegisterKeyword(GemMine);
        CustomKeywordManager.RegisterKeyword(GoldMineColumn);
        CustomKeywordManager.RegisterKeyword(SuperWeapon);
        CustomKeywordManager.RegisterKeyword(AlliedSuperWeapon);
        CustomKeywordManager.RegisterKeyword(SovietSuperWeapon);
        CustomKeywordManager.RegisterKeyword(BuildingTechTree);
        CustomKeywordManager.RegisterKeyword(OrbitalReadiness);
        CustomKeywordManager.RegisterKeyword(TerrorDrone);
        CustomKeywordManager.RegisterKeyword(SteelFlood);
        CustomKeywordManager.RegisterKeyword(Chrono);
        CustomKeywordManager.RegisterKeyword(Miner);
        CustomKeywordManager.RegisterKeyword(Garrison);
        CustomKeywordManager.RegisterKeyword(Erase);
    }
}