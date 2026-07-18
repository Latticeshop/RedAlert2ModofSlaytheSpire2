// 小格子铺 | Latticeshop
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Patches;
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
        
        // 注册所有盟军卡牌到盟军卡池
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AmericanSoldier));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(GrizzlyTank));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedMCV));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliesBarracksCard));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(ChronoMiner));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedRefinery));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(MirageTank));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(PrismTank));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AircraftCarrier));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliesRepairDepot));
        
        // 公共卡牌和盟军专属卡牌通过各阵营 CardPool.GenerateAllCards() -> *CardRegistry 获取，无需在此注册
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        Logger.Info("红警2Mod加载成功！");
    }
}