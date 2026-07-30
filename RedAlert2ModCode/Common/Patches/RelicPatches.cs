using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LeafyPoultice), "AfterObtained")]
    public static bool LeafyPoulticeAfterObtainedPrefix(LeafyPoultice __instance, ref System.Threading.Tasks.Task __result)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character) && !IsSovietCharacter(__instance.Owner.Character))
            return true;

        __result = LeafyPoulticeTransformAsync(__instance);
        return false;
    }

    private static async System.Threading.Tasks.Task LeafyPoulticeTransformAsync(LeafyPoultice __instance)
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
                    await CardCmd.Transform(soldier, replacement);
                }
            }
            if (tank != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != tank.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    await CardCmd.Transform(tank, replacement);
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
                    await CardCmd.Transform(conscript, replacement);
                }
            }
            if (tank != null)
            {
                var targets = allUnitCards.Where(t => t.Id.Entry != tank.Id.Entry).ToList();
                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    await CardCmd.Transform(tank, replacement);
                }
            }
        }

        var playerCreature = __instance.Owner.Creature;
        if (playerCreature != null)
        {
            int newMaxHp = 0;
            var maxHpProperty = playerCreature.GetType().GetProperty("MaxHp");
            if (maxHpProperty != null && maxHpProperty.CanWrite)
            {
                int currentMaxHp = (int)maxHpProperty.GetValue(playerCreature);
                newMaxHp = Math.Max(1, currentMaxHp - 12);
                maxHpProperty.SetValue(playerCreature, newMaxHp);
            }
            else
            {
                var maxHpField = playerCreature.GetType().GetField("_maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
                if (maxHpField != null)
                {
                    int currentMaxHp = (int)maxHpField.GetValue(playerCreature);
                    newMaxHp = Math.Max(1, currentMaxHp - 12);
                    maxHpField.SetValue(playerCreature, newMaxHp);
                }
            }

            var hpProperty = playerCreature.GetType().GetProperty("Hp");
            if (hpProperty != null && hpProperty.CanWrite)
            {
                int currentHp = (int)hpProperty.GetValue(playerCreature);
                int newHp = Math.Max(1, Math.Min(currentHp - 12, newMaxHp));
                hpProperty.SetValue(playerCreature, newHp);
            }
            else
            {
                var hpField = playerCreature.GetType().GetField("_hp", BindingFlags.Instance | BindingFlags.NonPublic);
                if (hpField != null)
                {
                    int currentHp = (int)hpField.GetValue(playerCreature);
                    int newHp = Math.Max(1, Math.Min(currentHp - 12, newMaxHp));
                    hpField.SetValue(playerCreature, newHp);
                }
            }
        }
    }

    private static List<CardModel> GetAllModUnitCards()
    {
        List<CardModel> allUnits = new();
        allUnits.AddRange(AlliedCardRegistry.GetAllUnits());
        allUnits.AddRange(SovietCardRegistry.GetAllUnits());
        return allUnits;
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

    #region New Leaf

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NewLeaf), "AfterObtained")]
    public static bool NewLeafAfterObtainedPrefix(NewLeaf __instance, ref System.Threading.Tasks.Task __result)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character) && !IsSovietCharacter(__instance.Owner.Character))
            return true;

        __result = NewLeafTransformAsync(__instance);
        return false;
    }

    private static async System.Threading.Tasks.Task NewLeafTransformAsync(NewLeaf __instance)
    {
        var prefs = new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(
            MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs.TransformSelectionPrompt,
            1,
            1
        );

        var selectedCards = (await MegaCrit.Sts2.Core.Commands.CardSelectCmd.FromDeckGeneric(
            player: __instance.Owner,
            prefs: prefs,
            filter: card => !IsWallCard(card) && card.Type != CardType.Curse
        )).ToList();

        if (selectedCards.Any())
        {
            var selectedCard = selectedCards.First();

            if (IsModUnitCard(selectedCard))
            {
                var allUnitCards = GetAllModUnitCards();
                var rng = __instance.Owner.PlayerRng.Transformations;
                var targets = allUnitCards.Where(t => t.Id.Entry != selectedCard.Id.Entry).ToList();

                if (targets.Any())
                {
                    var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                    await MegaCrit.Sts2.Core.Commands.CardCmd.Transform(selectedCard, replacement);
                }
            }
            else
            {
                await MegaCrit.Sts2.Core.Commands.CardCmd.TransformToRandom(selectedCard, __instance.Owner.RunState.Rng.Niche);
            }
        }
    }

    /// <summary>
    /// 所有 Mod 单位卡类型缓存（合并盟军和苏军，含特殊单位卡和 MCV，不含 Paratrooper 伞兵）。
    /// 通过注册类的 GetAllUnitTypes() 方法动态获取，避免硬编码单位列表。
    /// </summary>
    private static HashSet<Type>? _allModUnitTypes;

    private static HashSet<Type> GetAllModUnitTypes()
    {
        if (_allModUnitTypes != null)
            return _allModUnitTypes;

        var types = new HashSet<Type>();
        types.UnionWith(AlliedCardRegistry.GetAllUnitTypes());
        types.UnionWith(SovietCardRegistry.GetAllUnitTypes());
        _allModUnitTypes = types;
        return types;
    }

    private static bool IsModUnitCard(CardModel card)
    {
        return GetAllModUnitTypes().Contains(card.GetType());
    }

    /// <summary>
    /// 判断是否为围墙/坚固围墙卡（这些卡不可被转换）
    /// </summary>
    private static bool IsWallCard(CardModel card)
    {
        return card is AlliedWallCard || card is FortifiedWall ||
               card is SovietWallCard || card is SovietFortifiedWall;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
    public static bool ArchaicToothAfterObtainedPrefix(ArchaicTooth __instance, ref System.Threading.Tasks.Task __result)
    {
        if (!IsAlliesCharacter(__instance.Owner.Character) && !IsSovietCharacter(__instance.Owner.Character))
            return true;

        __result = ArchaicToothAfterObtainedAsync(__instance);
        return false;
    }

    private static async System.Threading.Tasks.Task ArchaicToothAfterObtainedAsync(ArchaicTooth __instance)
    {
        if (IsAlliesCharacter(__instance.Owner.Character))
        {
            var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is AlliedWallCard);
            if (wallCard != null)
            {
                var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<FortifiedWall>(), __instance.Owner);
                if (wallCard.IsUpgraded)
                    CardCmd.Upgrade(fortifiedWall);
                await CardCmd.Transform(wallCard, fortifiedWall);
            }
            return;
        }

        if (IsSovietCharacter(__instance.Owner.Character))
        {
            var wallCard = __instance.Owner.Deck.Cards.FirstOrDefault(c => c is SovietWallCard);
            if (wallCard != null)
            {
                var fortifiedWall = __instance.Owner.RunState.CreateCard(ModelDb.Card<SovietFortifiedWall>(), __instance.Owner);
                if (wallCard.IsUpgraded)
                    CardCmd.Upgrade(fortifiedWall);
                await CardCmd.Transform(wallCard, fortifiedWall);
            }
            return;
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