using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// Harmony补丁：拦截PowerModel.Icon属性，为自定义能力提供正确的图标
/// 因为PowerModel.Icon和PackedIconPath都不是virtual的，无法通过override/new来修改
/// </summary>
[HarmonyPatch]
public static class PowerIconPatch
{
    /// <summary>
    /// 自定义能力类型到图标路径的映射
    /// </summary>
    private static readonly Dictionary<Type, string> _customIconPaths = new()
    {
        { typeof(PowerPlantPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/powricon.png" },
        { typeof(AlliedMCVPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png" },
        { typeof(AlliedRefineryPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/reficon.png" },
        { typeof(AlliedWarFactoryPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/gwepicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietMCVPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietPowerPlantPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/npwricon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietFlakTrackDexterityPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/htkicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietRefineryPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nreficon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietWarFactoryPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwepicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietRadarPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietBattleLabPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/ntchicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietFlakCannonPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/flakicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietTerrorDronePower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/dronicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietGiantSquidPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sqdicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietTeslaCoilPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/tslaicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietTeslaCoilChargePower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/tslaicon.png" },
        { typeof(IfvTemporaryDexterityPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/fvicon.png" },
        { typeof(NightHawkTemporaryDexterityPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/shadicon.png" },
        { typeof(RocketSoldierTemporaryDexterityPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/jjeticon.png" },
        { typeof(PrismTowerPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/prisicon.png" },
        { typeof(DollarPower), "res://RedAlert2ModResources/images/packed/powers/dollar_power.png" },
        { typeof(PillboxPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/pillicon.png" },
        { typeof(StrategyTowerDefensePower), "res://RedAlert2ModResources/images/packed/card_portraits/strategy_tower_defense.png" },
        { typeof(BattleLabPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/techicon.png" },
        { typeof(PatriotMissilePower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/samicon.png" },
        { typeof(HornetPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/hornet.png" },
        { typeof(TargetLockedPower), "res://RedAlert2ModResources/images/packed/powers/target_locked.png" },
        { typeof(OilDerrickPower), "res://RedAlert2ModResources/images/packed/card_portraits/oil_derrick_power.png" },
        { typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
        { typeof(EagleMachineGunPower), "res://RedAlert2ModResources/images/packed/powers/EagleMachineGunPower.png" },
        { typeof(EagleAirStrikePower), "res://RedAlert2ModResources/images/packed/powers/EagleAirStrikePower.png" },
        { typeof(MassProductionPower), "res://RedAlert2ModResources/images/packed/powers/MassProductionPower.png" },
        { typeof(GoldMinePower), "res://RedAlert2ModResources/images/packed/powers/gold_mine_power.png" },
        { typeof(GemMinePower), "res://RedAlert2ModResources/images/packed/powers/gem_mine_power.png" },
        { typeof(GoldMineColumnPower), "res://RedAlert2ModResources/images/packed/powers/gold_mine_column_power.png" },
        { typeof(EarlyMiningPower), "res://RedAlert2ModResources/images/packed/powers/early_mining_power.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietEarlyMiningPower), "res://RedAlert2ModResources/images/packed/powers/early_mining_soviet_power.png" },
        { typeof(ChronoSpherePower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/csphicon.png" },
        { typeof(WeatherControllerPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/wethicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.IronCurtainPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/ironicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.NuclearMissileSiloPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/msslicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietPillboxPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/plticon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.BattleBunkerPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/bnkricon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.KirovPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/zepicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.V3RocketPower), "res://RedAlert2ModResources/images/packed/powers/v3.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.DreadnoughtPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/dredicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.NuclearReactorCorePower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nrcticon.png" },
        { typeof(SteelFloodPower), "res://RedAlert2ModResources/images/packed/powers/SteelFloodPower.png" },
        { typeof(KitingPower), "res://RedAlert2ModResources/images/packed/powers/KitingPower.png" },
        { typeof(OreRefineryPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/gorepicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.IndustrialPlantPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/indpicon.png" },
        { typeof(AlliedBarracksPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png" },
        { typeof(AlliedShipyardPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/ayaricon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietBarracksPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/handicon.png" },
        { typeof(RedAlert2ModCode.Soviet.Powers.SovietShipyardPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/yardicon.png" },
        { typeof(AlliedAirForceCommandPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/heliicon.png" },
        { typeof(ForceFieldPower), "res://RedAlert2ModResources/images/packed/card_portraits/forcicon.png" },
        { typeof(MineRaidPower), "res://RedAlert2ModResources/images/packed/powers/mine_raid_power.png" },
        { typeof(ErasingPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/clegicon.png" },
        };

    /// <summary>
    /// 缓存已加载的图标
    /// </summary>
    private static readonly Dictionary<Type, Texture2D> _iconCache = new();

    /// <summary>
    /// 拦截PowerModel.Icon属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.Icon), MethodType.Getter)]
    public static bool IconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        // 检查是否有自定义图标路径
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (!_iconCache.TryGetValue(type, out Texture2D icon))
            {
                if (ResourceLoader.Exists(iconPath))
                {
                    icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
                    _iconCache[type] = icon;
                }
            }
            
            if (icon != null)
            {
                __result = icon;
                return false; // 跳过原方法
            }
        }

        // 对于TrainingQueuePower，动态获取图标 战斗界面人物底部状态栏的能力小图标展示
        if (__instance is TrainingQueuePower trainingPower)
        {
            string trainingIconPath = GetTrainingQueueIconPath(trainingPower);
            if (!string.IsNullOrEmpty(trainingIconPath) && ResourceLoader.Exists(trainingIconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(trainingIconPath, null, ResourceLoader.CacheMode.Reuse);
                return false;
            }
        }

        return true; // 执行原方法
    }

    /// <summary>
    /// 拦截PowerModel.PackedIconPath属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.PackedIconPath), MethodType.Getter)]
    public static bool PackedIconPathPrefix(PowerModel __instance, ref string __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            __result = iconPath;
            return false;
        }

        // 对于TrainingQueuePower，动态获取图标
        if (__instance is TrainingQueuePower trainingPower)
        {
            string trainingIconPath = GetTrainingQueueIconPath(trainingPower);
            if (!string.IsNullOrEmpty(trainingIconPath))
            {
                __result = trainingIconPath;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 拦截PowerModel.BigIcon属性的getter
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.BigIcon), MethodType.Getter)]
    public static bool BigIconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        if (__instance == null)
            return true;

        Type type = __instance.GetType();
        
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (!_iconCache.TryGetValue(type, out Texture2D icon))
            {
                if (ResourceLoader.Exists(iconPath))
                {
                    icon = ResourceLoader.Load<Texture2D>(iconPath, null, ResourceLoader.CacheMode.Reuse);
                    _iconCache[type] = icon;
                }
            }
            
            if (icon != null)
            {
                __result = icon;
                return false;
            }
        }

        // 对于TrainingQueuePower，动态获取图标 鼠标悬停时的提示框的大图标，训练序列
        if (__instance is TrainingQueuePower trainingPower)
        {
            string trainingIconPath = GetTrainingQueueIconPath(trainingPower);
            if (!string.IsNullOrEmpty(trainingIconPath) && ResourceLoader.Exists(trainingIconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(trainingIconPath, null, ResourceLoader.CacheMode.Reuse);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取TrainingQueuePower的图标路径
    /// 按照优先级：1. TrainedUnitIconPath（直接存储，克隆后仍然有效） 2. TrainedCardId动态获取 3. PowerIconManager设置的图标（通过Handle） 4. 默认兵营图标
    /// </summary>
    private static string GetTrainingQueueIconPath(TrainingQueuePower trainingPower)
    {
        // 调试日志：打印当前能力的属性状态
        GD.Print($"[TrainingQueuePower] GetTrainingQueueIconPath调用 - InstanceId={trainingPower.InstanceId}, TrainedUnitIconPath='{trainingPower.TrainedUnitIconPath}', TrainedCardId='{trainingPower.TrainedCardId}', UnitName='{trainingPower.UnitName}'");

        // 1. 优先使用 TrainedUnitIconPath（直接存储，克隆后仍然有效，最可靠）
        if (!string.IsNullOrEmpty(trainingPower.TrainedUnitIconPath))
        {
            if (ResourceLoader.Exists(trainingPower.TrainedUnitIconPath))
            {
                GD.Print($"[TrainingQueuePower] 成功通过TrainedUnitIconPath获取图标: {trainingPower.TrainedUnitIconPath}");
                return trainingPower.TrainedUnitIconPath;
            }
            else
            {
                GD.Print($"[TrainingQueuePower] 警告: TrainedUnitIconPath={trainingPower.TrainedUnitIconPath} 路径不存在");
            }
        }
        else
        {
            GD.Print($"[TrainingQueuePower] TrainedUnitIconPath为空，跳过");
        }

        // 2. 通过 TrainedCardId 动态获取图标
        if (!string.IsNullOrEmpty(trainingPower.TrainedCardId))
        {
            CardModel? cardModel = GetCardModel(trainingPower.TrainedCardId);
            if (cardModel != null)
            {
                if (!string.IsNullOrEmpty(cardModel.PortraitPath))
                {
                    GD.Print($"[TrainingQueuePower] 成功通过TrainedCardId获取图标: {trainingPower.TrainedCardId} -> {cardModel.PortraitPath}");
                    return cardModel.PortraitPath;
                }
                else
                {
                    GD.Print($"[TrainingQueuePower] 警告: TrainedCardId={trainingPower.TrainedCardId} 的卡牌模型PortraitPath为空");
                }
            }
            else
            {
                GD.Print($"[TrainingQueuePower] 错误: 无法通过TrainedCardId={trainingPower.TrainedCardId}获取卡牌模型");
            }
        }
        else
        {
            GD.Print($"[TrainingQueuePower] TrainedCardId为空，跳过");
        }

        // 3. 检查PowerIconManager设置的图标（通过Handle查找，克隆后仍有效）
        string? customPath = PowerIconManager.GetIconPath(trainingPower);
        if (!string.IsNullOrEmpty(customPath))
        {
            GD.Print($"[TrainingQueuePower] 通过PowerIconManager获取图标: {customPath}");
            return customPath;
        }
        else
        {
            GD.Print($"[TrainingQueuePower] PowerIconManager中找不到图标");
        }

        // 4. 默认回退到兵营图标（能力刚创建时未设置训练单位是正常的）
        GD.Print($"[TrainingQueuePower] 回退到默认兵营图标（能力刚创建）");
        return "res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";
    }

    /// <summary>
    /// 根据卡牌ID获取卡牌模型
    /// </summary>
    private static CardModel? GetCardModel(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
            return null;

        string[] parts = cardId.Split('_');
        string typeName = string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        
        var cardType = Assembly.GetExecutingAssembly()
            .GetType($"RedAlert2ModCode.Allies.Cards.{typeName}");
        
        if (cardType == null)
        {
            cardType = typeof(CardModel).Assembly.GetType($"MegaCrit.Sts2.Core.Models.Cards.{typeName}");
        }
        
        if (cardType != null)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)
                ?.MakeGenericMethod(cardType);
            return method?.Invoke(null, null) as CardModel;
        }
        
        return null;
    }
}