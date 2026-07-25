#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Common.Utils;

public enum SpyDeployType
{
    PowerPlant,
    Radar,
    OreRefinery,
    SovietBattleLab,
    AlliedBattleLab,
    Barracks,
    WarFactory,
    Shipyard
}

public sealed class SpyDeployChoiceValues
{
    public SpyDeployType DeployType { get; }
    public string TitleKey { get; }
    public string DescriptionKey { get; }
    public string IconPath { get; }
    public int Weight { get; }

    public SpyDeployChoiceValues(SpyDeployType deployType, string titleKey, string descriptionKey, string iconPath, int weight)
    {
        DeployType = deployType;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;
        IconPath = iconPath;
        Weight = weight;
    }
}

public static class SpyChoiceHelper
{
    public static List<SpyDeployChoiceValues> GetAllDeployChoices()
    {
        return new List<SpyDeployChoiceValues>
        {
            new(SpyDeployType.PowerPlant, "ui.spy.deploy.powerplant_title", "ui.spy.deploy.powerplant_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/powerplanticon.png", 8),
            new(SpyDeployType.Radar, "ui.spy.deploy.radar_title", "ui.spy.deploy.radar_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedradaricon.png", 10),
            new(SpyDeployType.OreRefinery, "ui.spy.deploy.orerefinery_title", "ui.spy.deploy.orerefinery_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/orerefineryicon.png", 10),
            new(SpyDeployType.SovietBattleLab, "ui.spy.deploy.sovietbattlelab_title", "ui.spy.deploy.sovietbattlelab_desc", "res://RedAlert2ModResources/images/packed/card_portraits/soviets/sovietbattlelabicon.png", 4),
            new(SpyDeployType.AlliedBattleLab, "ui.spy.deploy.alliedbattlelab_title", "ui.spy.deploy.alliedbattlelab_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedbattlelabicon.png", 4),
            new(SpyDeployType.Barracks, "ui.spy.deploy.barracks_title", "ui.spy.deploy.barracks_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedbarracksicon.png", 10),
            new(SpyDeployType.WarFactory, "ui.spy.deploy.warfactory_title", "ui.spy.deploy.warfactory_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedwarfactoryicon.png", 10),
            new(SpyDeployType.Shipyard, "ui.spy.deploy.shipyard_title", "ui.spy.deploy.shipyard_desc", "res://RedAlert2ModResources/images/packed/card_portraits/allies/alliedshipyardicon.png", 10)
        };
    }

    public static List<SpyDeployChoiceValues> GetRandomDeployChoices(Player player, int count, bool upgraded)
    {
        List<SpyDeployChoiceValues> allChoices = GetAllDeployChoices();
        List<SpyDeployChoiceValues> result = new();

        for (int i = 0; i < count && allChoices.Count > 0; i++)
        {
            int totalWeight = allChoices.Sum(c => c.Weight);
            int randomValue = player.RunState.Rng.CombatCardSelection.NextInt(totalWeight);
            
            int accumulatedWeight = 0;
            int selectedIndex = 0;
            for (int j = 0; j < allChoices.Count; j++)
            {
                accumulatedWeight += allChoices[j].Weight;
                if (randomValue < accumulatedWeight)
                {
                    selectedIndex = j;
                    break;
                }
            }

            SpyDeployChoiceValues selected = allChoices[selectedIndex];
            allChoices.RemoveAt(selectedIndex);
            result.Add(selected);
        }

        return result;
    }

    public static async Task ExecuteDeployEffect(PlayerChoiceContext ctx, CardModel card, SpyDeployType deployType, bool upgraded)
    {
        switch (deployType)
        {
            case SpyDeployType.PowerPlant:
                await ExecutePowerPlantDeployEffect(ctx, card, upgraded);
                break;
            case SpyDeployType.Radar:
                await ExecuteRadarDeployEffect(ctx, card, upgraded);
                break;
            case SpyDeployType.OreRefinery:
                await ExecuteOreRefineryDeployEffect(ctx, card, upgraded);
                break;
            case SpyDeployType.SovietBattleLab:
                await ExecuteSovietBattleLabDeployEffect(ctx, card);
                break;
            case SpyDeployType.AlliedBattleLab:
                await ExecuteAlliedBattleLabDeployEffect(ctx, card);
                break;
            case SpyDeployType.Barracks:
                await ExecuteBarracksDeployEffect(ctx, card, upgraded);
                break;
            case SpyDeployType.WarFactory:
                await ExecuteWarFactoryDeployEffect(ctx, card, upgraded);
                break;
            case SpyDeployType.Shipyard:
                await ExecuteShipyardDeployEffect(ctx, card, upgraded);
                break;
        }
    }

