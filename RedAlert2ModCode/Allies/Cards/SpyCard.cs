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

namespace RedAlert2ModCode.Allies.Cards;

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
    private const int RADAR_TARGET_AGILITY_LOSS = 2;
    private const int RADAR_TARGET_AGILITY_LOSS_UPGRADED = 1;
    private const int RADAR_ATTACKER_AGILITY_GAIN = 4;
    private const int RADAR_ATTACKER_AGILITY_GAIN_UPGRADED = 5;

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
        new IntVar("RadarTargetAgilityLoss", RADAR_TARGET_AGILITY_LOSS),
        new IntVar("RadarTargetAgilityLossUpgraded", RADAR_TARGET_AGILITY_LOSS_UPGRADED),
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

        if (MultiplayerSyncHelper.IsMultiplayerGame())
        {
            int? choice = await SpyChoiceScreen.ShowSelectionWithSync(Owner);

            if (choice == null)
                return;

            if (choice == 0)
            {
                await ExecuteAttackMode(ctx, play);
            }
            else
            {
                await ExecuteDeployMode(ctx, play);
            }
        }
        else
        {
            await ExecuteDeployMode(ctx, play);
        }
    }

    private async Task ExecuteAttackMode(PlayerChoiceContext ctx, CardPlay play)
    {
        List<Player> teammates = GetTeammates();
        if (teammates.Count == 0)
            return;

        List<(Type PowerType, string Title, string Description, string IconPath)> buildingOptions =
            GetTeammateBuildingOptions(teammates[0]);

        if (buildingOptions.Count == 0)
            return;

        int? selectedOptionIndex = await SpyInfiltrateScreen.ShowSelectionWithSync(buildingOptions, Owner);
        if (selectedOptionIndex == null)
            return;

        PlayRandomVoice();

        var selectedOption = buildingOptions[(int)selectedOptionIndex];
        await SpyChoiceHelper.ExecuteAttackEffect(ctx, this, teammates[0], selectedOption.PowerType, IsUpgraded);
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

            bool isValid = CommonCardValues.GetSellablePowerTypes().Any(t => t.IsAssignableFrom(powerType)) ||
                           powerType.Name.Contains("MCV", StringComparison.OrdinalIgnoreCase) ||
                           powerType.Name.Contains("Super", StringComparison.OrdinalIgnoreCase);

            if (!isValid)
                continue;

            string title = GetPowerTitle(power);
            string description = GetAttackEffectDescription(powerType);
            string iconPath = GetPowerIconPath(power);

            options.Add((powerType, title, description, iconPath));
            addedTypes.Add(powerType);
        }

        return options;
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
            object? icon = power.GetType().GetProperty("Icon")?.GetValue(power);
            if (icon != null)
            {
                object? path = icon.GetType().GetProperty("Path")?.GetValue(icon);
                if (path is string pathStr && !string.IsNullOrEmpty(pathStr))
                {
                    return pathStr;
                }
            }
        }
        catch { }

        return "res://RedAlert2ModResources/images/packed/card_portraits/allies/spyicon.png";
    }

    private string GetAttackEffectDescription(Type powerType)
    {
        string typeName = powerType.Name;
        string upgradedTag = IsUpgraded ? "_upgraded" : "_base";
        string key = string.Empty;

        if (typeName.Contains("MCV", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.base_desc" + upgradedTag;
        else if (typeName.Contains("Power", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.powerplant_desc" + upgradedTag;
        else if (typeName.Contains("Ore", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.orerefinery_desc" + upgradedTag;
        else if (typeName.Contains("BattleLab", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.battlelab_desc" + upgradedTag;
        else if (typeName.Contains("Barracks", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.barracks_desc" + upgradedTag;
        else if (typeName.Contains("WarFactory", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.warfactory_desc" + upgradedTag;
        else if (typeName.Contains("Shipyard", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.shipyard_desc" + upgradedTag;
        else if (typeName.Contains("Radar", StringComparison.OrdinalIgnoreCase) ||
                 typeName.Contains("AirForce", StringComparison.OrdinalIgnoreCase))
            key = "ui.spy.attack.radar_desc" + upgradedTag;
        else if (typeName.Contains("Super", StringComparison.OrdinalIgnoreCase))
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
        int radarTargetAgilityLoss = IsUpgraded ? RADAR_TARGET_AGILITY_LOSS_UPGRADED : RADAR_TARGET_AGILITY_LOSS;
        int radarAttackerAgilityGain = IsUpgraded ? RADAR_ATTACKER_AGILITY_GAIN_UPGRADED : RADAR_ATTACKER_AGILITY_GAIN;

        text = text.Replace("{PowerPlantDamage}", powerPlantDamage.ToString());
        text = text.Replace("{PowerPlantDamageUpgraded}", POWER_PLANT_DAMAGE_UPGRADED.ToString());
        text = text.Replace("{RadarWeak}", radarWeak.ToString());
        text = text.Replace("{RadarWeakUpgraded}", RADAR_WEAK_UPGRADED.ToString());
        text = text.Replace("{OreRefineryCredits}", oreRefineryCredits.ToString());
        text = text.Replace("{OreRefineryCreditsUpgraded}", ORE_REFINERY_CREDITS_UPGRADED.ToString());
        text = text.Replace("{PowerPlantEnergyGain}", powerPlantEnergyGain.ToString());
        text = text.Replace("{PowerPlantEnergyGainUpgraded}", POWER_PLANT_ENERGY_GAIN_UPGRADED.ToString());
        text = text.Replace("{RadarTargetAgilityLoss}", radarTargetAgilityLoss.ToString());
        text = text.Replace("{RadarTargetAgilityLossUpgraded}", RADAR_TARGET_AGILITY_LOSS_UPGRADED.ToString());
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