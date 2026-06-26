using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Allies.Patches;
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
        AssetHooks.Install(harmony);
        
        // 注册苏军角色立绘补丁
        SovietAssetHooks.Install(harmony);
        
        // 注册角色选择语音补丁
        CharacterSelectPatch.Install(harmony);
        
        // 注册所有盟军卡牌到盟军卡池
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AmericanSoldier));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(GrizzlyTank));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedMCV));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliesBarracksCard));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(ChronoMiner));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliedRefinery));
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(MirageTank));  // 高科技(T2)单位
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(PrismTank));  // 高科技(T2)单位
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AircraftCarrier));  // 高科技(T2)海军单位
        ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AlliesRepairDepot));  // 修理厂
        // Rally 通过 AlliedCardRegistry.PowerCards 注册，不需要在这里重复注册
        
        // 注册所有公共卡牌到CommonCardPool
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(OilDerrickCard));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(GoldMineCard));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(GoldMineColumnCard));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(GemMineCard));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(SellMCV));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(StopProductionCard));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(Paratrooper));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(Ra2Rally));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(EagleMachineGun));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(EagleAirStrike));
        ModHelper.AddModelToPool(typeof(CommonCardPool), typeof(Eagle500kg));
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        Logger.Info("红警2Mod加载成功！");
    }
}