using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using RedAlert2ModCode.Allies;
using SovietCharacter = RedAlert2ModCode.Soviet.Soviet;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// Harmony补丁：为盟军和苏军角色添加建筑师对话
/// 
/// 对话机制：
/// - charVisits = TotalWins（角色总胜场数）
/// - 优先匹配VisitIndex == charVisits的对话
/// - 若无匹配，回退到IsRepeating的对话池
/// - 所有对话同时设置VisitIndex（精确匹配）和IsRepeating（回退）
/// </summary>
[HarmonyPatch]
public static class ArchitectDialoguePatch
{
    private static string? _cachedAlliesEntry;
    private static string? _cachedSovietEntry;

    private static string GetAlliesCharacterEntry()
    {
        if (_cachedAlliesEntry != null)
            return _cachedAlliesEntry;

        try
        {
            if (ModelDb.Contains(typeof(Allies)))
            {
                _cachedAlliesEntry = ModelDb.GetId<Allies>().Entry;
                return _cachedAlliesEntry;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RedAlert2Mod] Failed to get Allies ModelId: {ex.Message}");
        }

        _cachedAlliesEntry = "RED_ALERT2_MOD_CHARACTER_ALLIES";
        return _cachedAlliesEntry;
    }

    private static string GetSovietCharacterEntry()
    {
        if (_cachedSovietEntry != null)
            return _cachedSovietEntry;

        try
        {
            if (ModelDb.Contains(typeof(SovietCharacter)))
            {
                _cachedSovietEntry = ModelDb.GetId<SovietCharacter>().Entry;
                return _cachedSovietEntry;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RedAlert2Mod] Failed to get Soviet ModelId: {ex.Message}");
        }

        _cachedSovietEntry = "RED_ALERT2_MOD_CHARACTER_SOVIET";
        return _cachedSovietEntry;
    }

    /// <summary>
    /// 补丁：在TheArchitect的DefineDialogues方法后添加盟军和苏军角色的对话
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
    public static void TheArchitectDefineDialoguesPostfix(AncientDialogueSet __result)
    {
        string alliesId = GetAlliesCharacterEntry();
        var alliesDialogues = CreateDialogues(
            (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4));
        __result.CharacterDialogues[alliesId] = alliesDialogues;
        GD.Print($"[ArchitectDialoguePatch] Allies: CharacterDialogues key='{alliesId}', dialogue count={alliesDialogues.Length}");

        string sovietId = GetSovietCharacterEntry();
        var sovietDialogues = CreateDialogues(
            (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (6, 4));
        __result.CharacterDialogues[sovietId] = sovietDialogues;
        GD.Print($"[ArchitectDialoguePatch] Soviet: CharacterDialogues key='{sovietId}', dialogue count={sovietDialogues.Length}");

        GD.Print($"[ArchitectDialoguePatch] All CharacterDialogues keys: {string.Join(", ", __result.CharacterDialogues.Keys)}");
    }

    /// <summary>
    /// 补丁：在LoadDialogues后打印选中的对话信息
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TheArchitect), "LoadDialogue")]
    public static void LoadDialoguePostfix(TheArchitect __instance)
    {
        try
        {
            var dialogueField = typeof(TheArchitect).GetField("_dialogue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dialogue = dialogueField?.GetValue(__instance) as AncientDialogue;
            if (dialogue != null)
            {
                int? visitIdx = dialogue.VisitIndex;
                bool isRepeating = dialogue.IsRepeating;
                int lineCount = dialogue.Lines?.Count ?? 0;
                string firstLineKey = dialogue.Lines?[0]?.LineText?.LocEntryKey ?? "null";
                GD.Print($"[ArchitectDialoguePatch] LoadDialogue result: VisitIndex={visitIdx}, IsRepeating={isRepeating}, Lines={lineCount}, FirstLineKey={firstLineKey}");
            }
            else
            {
                GD.Print($"[ArchitectDialoguePatch] LoadDialogue result: Dialogue is NULL!");
            }
        }
        catch (System.Exception ex)
        {
            GD.Print($"[ArchitectDialoguePatch] LoadDialoguePostfix error: {ex.Message}");
        }
    }

    /// <summary>
    /// 补丁：在GetValidDialogues后打印返回的对话列表
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AncientDialogueSet), "GetValidDialogues")]
    public static void GetValidDialoguesPostfix(AncientDialogueSet __instance, ModelId characterId, int charVisits, int totalVisits, IEnumerable<AncientDialogue> __result)
    {
        try
        {
            var list = __result?.ToList();
            if (list != null && list.Count > 0)
            {
                GD.Print($"[ArchitectDialoguePatch] GetValidDialogues: charId={characterId.Entry}, charVisits={charVisits}, totalVisits={totalVisits}, result count={list.Count}, first VisitIndex={list[0].VisitIndex}");
            }
            else
            {
                GD.Print($"[ArchitectDialoguePatch] GetValidDialogues: charId={characterId.Entry}, charVisits={charVisits}, totalVisits={totalVisits}, result is EMPTY!");
                // 打印所有可用的 CharacterDialogues 键
                GD.Print($"[ArchitectDialoguePatch] Available CharacterDialogues keys: {string.Join(", ", __instance.CharacterDialogues.Keys)}");
            }
        }
        catch (System.Exception ex)
        {
            GD.Print($"[ArchitectDialoguePatch] GetValidDialoguesPostfix error: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建对话：同时设置VisitIndex（精确匹配）和IsRepeating（回退池）
    /// PopulateLines只在JSON有r后缀时覆盖IsRepeating，C#中设为true不会被覆盖
    /// </summary>
    private static AncientDialogue[] CreateDialogues(params (int visitIndex, int lineCount)[] specs)
    {
        var list = new List<AncientDialogue>();
        foreach (var (visitIndex, lineCount) in specs)
        {
            var lines = new string[lineCount];
            for (int i = 0; i < lineCount; i++)
                lines[i] = "";
            list.Add(new AncientDialogue(lines)
            {
                VisitIndex = visitIndex,
                IsRepeating = true,
                EndAttackers = ArchitectAttackers.Both
            });
        }
        return list.ToArray();
    }
}