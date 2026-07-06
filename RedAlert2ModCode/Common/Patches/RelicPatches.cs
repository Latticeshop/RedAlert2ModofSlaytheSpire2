using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Common.Patches;

[HarmonyPatch]
public static class RelicPatches
{
    #region Large Capsule

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
    public static bool GetStrikeForCharacterPrefix(CharacterModel character, ref CardModel __result)
    {
        if (IsAlliesCharacter(character))
        {
            __result = ModelDb.Card<AmericanSoldier>();
            return false;
        }

        if (IsSovietCharacter(character))
        {
            __result = ModelDb.Card<Conscript>();
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
    public static bool GetDefendForCharacterPrefix(CharacterModel character, ref CardModel __result)
    {
        if (IsAlliesCharacter(character))
        {
            __result = ModelDb.Card<GrizzlyTank>();
            return false;
        }

        if (IsSovietCharacter(character))
        {
            __result = ModelDb.Card<RhinoTank>();
            return false;
        }

        return true;
    }

    #endregion

    #region Leafy Poultice

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LeafyPoultice), "AfterObtained")]
    public static void LeafyPoulticeAfterObtainedPostfix(LeafyPoultice __instance)
    {
        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        var allUnitCards = GetAllModUnitCards();
        var rng = __instance.Owner.PlayerRng.Transformations;

        if (IsAlliesCharacter(__instance.Owner.Character))
        {
            var soldier = deck.FirstOrDefault(c => c is AmericanSoldier);
            var tank = deck.FirstOrDefault(c => c is GrizzlyTank);
            if (soldier != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != soldier.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    _ = CardCmd.Transform(soldier, replacement);
                }
            }
            if (tank != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != tank.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    _ = CardCmd.Transform(tank, replacement);
                }
            }
        }
        else if (IsSovietCharacter(__instance.Owner.Character))
        {
            var conscript = deck.FirstOrDefault(c => c is Conscript);
            var tank = deck.FirstOrDefault(c => c is RhinoTank);
            if (conscript != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != conscript.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    _ = CardCmd.Transform(conscript, replacement);
                }
            }
            if (tank != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != tank.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    _ = CardCmd.Transform(tank, replacement);
                }
            }
        }
    }

    private static List<CardModel> GetAllModUnitCards()
    {
        List<CardModel> allUnits = new();
        allUnits.AddRange(AlliedCardRegistry.GetAllUnits());
        allUnits.AddRange(SovietCardRegistry.GetAllUnits());
        return allUnits.Where(c => !(c is AlliedWallCard || c is SovietWallCard || c is FortifiedWall || c is SovietFortifiedWall)).ToList();
    }

    #endregion

    #region Neow's Talisman

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NeowsTalisman), "AfterObtained")]
    public static void NeowsTalismanAfterObtainedPostfix(NeowsTalisman __instance)
    {
        if (__instance.Owner == null)
            return;

        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        if (deck == null)
            return;

        if (IsAlliesCharacter(__instance.Owner.Character))
        {
            var soldier = deck.FirstOrDefault(c => c is AmericanSoldier && !c.IsUpgraded);
            if (soldier != null)
                CardCmd.Upgrade(soldier);

            var tank = deck.FirstOrDefault(c => c is GrizzlyTank && !c.IsUpgraded);
            if (tank != null)
                CardCmd.Upgrade(tank);
        }
        else if (IsSovietCharacter(__instance.Owner.Character))
        {
            var conscript = deck.FirstOrDefault(c => c is Conscript && !c.IsUpgraded);
            if (conscript != null)
                CardCmd.Upgrade(conscript);

            var tank = deck.FirstOrDefault(c => c is RhinoTank && !c.IsUpgraded);
            if (tank != null)
                CardCmd.Upgrade(tank);
        }
    }

    #endregion

    #region Archaic Tooth

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArchaicTooth), "SetupForPlayer")]
    public static bool ArchaicToothSetupForPlayerPrefix(ArchaicTooth __instance, Player player, ref bool __result)
    {
        if (IsAlliesCharacter(player.Character))
        {
            if (player.Deck.Cards.Any(c => c is AlliedWallCard))
            {
                __result = true;
                return false;
            }
            return true;
        }

        if (IsSovietCharacter(player.Character))
        {
            if (player.Deck.Cards.Any(c => c is SovietWallCard))
            {
                __result = true;
                return false;
            }
            return true;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
    public static void ArchaicToothAfterObtainedPostfix(ArchaicTooth __instance)
    {
        if (IsAlliesCharacter(__instance.Owner.Character))
        {
            var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is AlliedWallCard);
            if (wallCard != null)
            {
                var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<FortifiedWall>(), __instance.Owner);
                if (wallCard.IsUpgraded)
                    CardCmd.Upgrade(fortifiedWall);
                _ = CardCmd.Transform(wallCard, fortifiedWall);
            }
        }
        else if (IsSovietCharacter(__instance.Owner.Character))
        {
            var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is SovietWallCard);
            if (wallCard != null)
            {
                var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<SovietFortifiedWall>(), __instance.Owner);
                if (wallCard.IsUpgraded)
                    CardCmd.Upgrade(fortifiedWall);
                _ = CardCmd.Transform(wallCard, fortifiedWall);
            }
        }
    }

    #endregion

    private static bool IsAlliesCharacter(CharacterModel character)
    {
        return character?.Id?.Entry?.Contains("ALLIES") ?? false;
    }

    private static bool IsSovietCharacter(CharacterModel character)
    {
        return character?.Id?.Entry?.Contains("SOVIET") ?? false;
    }
}