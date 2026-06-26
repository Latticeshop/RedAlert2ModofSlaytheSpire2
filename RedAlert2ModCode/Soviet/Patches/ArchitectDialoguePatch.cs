using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Soviet.Patches;

[HarmonyPatch]
public static class ArchitectDialoguePatch
{
    private static string? _cachedCharacterEntry;

    private static string GetSovietCharacterEntry()
    {
        if (_cachedCharacterEntry != null)
        {
            return _cachedCharacterEntry;
        }

        try
        {
            if (ModelDb.Contains(typeof(Soviet)))
            {
                _cachedCharacterEntry = ModelDb.GetId<Soviet>().Entry;
                return _cachedCharacterEntry;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RedAlert2Mod] Failed to get Soviet ModelId: {ex.Message}");
        }

        _cachedCharacterEntry = "REDALERT2MODCODE-SOVIET";
        return _cachedCharacterEntry;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
    public static void TheArchitectDefineDialoguesPostfix(AncientDialogueSet __result)
    {
        string sovietCharacterId = GetSovietCharacterEntry();

        var sovietDialogues = new[]
        {
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 0,
                EndAttackers = ArchitectAttackers.Both
            },
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 1,
                EndAttackers = ArchitectAttackers.Both
            },
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 2,
                EndAttackers = ArchitectAttackers.Both
            },
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 3,
                EndAttackers = ArchitectAttackers.Both
            }
        };

        __result.CharacterDialogues[sovietCharacterId] = sovietDialogues;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AncientDialogueSet), nameof(AncientDialogueSet.GetValidDialogues))]
    public static bool GetValidDialoguesPrefix(
        AncientDialogueSet __instance,
        ModelId characterId,
        int charVisits,
        int totalVisits,
        bool allowAnyCharacterDialogues,
        ref IEnumerable<AncientDialogue> __result)
    {
        string sovietCharacterId = GetSovietCharacterEntry();

        if (characterId.Entry != sovietCharacterId)
        {
            return true;
        }

        if (!__instance.CharacterDialogues.TryGetValue(sovietCharacterId, out IReadOnlyList<AncientDialogue>? characterDialogues))
        {
            return true;
        }

        List<AncientDialogue> exactDialogues = characterDialogues
            .Where(dialogue => dialogue.VisitIndex == charVisits)
            .ToList();
        if (exactDialogues.Count > 0)
        {
            __result = exactDialogues;
            return false;
        }

        List<AncientDialogue> repeatingDialogues = characterDialogues
            .Where(dialogue => dialogue.IsRepeating
                && (!dialogue.VisitIndex.HasValue || charVisits >= dialogue.VisitIndex.Value))
            .ToList();
        if (repeatingDialogues.Count > 0)
        {
            __result = repeatingDialogues;
            return false;
        }

        return true;
    }
}