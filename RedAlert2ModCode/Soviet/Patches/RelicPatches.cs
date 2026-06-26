using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class RelicPatches
{
    #region Large Capsule

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
    public static bool GetStrikeForCharacterPrefix(LargeCapsule __instance, CharacterModel character, ref CardModel __result)
    {
        if (!IsSovietCharacter(character))
            return true;

        __result = __instance.Owner.RunState.CreateCard(ModelDb.Card<Conscript>(), __instance.Owner);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
    public static bool GetDefendForCharacterPrefix(LargeCapsule __instance, CharacterModel character, ref CardModel __result)
    {
        if (!IsSovietCharacter(character))
            return true;

        __result = __instance.Owner.RunState.CreateCard(ModelDb.Card<RhinoTank>(), __instance.Owner);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LargeCapsule), "AfterObtained")]
    public static void LargeCapsuleAfterObtainedPostfix(LargeCapsule __instance)
    {
        if (!IsSovietCharacter(__instance.Owner.Character))
            return;

        var conscript = __instance.Owner.RunState.CreateCard(ModelDb.Card<Conscript>(), __instance.Owner);
        var tank = __instance.Owner.RunState.CreateCard(ModelDb.Card<RhinoTank>(), __instance.Owner);
        
        _ = CardPileCmd.Add(conscript, PileType.Deck);
        _ = CardPileCmd.Add(tank, PileType.Deck);
    }

    #endregion

    #region Leafy Poultice

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LeafyPoultice), "AfterObtained")]
    public static void LeafyPoulticeAfterObtainedPostfix(LeafyPoultice __instance)
    {
        if (!IsSovietCharacter(__instance.Owner.Character))
            return;

        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        var conscript = deck.FirstOrDefault(c => c is Conscript);
        var tank = deck.FirstOrDefault(c => c is RhinoTank);

        List<CardTransformation> transformations = new();
        if (conscript != null)
            transformations.Add(new CardTransformation(conscript));
        if (tank != null)
            transformations.Add(new CardTransformation(tank));

        if (transformations.Any())
            _ = CardCmd.Transform(transformations, __instance.Owner.PlayerRng.Transformations);
    }

    #endregion

    #region Neow's Talisman

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NeowsTalisman), "AfterObtained")]
    public static bool NeowsTalismanAfterObtainedPrefix(NeowsTalisman __instance)
    {
        // 添加 null 检查
        if (__instance.Owner == null)
            return true;

        if (!IsSovietCharacter(__instance.Owner.Character))
            return true;

        // 升级1张动员兵（打击牌）
        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        if (deck == null)
            return false;

        var conscript = deck.FirstOrDefault(c => c is Conscript && !c.IsUpgraded);
        if (conscript != null)
        {
            CardCmd.Upgrade(conscript);
        }

        // 升级1张犀牛坦克（防御牌）
        var tank = deck.FirstOrDefault(c => c is RhinoTank && !c.IsUpgraded);
        if (tank != null)
        {
            CardCmd.Upgrade(tank);
        }

        __instance.Flash();
        return false;
    }

    #endregion

    #region Archaic Tooth

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
    public static bool ArchaicToothAfterObtainedPrefix(ArchaicTooth __instance)
    {
        if (!IsSovietCharacter(__instance.Owner.Character))
            return true;

        var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is SovietWallCard);
        if (wallCard != null)
        {
            var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<SovietFortifiedWall>(), __instance.Owner);
            if (wallCard.IsUpgraded)
                CardCmd.Upgrade(fortifiedWall);
            _ = CardCmd.Transform(wallCard, fortifiedWall);
            return false;
        }

        return false;
    }

    #endregion

    private static bool IsSovietCharacter(CharacterModel character)
    {
        return character?.Id?.Entry?.Contains("SOVIET") ?? false;
    }
}