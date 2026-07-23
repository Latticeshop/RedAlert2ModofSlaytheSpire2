using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
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
            
            // 4. 获取内容注册表（后续逐步迁移内容时使用）
            // var contentRegistry = RitsuLibFramework.GetContentRegistry(ModId);
            
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
}
