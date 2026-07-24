// 小格子铺 | Latticeshop
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Patches;
using RedAlert2ModCode.Common.GameActions;
using RedAlert2ModCode.Common.Patches;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode;

[ModInitializer(nameof(Initialize))]
public static class ModInitializer
{
    public const string ModId = "RedAlert2Mod";
    
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } 
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);
    
    public static void Initialize()
    {
        // ========== RitsuLib框架初始化（渐进式迁移） ==========
        RitsuLibInitializer.Initialize();
        
        // ========== 原有BaseLib逻辑保持不变 ==========
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        
        // 注册盟军角色立绘补丁
        Allies.AssetHooks.Install(harmony);
        
        // 注册苏军角色立绘补丁
        Soviet.AssetHooks.Install(harmony);
        
        // 注册角色选择语音补丁
        CharacterSelectPatch.Install(harmony);

        // 注册国旗选择补丁
        FlagSelectionPatches.Install(harmony);

        // 注册刀乐能力点击补丁
        DollarPowerClickPatch.Install(harmony);
        
        // 注册手臂图片补丁
        HandTexturePatches.Install(harmony);
        
        // 注意：卡牌注册已通过 RitsuLib 的 [RegisterCard] 属性自动处理
        // 无需手动调用 ModHelper.AddModelToPool()
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        // ========== 应用RitsuLib补丁（渐进式迁移） ==========
        RitsuLibInitializer.ApplyPatches();
        
        Logger.Info("红警2Mod加载成功！（RitsuLib集成模式）");
    }
    
    private static void RegisterNetActionSubtype(Type netActionType)
    {
        try
        {
            var subtypesClass = Type.GetType("MegaCrit.Sts2.Core.GameActions.Multiplayer.INetActionSubtypes, sts2");
            if (subtypesClass == null)
            {
                Logger.Warn("INetActionSubtypes not found");
                return;
            }
            
            var subtypesField = subtypesClass.GetField("_subtypes", BindingFlags.NonPublic | BindingFlags.Static);
            if (subtypesField == null)
            {
                Logger.Warn("_subtypes field not found");
                return;
            }
            
            var currentSubtypes = (Type[])subtypesField.GetValue(null);
            var newSubtypes = new Type[currentSubtypes.Length + 1];
            Array.Copy(currentSubtypes, newSubtypes, currentSubtypes.Length);
            newSubtypes[currentSubtypes.Length] = netActionType;
            subtypesField.SetValue(null, newSubtypes);
            
            var countField = subtypesClass.GetField("Count", BindingFlags.Public | BindingFlags.Static);
            if (countField != null)
            {
                countField.SetValue(null, newSubtypes.Length);
            }
            
            Logger.Info($"Registered NetAction subtype: {netActionType.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to register NetAction subtype: {ex}");
        }
    }
}