    private static async Task ExecutePowerPlantDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        int strengthLoss = upgraded ? 7 : 5;
        if (card.DynamicVars.TryGetValue("PowerPlantDamage", out var varValue))
        {
            strengthLoss = upgraded 
                ? card.DynamicVars.TryGetValue("PowerPlantDamageUpgraded", out var upgradedVar) ? upgradedVar.IntValue : 5 
                : varValue.IntValue;
        }
        foreach (var enemy in card.Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
        {
            await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.DarkShacklesPower>(ctx, enemy, strengthLoss, card.Owner.Creature, card);
        }
    }

    private static async Task ExecuteRadarDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        int weakness = upgraded ? 3 : 2;
        if (card.DynamicVars.TryGetValue("RadarWeak", out var varValue))
        {
            weakness = upgraded 
                ? card.DynamicVars.TryGetValue("RadarWeakUpgraded", out var upgradedVar) ? upgradedVar.IntValue : 2 
                : varValue.IntValue;
        }
        foreach (var enemy in card.Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
        {
            await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(ctx, enemy, weakness, card.Owner.Creature, card);
        }
    }

    private static async Task ExecuteOreRefineryDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        int credits = upgraded ? 2000 : 1500;
        if (card.DynamicVars.TryGetValue("OreRefineryCredits", out var varValue))
        {
            credits = upgraded 
                ? card.DynamicVars.TryGetValue("OreRefineryCreditsUpgraded", out var upgradedVar) ? upgradedVar.IntValue : 1500 
                : varValue.IntValue;
        }
        var dollarPower = card.Owner.Creature.Powers.OfType<RedAlert2ModCode.Common.Powers.DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            await PowerCmd.ModifyAmount(ctx, dollarPower, credits, card.Owner.Creature, card);
        }
        else
        {
            await PowerCmd.Apply<RedAlert2ModCode.Common.Powers.DollarPower>(ctx, card.Owner.Creature, credits, card.Owner.Creature, card);
        }
    }

    private static async Task ExecuteSovietBattleLabDeployEffect(PlayerChoiceContext ctx, CardModel card)
    {
        int roll = card.Owner.RunState.Rng.CombatCardSelection.NextInt(50);
        CardModel newCard;
        if (roll == 0)
        {
            newCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<YuriPrimeCard>(), card.Owner);
        }
        else
        {
            newCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<PsiCommandoCard>(), card.Owner);
        }

        newCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteAlliedBattleLabDeployEffect(PlayerChoiceContext ctx, CardModel card)
    {
        int roll = card.Owner.RunState.Rng.CombatCardSelection.NextInt(2);
        CardModel newCard;
        if (roll == 0)
        {
            newCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<ChronoCommandos>(), card.Owner);
        }
        else
        {
            newCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<ChronoIvanCard>(), card.Owner);
        }

        newCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteBarracksDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel barracksCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            barracksCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietBarracksCard>(), card.Owner);
        }
        else
        {
            barracksCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesBarracksCard>(), card.Owner);
        }
        CardCmd.Upgrade(barracksCard);
        barracksCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(barracksCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteWarFactoryDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel warFactoryCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            warFactoryCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietWarFactory>(), card.Owner);
        }
        else
        {
            warFactoryCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedWarFactory>(), card.Owner);
        }
        CardCmd.Upgrade(warFactoryCard);
        warFactoryCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(warFactoryCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteShipyardDeployEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel shipyardCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            shipyardCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietShipyardCard>(), card.Owner);
        }
        else
        {
            shipyardCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesShipyardCard>(), card.Owner);
        }
        CardCmd.Upgrade(shipyardCard);
        shipyardCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(shipyardCard, PileType.Hand, card.Owner);
    }

    public static async Task ExecuteAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, Type powerType, bool upgraded)
    {
        string powerTypeName = powerType.Name;

        if (powerTypeName == "AlliedMCVPower" ||
            powerTypeName == "SovietMCVPower")
        {
            await ExecuteBaseAttackEffect(ctx, card, target, upgraded);
        }
        else if (powerTypeName == "ChronoSpherePower" ||
                 powerTypeName == "WeatherControllerPower" ||
                 powerTypeName == "IronCurtainPower" ||
                 powerTypeName == "NuclearMissileSiloPower")
        {
            await ExecuteSuperWeaponAttackEffect(ctx, card, target, powerType, upgraded);
        }
        else if (powerTypeName == "PowerPlantPower" ||
                 powerTypeName == "SovietPowerPlantPower" ||
                 powerTypeName == "NuclearReactorCorePower")
        {
            await ExecutePowerPlantAttackEffect(ctx, card, target, upgraded);
        }
        else if (powerTypeName == "AlliedRefineryPower" ||
                 powerTypeName == "SovietRefineryPower" ||
                 powerTypeName == "OreRefineryPower")
        {
            await ExecuteOreRefineryAttackEffect(ctx, card, target, upgraded);
        }
        else if (powerTypeName == "BattleLabPower" ||
                 powerTypeName == "SovietBattleLabPower")
        {
            await ExecuteBattleLabAttackEffect(ctx, card, target, powerType, upgraded);
        }
        else if (powerTypeName == "AlliedBarracksPower" ||
                 powerTypeName == "SovietBarracksPower")
        {
            await ExecuteBarracksAttackEffect(ctx, card, upgraded);
        }
        else if (powerTypeName == "AlliedWarFactoryPower" ||
                 powerTypeName == "SovietWarFactoryPower")
        {
            await ExecuteWarFactoryAttackEffect(ctx, card, upgraded);
        }
        else if (powerTypeName == "AlliedShipyardPower" ||
                 powerTypeName == "SovietShipyardPower")
        {
            await ExecuteShipyardAttackEffect(ctx, card, upgraded);
        }
        else if (powerTypeName == "AlliedAirForceCommandPower" ||
                 powerTypeName == "SovietRadarPower")
        {
            await ExecuteRadarAttackEffect(ctx, card, target, upgraded);
        }
    }

    private static async Task ExecuteBaseAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, bool upgraded)
    {
        decimal blockAmount = target.Creature.Block;
        if (upgraded)
        {
            blockAmount *= 2;
        }
        await CreatureCmd.GainBlock(card.Owner.Creature, blockAmount, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
    }

    private static async Task ExecutePowerPlantAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, bool upgraded)
    {
        await PlayerCmd.LoseEnergy(1, target);
        int energyGain = upgraded ? 3 : 2;
        if (card.DynamicVars.TryGetValue("PowerPlantEnergyGain", out var varValue))
        {
            energyGain = upgraded 
                ? card.DynamicVars.TryGetValue("PowerPlantEnergyGainUpgraded", out var upgradedVar) ? upgradedVar.IntValue : 2 
                : varValue.IntValue;
        }
        await PlayerCmd.GainEnergy(energyGain, card.Owner);
    }

    private static async Task ExecuteOreRefineryAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, bool upgraded)
    {
        var targetDollarPower = target.Creature.Powers.OfType<RedAlert2ModCode.Common.Powers.DollarPower>().FirstOrDefault();
        if (targetDollarPower == null || targetDollarPower.Amount <= 0)
            return;

        decimal stealAmount = (decimal)Math.Floor((double)targetDollarPower.Amount * 0.5);
        await PowerCmd.ModifyAmount(ctx, targetDollarPower, -stealAmount, card.Owner.Creature, card);

        float multiplier = upgraded ? 1.5f : 1.25f;
        decimal gainAmount = (decimal)Math.Floor((double)stealAmount * multiplier);

        var ownerDollarPower = card.Owner.Creature.Powers.OfType<RedAlert2ModCode.Common.Powers.DollarPower>().FirstOrDefault();
        if (ownerDollarPower != null)
        {
            await PowerCmd.ModifyAmount(ctx, ownerDollarPower, gainAmount, card.Owner.Creature, card);
        }
        else
        {
            await PowerCmd.Apply<RedAlert2ModCode.Common.Powers.DollarPower>(ctx, card.Owner.Creature, gainAmount, card.Owner.Creature, card);
        }
    }

    private static readonly MethodInfo _modelDbRelicMethod = typeof(ModelDb).GetMethod("Relic", 1, Type.EmptyTypes)
        ?? throw new InvalidOperationException("Could not find ModelDb.Relic<T>() method.");

    private static async Task ExecuteBattleLabAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, Type powerType, bool upgraded)
    {
        Type? relicType = null;

        if (powerType == typeof(Allies.Powers.BattleLabPower))
        {
            relicType = typeof(Allies.Relics.ChronoCommandosRelic);
        }
        else if (powerType == typeof(Soviet.Powers.SovietBattleLabPower))
        {
            relicType = typeof(Soviet.Relics.ChronoIvanRelic);
        }

        if (relicType != null && !card.Owner.Relics.Any(r => r.GetType() == relicType))
        {
            MethodInfo generic = _modelDbRelicMethod.MakeGenericMethod(relicType);
            RelicModel relic = (RelicModel)generic.Invoke(null, null)!;
            await RelicCmd.Obtain(relic.ToMutable(), card.Owner);
        }
    }

    private static async Task ExecuteBarracksAttackEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel barracksCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            barracksCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietBarracksCard>(), card.Owner);
        }
        else
        {
            barracksCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesBarracksCard>(), card.Owner);
        }
        barracksCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(barracksCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteWarFactoryAttackEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel warFactoryCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            warFactoryCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietWarFactory>(), card.Owner);
        }
        else
        {
            warFactoryCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedWarFactory>(), card.Owner);
        }
        warFactoryCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(warFactoryCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteShipyardAttackEffect(PlayerChoiceContext ctx, CardModel card, bool upgraded)
    {
        CardModel shipyardCard;
        if (FlagManager.GetPlayerFaction(card.Owner) == FlagManager.Faction.Soviet)
        {
            shipyardCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietShipyardCard>(), card.Owner);
        }
        else
        {
            shipyardCard = card.Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliesShipyardCard>(), card.Owner);
        }
        shipyardCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(shipyardCard, PileType.Hand, card.Owner);
    }

    private static async Task ExecuteRadarAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, bool upgraded)
    {
        int targetVulnerable = upgraded ? 1 : 2;
        int attackerAgilityGain = upgraded ? 3 : 2;

        if (card.DynamicVars.TryGetValue("RadarTargetVulnerable", out var lossVar))
        {
            targetVulnerable = upgraded 
                ? card.DynamicVars.TryGetValue("RadarTargetVulnerableUpgraded", out var upgradedLossVar) ? upgradedLossVar.IntValue : 2 
                : lossVar.IntValue;
        }
        if (card.DynamicVars.TryGetValue("RadarAttackerAgilityGain", out var gainVar))
        {
            attackerAgilityGain = upgraded 
                ? card.DynamicVars.TryGetValue("RadarAttackerAgilityGainUpgraded", out var upgradedGainVar) ? upgradedGainVar.IntValue : 3 
                : gainVar.IntValue;
        }

        await PowerCmd.Apply<VulnerablePower>(ctx, target.Creature, targetVulnerable, target.Creature, card);
        await PowerCmd.Apply<DexterityPower>(ctx, card.Owner.Creature, attackerAgilityGain, card.Owner.Creature, card);
    }

    private static async Task ExecuteSuperWeaponAttackEffect(PlayerChoiceContext ctx, CardModel card, Player target, Type powerType, bool upgraded)
    {
        bool shouldGiveSuperWeapon = false;
        
        foreach (var power in target.Creature.Powers)
        {
            if (power.GetType() == powerType)
            {
                shouldGiveSuperWeapon = DecrementSuperWeaponCounter(power);
                break;
            }
        }

        if (shouldGiveSuperWeapon)
        {
            CardModel? superWeaponCard = CreateSuperWeaponCard(card.Owner, powerType);
            if (superWeaponCard != null)
            {
                superWeaponCard.EnergyCost.SetCustomBaseCost(0);
                superWeaponCard.AddKeyword(CardKeyword.Exhaust);
                await CardPileCmd.AddGeneratedCardToCombat(superWeaponCard, PileType.Hand, card.Owner);
            }
        }
    }

    private static bool DecrementSuperWeaponCounter(PowerModel power)
    {
        Type powerType = power.GetType();
        var turnCounterField = powerType.GetField("_turnCounter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var getIntervalMethod = powerType.GetMethod("GetInterval", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (turnCounterField != null && getIntervalMethod != null)
        {
            int currentCounter = (int)turnCounterField.GetValue(power)!;
            int interval = (int)getIntervalMethod.Invoke(power, null)!;
            
            if (currentCounter <= 0)
            {
                turnCounterField.SetValue(power, interval);
                return true;
            }
            
            int newCounter = currentCounter - 1;
            if (newCounter <= 0)
            {
                turnCounterField.SetValue(power, interval);
                return true;
            }
            else
            {
                turnCounterField.SetValue(power, newCounter);
                return false;
            }
        }
        
        return false;
    }

    private static CardModel? CreateSuperWeaponCard(Player owner, Type powerType)
    {
        string typeName = powerType.Name;
        
        if (typeName == "WeatherControllerPower")
            return owner.Creature.CombatState.CreateCard(ModelDb.Card<RedAlert2ModCode.Allies.Cards.LightningStorm>(), owner);
        
        if (typeName == "NuclearMissileSiloPower")
            return owner.Creature.CombatState.CreateCard(ModelDb.Card<RedAlert2ModCode.Soviet.Cards.NuclearAttack>(), owner);
        
        if (typeName == "ChronoSpherePower")
            return owner.Creature.CombatState.CreateCard(ModelDb.Card<RedAlert2ModCode.Allies.Cards.ChronoWarp>(), owner);
        
        if (typeName == "IronCurtainPower")
            return owner.Creature.CombatState.CreateCard(ModelDb.Card<RedAlert2ModCode.Soviet.Cards.IronCurtain>(), owner);

        return null;
    }
}