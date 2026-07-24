#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class SpyCard : CardModel
{
    private const int COST = 1;
    private const int BASE_CHOICE_COUNT = 2;
    private const int UPGRADED_CHOICE_COUNT = 1;
    private const int DOLLAR_COST = 1000;
    private const int TECH_LEVEL = 3;

    private const int POWER_PLANT_DAMAGE = 5;
    private const int POWER_PLANT_DAMAGE_UPGRADED = 7;
    private const int RADAR_WEAK = 2;
    private const int RADAR_WEAK_UPGRADED = 3;
    private const int ORE_REFINERY_CREDITS = 1500;
    private const int ORE_REFINERY_CREDITS_UPGRADED = 2000;
    private const int POWER_PLANT_ENERGY_GAIN = 2;
    private const int POWER_PLANT_ENERGY_GAIN_UPGRADED = 3;
    private const int RADAR_TARGET_VULNERABLE = 2;
    private const int RADAR_TARGET_VULNERABLE_UPGRADED = 1;
    private const int RADAR_ATTACKER_AGILITY_GAIN = 2;
    private const int RADAR_ATTACKER_AGILITY_GAIN_UPGRADED = 3;

    public SpyCard() : base(COST, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/spyicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("ChoiceCount", BASE_CHOICE_COUNT),
        new IntVar("DollarNumber", DOLLAR_COST),
        new IntVar("PowerPlantDamage", POWER_PLANT_DAMAGE),
        new IntVar("PowerPlantDamageUpgraded", POWER_PLANT_DAMAGE_UPGRADED),
        new IntVar("RadarWeak", RADAR_WEAK),
        new IntVar("RadarWeakUpgraded", RADAR_WEAK_UPGRADED),
        new IntVar("OreRefineryCredits", ORE_REFINERY_CREDITS),
        new IntVar("OreRefineryCreditsUpgraded", ORE_REFINERY_CREDITS_UPGRADED),
        new IntVar("PowerPlantEnergyGain", POWER_PLANT_ENERGY_GAIN),
        new IntVar("PowerPlantEnergyGainUpgraded", POWER_PLANT_ENERGY_GAIN_UPGRADED),
        new IntVar("RadarTargetVulnerable", RADAR_TARGET_VULNERABLE),
        new IntVar("RadarTargetVulnerableUpgraded", RADAR_TARGET_VULNERABLE_UPGRADED),
        new IntVar("RadarAttackerAgilityGain", RADAR_ATTACKER_AGILITY_GAIN),
        new IntVar("RadarAttackerAgilityGainUpgraded", RADAR_ATTACKER_AGILITY_GAIN_UPGRADED)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Soldier.CreateHoverTip(),
        ModCardKeywords.Unit.CreateHoverTip()
    ];

    private int ChoiceCount => IsUpgraded ? BASE_CHOICE_COUNT + UPGRADED_CHOICE_COUNT : BASE_CHOICE_COUNT;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceConfig.PlayUnitVoice("Spy", "Camouflage");

        if (!MultiplayerSyncHelper.IsMultiplayerGame())
        {
            await ExecuteDeployMode(ctx, play);
            return;
        }

        while (true)
        {
            int? choice = await SpyChoiceScreen.ShowSelectionWithSync(Owner);

            if (choice == null)
                return;

            if (choice == 0)
            {
                List<Player> teammates = GetTeammates();
                if (teammates.Count == 0)
                {
                    await ExecuteDeployMode(ctx, play);
                    return;
                }

                Player? targetTeammate = await SpyTeammateSelectScreen.ShowSelectionWithSync(teammates, Owner);
                if (targetTeammate == null)
                    continue;

                await ExecuteAttackMode(ctx, play, targetTeammate);
                return;
            }
            else
            {
                await ExecuteDeployMode(ctx, play);
                return;
            }
        }
    }

    private async Task ExecuteAttackMode(PlayerChoiceContext ctx, CardPlay play, Player targetTeammate)
    {
        List<(Type PowerType, string Title, string Description, string IconPath)> buildingOptions =
            GetTeammateBuildingOptions(targetTeammate);

        if (buildingOptions.Count == 0)
            return;

        int? selectedOptionIndex = await SpyInfiltrateScreen.ShowSelectionWithSync(buildingOptions, Owner);
        if (selectedOptionIndex == null)
            return;

        PlayRandomVoice();

        var selectedOption = buildingOptions[(int)selectedOptionIndex];
        await SpyChoiceHelper.ExecuteAttackEffect(ctx, this, targetTeammate, selectedOption.PowerType, IsUpgraded);
    }

    private async Task ExecuteDeployMode(PlayerChoiceContext ctx, CardPlay play)
    {
        List<SpyDeployChoiceValues> choices = SpyChoiceHelper.GetRandomDeployChoices(Owner, ChoiceCount, IsUpgraded);

        List<ChoiceSelectionScreen.Choice> choiceItems = choices
            .Select((c, idx) => new ChoiceSelectionScreen.Choice
            {
                Type = (ChoiceSelectionScreen.ChoiceType)idx,
                Title = new LocString("card_keywords", c.TitleKey).GetRawText(),
                Description = ReplaceDynamicVarPlaceholders(new LocString("card_keywords", c.DescriptionKey).GetRawText()),
                Weight = 1
            })
            .ToList();

        var selectedChoice = await ChoiceSelectionScreen.ShowSelectionWithSync(choiceItems, PortraitPath, Owner, FactionType.Allied);

        if (selectedChoice == null)
            return;

        PlayRandomVoice();

        int selectedIndex = (int)selectedChoice.Type;
        if (selectedIndex >= 0 && selectedIndex < choices.Count)
        {
            await SpyChoiceHelper.ExecuteDeployEffect(ctx, this, choices[selectedIndex].DeployType, IsUpgraded);
        }
    }

    private List<Player> GetTeammates()
    {
        if (!MultiplayerSyncHelper.IsMultiplayerGame())
            return new List<Player>();

        List<Player> teammates = new();
        var combatState = Owner.Creature.CombatState;
        if (combatState != null)
        {
            foreach (var player in combatState.Players)
            {
                if (player.NetId != Owner.NetId)
                {
                    teammates.Add(player);
                }
            }
        }
        return teammates;
    }

    private List<(Type PowerType, string Title, string Description, string IconPath)> GetTeammateBuildingOptions(Player teammate)
    {
        List<(Type PowerType, string Title, string Description, string IconPath)> options = new();
        HashSet<Type> addedTypes = new();

        foreach (PowerModel power in teammate.Creature.Powers)
        {
            Type powerType = power.GetType();
            if (addedTypes.Contains(powerType))
                continue;

            if (!IsStealableBuilding(powerType))
                continue;

            string title = GetLocStringText(power.Title);
            string description = GetAttackEffectDescription(powerType);
            string iconPath = GetPowerIconPath(power);

            options.Add((powerType, title, description, iconPath));
            addedTypes.Add(powerType);
        }

        return options;
    }

    private bool IsStealableBuilding(Type powerType)
    {
        string typeName = powerType.Name;
        return typeName == "AlliedMCVPower" ||
               typeName == "SovietMCVPower" ||
               typeName == "PowerPlantPower" ||
               typeName == "SovietPowerPlantPower" ||
               typeName == "NuclearReactorCorePower" ||
               typeName == "AlliedRefineryPower" ||
               typeName == "SovietRefineryPower" ||
               typeName == "OreRefineryPower" ||
               typeName == "AlliedBarracksPower" ||
               typeName == "SovietBarracksPower" ||
               typeName == "AlliedWarFactoryPower" ||
               typeName == "SovietWarFactoryPower" ||
               typeName == "AlliedShipyardPower" ||
               typeName == "SovietShipyardPower" ||
               typeName == "BattleLabPower" ||
               typeName == "SovietBattleLabPower" ||
               typeName == "AlliedAirForceCommandPower" ||
               typeName == "SovietRadarPower" ||
               typeName == "ChronoSpherePower" ||
               typeName == "WeatherControllerPower" ||
               typeName == "IronCurtainPower" ||
               typeName == "NuclearMissileSiloPower";
    }

    private string GetPowerTitle(PowerModel power)
    {
        string typeName = power.GetType().Name;

        if (typeName.Contains("MCV", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.base_title"));
        if (typeName.Contains("Power", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.powerplant_title"));
        if (typeName.Contains("Ore", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.orerefinery_title"));
        if (typeName.Contains("BattleLab", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.battlelab_title"));
        if (typeName.Contains("Barracks", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.barracks_title"));
        if (typeName.Contains("WarFactory", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.warfactory_title"));
        if (typeName.Contains("Shipyard", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.shipyard_title"));
        if (typeName.Contains("Radar", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("AirForce", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.radar_title"));
        if (typeName.Contains("Super", StringComparison.OrdinalIgnoreCase))
            return GetLocStringText(new LocString("card_keywords", "ui.spy.attack.superweapon_title"));

        return GetLocStringText(power.Title);
    }

    private string GetPowerIconPath(PowerModel power)
    {
        try
        {
            var packedIconPathProp = power.GetType().GetProperty("PackedIconPath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (packedIconPathProp != null)
            {
                string? path = packedIconPathProp.GetValue(power) as string;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            var iconProp = power.GetType().GetProperty("Icon", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (iconProp != null)
            {
                object? icon = iconProp.GetValue(power);
                if (icon != null)
                {
                    var pathProp = icon.GetType().GetProperty("Path");
                    if (pathProp != null)
                    {
                        string? path = pathProp.GetValue(icon) as string;
                        if (!string.IsNullOrEmpty(path))
                            return path;
                    }
                }
            }
        }
        catch { }

        return "res://RedAlert2ModResources/images/packed/card_portraits/allies/spyicon.png";
    }

    private string GetBuildingCardPortrait(Type powerType, Player teammate)
    {
        string typeName = powerType.Name;
        bool isSoviet = FlagManager.GetPlayerFaction(teammate) == FlagManager.Faction.Soviet;
        string factionPath = isSoviet ? "soviet" : "allies";

        if (typeName == "AlliedMCVPower" || typeName == "SovietMCVPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/mcvicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";
        
        if (typeName == "PowerPlantPower" || typeName == "SovietPowerPlantPower" || typeName == "NuclearReactorCorePower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/npwricon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/powerplanticon.png";
        
        if (typeName == "NuclearMissileSiloPower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwepicon.png";
        
        if (typeName == "AlliedRefineryPower" || typeName == "SovietRefineryPower" || typeName == "OreRefineryPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/orerefineryicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/orerefineryicon.png";
        
        if (typeName == "AlliedBarracksPower" || typeName == "SovietBarracksPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sovietbarracksicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedbarracksicon.png";
        
        if (typeName == "AlliedWarFactoryPower" || typeName == "SovietWarFactoryPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sovietwarfactoryicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedwarfactoryicon.png";
        
        if (typeName == "AlliedShipyardPower" || typeName == "SovietShipyardPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sovietshipyardicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedshipyardicon.png";
        
        if (typeName == "BattleLabPower" || typeName == "SovietBattleLabPower")
            return isSoviet 
                ? "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sovietbattlelabicon.png" 
                : "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedbattlelabicon.png";
        
        if (typeName == "SovietRadarPower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sovietradaricon.png";
        
        if (typeName == "AlliedAirForceCommandPower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedradaricon.png";
        
        if (typeName == "ChronoSpherePower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/csphicon.png";
        
        if (typeName == "WeatherControllerPower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/wethicon.png";
        
        if (typeName == "IronCurtainPower")
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/ironicon.png";

        return "res://RedAlert2ModResources/images/packed/card_portraits/allies/spyicon.png";
    }

    private string GetAttackEffectDescription(Type powerType)
    {
        string typeName = powerType.Name;
        string upgradedTag = IsUpgraded ? "_upgraded" : "_base";
        string key = string.Empty;

        if (typeName == "AlliedMCVPower" || typeName == "SovietMCVPower")
            key = "ui.spy.attack.base_desc" + upgradedTag;
        else if (typeName == "PowerPlantPower" || typeName == "SovietPowerPlantPower" || typeName == "NuclearReactorCorePower")
            key = "ui.spy.attack.powerplant_desc" + upgradedTag;
        else if (typeName == "AlliedRefineryPower" || typeName == "SovietRefineryPower" || typeName == "OreRefineryPower")
            key = "ui.spy.attack.orerefinery_desc" + upgradedTag;
        else if (typeName == "BattleLabPower" || typeName == "SovietBattleLabPower")
            key = "ui.spy.attack.battlelab_desc" + upgradedTag;
        else if (typeName == "AlliedBarracksPower" || typeName == "SovietBarracksPower")
            key = "ui.spy.attack.barracks_desc" + upgradedTag;
        else if (typeName == "AlliedWarFactoryPower" || typeName == "SovietWarFactoryPower")
            key = "ui.spy.attack.warfactory_desc" + upgradedTag;
        else if (typeName == "AlliedShipyardPower" || typeName == "SovietShipyardPower")
            key = "ui.spy.attack.shipyard_desc" + upgradedTag;
        else if (typeName == "SovietRadarPower" || typeName == "AlliedAirForceCommandPower")
            key = "ui.spy.attack.radar_desc" + upgradedTag;
        else if (typeName == "ChronoSpherePower" || typeName == "WeatherControllerPower" || 
                 typeName == "IronCurtainPower" || typeName == "NuclearMissileSiloPower")
            key = "ui.spy.attack.superweapon_desc" + upgradedTag;
        else
            key = "ui.spy.attack.unknown_desc";

        string text = new LocString("card_keywords", key).GetRawText();
        return ReplaceDynamicVarPlaceholders(text);
    }

    private string ReplaceDynamicVarPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        int powerPlantDamage = IsUpgraded ? POWER_PLANT_DAMAGE_UPGRADED : POWER_PLANT_DAMAGE;
        int radarWeak = IsUpgraded ? RADAR_WEAK_UPGRADED : RADAR_WEAK;
        int oreRefineryCredits = IsUpgraded ? ORE_REFINERY_CREDITS_UPGRADED : ORE_REFINERY_CREDITS;
        int powerPlantEnergyGain = IsUpgraded ? POWER_PLANT_ENERGY_GAIN_UPGRADED : POWER_PLANT_ENERGY_GAIN;
        int radarTargetVulnerable = IsUpgraded ? RADAR_TARGET_VULNERABLE_UPGRADED : RADAR_TARGET_VULNERABLE;
        int radarAttackerAgilityGain = IsUpgraded ? RADAR_ATTACKER_AGILITY_GAIN_UPGRADED : RADAR_ATTACKER_AGILITY_GAIN;

        text = text.Replace("{PowerPlantDamage}", powerPlantDamage.ToString());
        text = text.Replace("{PowerPlantDamageUpgraded}", POWER_PLANT_DAMAGE_UPGRADED.ToString());
        text = text.Replace("{RadarWeak}", radarWeak.ToString());
        text = text.Replace("{RadarWeakUpgraded}", RADAR_WEAK_UPGRADED.ToString());
        text = text.Replace("{OreRefineryCredits}", oreRefineryCredits.ToString());
        text = text.Replace("{OreRefineryCreditsUpgraded}", ORE_REFINERY_CREDITS_UPGRADED.ToString());
        text = text.Replace("{PowerPlantEnergyGain}", powerPlantEnergyGain.ToString());
        text = text.Replace("{PowerPlantEnergyGainUpgraded}", POWER_PLANT_ENERGY_GAIN_UPGRADED.ToString());
        text = text.Replace("{RadarTargetVulnerable}", radarTargetVulnerable.ToString());
        text = text.Replace("{RadarTargetVulnerableUpgraded}", RADAR_TARGET_VULNERABLE_UPGRADED.ToString());
        text = text.Replace("{RadarAttackerAgilityGain}", radarAttackerAgilityGain.ToString());
        text = text.Replace("{RadarAttackerAgilityGainUpgraded}", RADAR_ATTACKER_AGILITY_GAIN_UPGRADED.ToString());

        return text;
    }

    private void PlayVoice(string voiceKey)
    {
        UnitVoiceConfig.PlayUnitVoice("Spy", voiceKey);
    }

    private void PlayRandomVoice()
    {
        UnitVoiceConfig.PlayRandomVoice("Spy");
    }

    private string GetLocStringText(object? locStringObj)
    {
        if (locStringObj == null) return string.Empty;
        if (locStringObj is string str) return str;

        System.Reflection.MethodInfo? formatMethod = locStringObj.GetType().GetMethod("GetFormattedText");
        if (formatMethod != null)
        {
            try
            {
                object? result = formatMethod.Invoke(locStringObj, null);
                if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
                {
                    return formattedText;
                }
            }
            catch { }
        }

        System.Reflection.MethodInfo? rawMethod = locStringObj.GetType().GetMethod("GetRawText");
        if (rawMethod != null)
        {
            try
            {
                object? result = rawMethod.Invoke(locStringObj, null);
                if (result is string rawText && !string.IsNullOrEmpty(rawText))
                {
                    return rawText;
                }
            }
            catch { }
        }

        string toString = locStringObj.ToString() ?? string.Empty;
        if (!toString.StartsWith("MegaCrit.Sts2.Core.Localization") && !toString.Contains("LocString"))
        {
            return toString;
        }

        return string.Empty;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChoiceCount"].UpgradeValueBy(UPGRADED_CHOICE_COUNT);
    }
}