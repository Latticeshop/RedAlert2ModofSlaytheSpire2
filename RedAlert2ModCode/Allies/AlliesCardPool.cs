using BaseLib.Abstracts;
using Godot;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军卡池 - 使用BaseLib的CustomCardPoolModel
/// </summary>
public sealed class AlliesCardPool : CustomCardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "allies";
    public override bool IsColorless => false;
    
    // 卡牌框材质路径 - 使用蓝色框
    public override string CardFrameMaterialPath => "card_frame_blue";
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");
}
