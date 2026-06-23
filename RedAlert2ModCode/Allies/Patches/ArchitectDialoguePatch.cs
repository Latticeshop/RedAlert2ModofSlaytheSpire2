using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using RedAlert2ModCode.Allies;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// Harmony补丁：为盟军角色添加建筑师对话
/// 
/// 对话流程：
/// - 第1次访问：建筑师对盟军指挥官表示不屑
/// - 第2次访问：建筑师开始重视盟军的实力
/// - 第3次访问：建筑师承认盟军的战术能力
/// - 第4次访问：最终对决前的对话
/// </summary>
[HarmonyPatch]
public static class ArchitectDialoguePatch
{
    private static string? _cachedCharacterEntry;

    /// <summary>
    /// 获取盟军角色的ModelId.Entry
    /// 参考WineFox mod，角色ID是通过StringHelper.Slugify(type.FullName)生成的完整类型名
    /// </summary>
    private static string GetAlliesCharacterEntry()
    {
        if (_cachedCharacterEntry != null)
        {
            return _cachedCharacterEntry;
        }

        // 尝试从ModelDb获取实际的Entry
        try
        {
            if (ModelDb.Contains(typeof(Allies)))
            {
                _cachedCharacterEntry = ModelDb.GetId<Allies>().Entry;
                return _cachedCharacterEntry;
            }
        }
        catch (Exception ex)
        {
            // 记录错误但继续使用默认值
            System.Diagnostics.Debug.WriteLine($"[RedAlert2Mod] Failed to get Allies ModelId: {ex.Message}");
        }

        // 回退到完整类型名生成的Slug（参考WineFox使用STS2_WINE_FOX_CHARACTER_WINE_FOX）
        // 从截图看到实际格式是 REDALERT2MODCODE-ALLIES（连字符分隔）
        _cachedCharacterEntry = "REDALERT2MODCODE-ALLIES";
        return _cachedCharacterEntry;
    }

    /// <summary>
    /// 补丁：在TheArchitect的DefineDialogues方法后添加盟军角色的对话
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
    public static void TheArchitectDefineDialoguesPostfix(AncientDialogueSet __result)
    {
        string alliesCharacterId = GetAlliesCharacterEntry();

        // 创建盟军角色的建筑师对话
        var alliesDialogues = new[]
        {
            // 第1次访问
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 0,
                EndAttackers = ArchitectAttackers.Both
            },
            // 第2次访问
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 1,
                EndAttackers = ArchitectAttackers.Both
            },
            // 第3次访问
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 2,
                EndAttackers = ArchitectAttackers.Both
            },
            // 第4次访问
            new AncientDialogue(["", "", ""])
            {
                VisitIndex = 3,
                EndAttackers = ArchitectAttackers.Both
            }
        };

        // 添加盟军角色的建筑师对话（使用实际的ModelId.Entry）
        __result.CharacterDialogues[alliesCharacterId] = alliesDialogues;
    }

    /// <summary>
    /// 补丁：拦截GetValidDialogues方法，确保盟军角色的对话能被正确找到
    /// </summary>
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
        string alliesCharacterId = GetAlliesCharacterEntry();

        // 如果不是盟军角色，执行原方法
        if (characterId.Entry != alliesCharacterId)
        {
            return true;
        }

        // 尝试获取盟军角色的对话
        if (!__instance.CharacterDialogues.TryGetValue(alliesCharacterId, out IReadOnlyList<AncientDialogue>? characterDialogues))
        {
            return true;
        }

        // 查找匹配当前访问次数的对话
        List<AncientDialogue> exactDialogues = characterDialogues
            .Where(dialogue => dialogue.VisitIndex == charVisits)
            .ToList();
        if (exactDialogues.Count > 0)
        {
            __result = exactDialogues;
            return false;
        }

        // 查找可重复的对话
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