using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Ra2Mod;

[ModInitializer(nameof(Initialize))]
public static class ModInitializer
{
    public const string ModId = "Ra2Mod";
    
    public static void Initialize()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        MegaCrit.Sts2.Core.Logging.Log.Info("[Ra2Mod] 红警2Mod加载成功！");
    }
}
