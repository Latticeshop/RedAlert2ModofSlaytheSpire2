using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using RedAlert2ModCode.Utils;
using System.Reflection;

namespace RedAlert2ModCode.Allies.Patches;

/// <summary>
/// 角色选择补丁
/// 当在角色选择页面选中盟军角色图标时播放谭雅语音
/// </summary>
internal static class CharacterSelectPatch
{
    public static void Install(Harmony harmony)
    {
        // 拦截角色选择按钮点击事件
        var selectCharacterMethod = AccessTools.Method(
            typeof(NCharacterSelectScreen), 
            nameof(NCharacterSelectScreen.SelectCharacter),
            new[] { typeof(NCharacterSelectButton), typeof(CharacterModel) }
        );
        
        if (selectCharacterMethod != null)
        {
            harmony.Patch(
                original: selectCharacterMethod,
                postfix: new HarmonyMethod(typeof(CharacterSelectPatch), nameof(SelectCharacterPostfix))
            );
            ModInitializer.Logger.Info("[CharacterSelectPatch] 已安装角色选择补丁");
        }
        else
        {
            ModInitializer.Logger.Error("[CharacterSelectPatch] 无法找到 SelectCharacter 方法");
        }
    }

    private static void SelectCharacterPostfix(NCharacterSelectScreen __instance, CharacterModel __1)
    {
        try
        {
            // 检查是否是盟军角色
            if (__1 is Allies)
            {
                // 播放角色选择语音
                CharacterSelectAudioHelper.PlayAlliesSelectVoice();
                ModInitializer.Logger.Info("[CharacterSelectPatch] 播放盟军角色选择语音");
            }
        }
        catch (Exception ex)
        {
            ModInitializer.Logger.Error($"[CharacterSelectPatch] 播放语音失败: {ex.Message}");
        }
    }
}
