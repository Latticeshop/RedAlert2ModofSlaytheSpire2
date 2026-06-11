using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 盟军基地车能力 - 用于显示能力图标
/// </summary>
public sealed class AlliedMCVPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 使用mod资源路径，而不是游戏默认路径
    /// </summary>
    public new string IconPath => "res://RedAlert2ModResources/images/packed/powers/allied_mc_v_power.png";

    /// <summary>
    /// 使用mod资源路径加载图标
    /// </summary>
    public new Texture2D Icon => ResourceLoader.Load<Texture2D>(IconPath, null, ResourceLoader.CacheMode.Reuse);
}