using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
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
    /// 覆盖默认图标路径，使用独立图片而不是图集
    /// </summary>
    public new string IconPath => ImageHelper.GetImagePath("powers/allied_mc_v_power.png");

    /// <summary>
    /// 覆盖默认图标加载
    /// </summary>
    public new Texture2D Icon => ResourceLoader.Load<Texture2D>(IconPath, null, ResourceLoader.CacheMode.Reuse);
}