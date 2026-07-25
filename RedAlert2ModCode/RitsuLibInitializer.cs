using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Common.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace RedAlert2ModCode;

/// <summary>
/// RitsuLib框架初始化入口
/// 渐进式迁移：先初始化框架，后续逐步启用内容注册和补丁功能
/// </summary>
public static class RitsuLibInitializer
{
    public const string ModId = "RedAlert2Mod";
    
    /// <summary>
    /// RitsuLib初始化状态
    /// </summary>
    public static bool IsInitialized { get; private set; }
    
    /// <summary>
    /// RitsuLib日志器实例
    /// </summary>
    public static Logger? Logger { get; private set; }
    
    /// <summary>
    /// RitsuLib补丁器实例
    /// </summary>
    public static ModPatcher? Patcher { get; private set; }
    
    /// <summary>
    /// 初始化RitsuLib框架
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // 1. 创建RitsuLib日志器
            Logger = RitsuLibFramework.CreateLogger(ModId);
            
            // 2. 注册mod程序集（用于属性扫描和自动注册角色、卡牌等内容）
            ModTypeDiscoveryHub.RegisterModAssembly(ModId, Assembly.GetExecutingAssembly());
            
            // 3. 开始mod数据注册
            RitsuLibFramework.BeginModDataRegistration(ModId);
            
            // 4. 使用BaseLib的ModHelper将公共卡牌注册到TokenCardPool
            // 这样公共卡牌能被所有角色使用，且运行时根据持有者动态显示卡框颜色
            // 卡池查看器和奖励中的显示通过重写AllCards属性实现
            RegisterCommonCardsToTokenPool();
            
            // 5. 创建补丁器（渐进式迁移：先创建，后续逐步注册补丁）
            if (Logger != null)
            {
                Patcher = new ModPatcher(ModId, Logger, "main");
            }
            
            IsInitialized = true;
            ModInitializer.Logger.Info("RitsuLib框架初始化成功！");
        }
        catch (Exception ex)
        {
            IsInitialized = false;
            ModInitializer.Logger.Error($"RitsuLib框架初始化失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 应用RitsuLib补丁（需要在Harmony补丁之后调用）
    /// </summary>
    public static void ApplyPatches()
    {
        if (Patcher == null || !IsInitialized)
            return;
            
        try
        {
            // 应用RitsuLib补丁
            RitsuLibFramework.ApplyRequiredPatcher(Patcher, DisableMod, ModId);
            ModInitializer.Logger.Info("RitsuLib补丁应用成功！");
        }
        catch (Exception ex)
        {
            ModInitializer.Logger.Error($"RitsuLib补丁应用失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 禁用mod回调
    /// </summary>
    private static void DisableMod()
    {
        ModInitializer.Logger.Error("RitsuLib补丁失败，Mod已禁用！");
    }
    
    /// <summary>
    /// 公共卡牌不再直接注册到TokenCardPool，避免污染全局token卡池
    /// 公共卡牌通过卡池的AllCards属性单独包含（使用ModelDb.Card<T>()获取）
    /// 这样可以避免影响游戏原版非本mod的token卡牌
    /// </summary>
    private static void RegisterCommonCardsToTokenPool()
    {
        try
        {
            // 注意：不再将公共卡牌注册到TokenCardPool，以避免污染全局token卡池
            // 公共卡牌通过AlliesCardPool和SovietCardPool的AllCards属性单独包含
            
            ModInitializer.Logger.Info("公共卡牌注册策略已修改：不再直接注册到TokenCardPool，避免影响原版token卡牌");
        }
        catch (Exception ex)
        {
            ModInitializer.Logger.Error($"公共卡牌注册失败: {ex.Message}");
        }
    }
}
