using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode;

[ModInitializer(nameof(Initialize))]
public static class ModInitializer
{
    public const string ModId = "RedAlert2Mod";
    
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } 
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);
    
    public static void Initialize()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        
        // 注册所有盟军卡牌到盟军卡池
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AmericanSoldier));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(GrizzlyTank));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedMCV));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(BarracksCard));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(ChronoMiner));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedRefinery));
        // Rally 通过 AlliedCardRegistry.PowerCards 注册，不需要在这里重复注册
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        Logger.Info("红警2Mod加载成功！");
    }
}