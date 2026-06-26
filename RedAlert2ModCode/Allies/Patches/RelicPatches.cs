using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Patches;

[HarmonyPatch]
public static class RelicPatches
{
    #region 巨大扭蛋 / 大型胶囊 (Large Capsule)

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetStrikeForCharacter")]
    public static bool GetStrikeForCharacterPrefix(LargeCapsule __instance, CharacterModel character, ref CardModel __result)
    {
        if (!IsAlliesCharacter(character))
            return true;

        __result = __instance.Owner.RunState.CreateCard(ModelDb.Card<AmericanSoldier>(), __instance.Owner);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LargeCapsule), "GetDefendForCharacter")]
    public static bool GetDefendForCharacterPrefix(LargeCapsule __instance, CharacterModel character, ref CardModel __result)
    {
        if (!IsAlliesCharacter(character))
            return true;

        __result = __instance.Owner.RunState.CreateCard(ModelDb.Card<GrizzlyTank>(), __instance.Owner);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LargeCapsule), "AfterObtained")]
    public static void LargeCapsuleAfterObtainedPostfix(LargeCapsule __instance)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character))
            return;

        var soldier = __instance.Owner.RunState.CreateCard(ModelDb.Card<AmericanSoldier>(), __instance.Owner);
        var tank = __instance.Owner.RunState.CreateCard(ModelDb.Card<GrizzlyTank>(), __instance.Owner);
        
        _ = CardPileCmd.Add(soldier, PileType.Deck);
        _ = CardPileCmd.Add(tank, PileType.Deck);
    }

    #endregion

    #region 树叶药膏 (Leafy Poultice)

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LeafyPoultice), "AfterObtained")]
    public static void LeafyPoulticeAfterObtainedPostfix(LeafyPoultice __instance)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character))
            return;

        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        var soldier = deck.FirstOrDefault(c => c is AmericanSoldier);
        var tank = deck.FirstOrDefault(c => c is GrizzlyTank);

        List<CardTransformation> transformations = new();
        if (soldier != null)
            transformations.Add(new CardTransformation(soldier));
        if (tank != null)
            transformations.Add(new CardTransformation(tank));

        if (transformations.Any())
            _ = CardCmd.Transform(transformations, __instance.Owner.PlayerRng.Transformations);
    }

    #endregion

    #region 尼奥护身符 (Neow's Talisman)

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NeowsTalisman), "AfterObtained")]
    public static bool NeowsTalismanAfterObtainedPrefix(NeowsTalisman __instance)
    {
        // 添加 null 检查
        if (__instance.Owner == null)
            return true;

        if (!IsAlliesCharacter(__instance.Owner.Character))
            return true;

        // 升级1张美国大兵（打击牌）
        var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
        if (deck == null)
            return false;

        var soldier = deck.FirstOrDefault(c => c is AmericanSoldier && !c.IsUpgraded);
        if (soldier != null)
        {
            CardCmd.Upgrade(soldier);
        }

        // 升级1张灰熊坦克（防御牌）
        var tank = deck.FirstOrDefault(c => c is GrizzlyTank && !c.IsUpgraded);
        if (tank != null)
        {
            CardCmd.Upgrade(tank);
        }

        __instance.Flash();
        return false;
    }

    #endregion

    #region 古老牙齿 (Archaic Tooth)

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
    public static bool ArchaicToothAfterObtainedPrefix(ArchaicTooth __instance)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character))
            return true;

        var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is AlliedWallCard);
        if (wallCard != null)
        {
            var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<FortifiedWall>(), __instance.Owner);
            if (wallCard.IsUpgraded)
                CardCmd.Upgrade(fortifiedWall);
            _ = CardCmd.Transform(wallCard, fortifiedWall);
            return false;
        }

        return false;
    }

    #endregion

    private static bool IsAlliesCharacter(CharacterModel character)
    {
        return character?.Id?.Entry?.Contains("REDALERT") ?? false;
    }
}