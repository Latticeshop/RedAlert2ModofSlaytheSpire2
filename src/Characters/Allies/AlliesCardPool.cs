using BaseLib.Abstracts;
using Godot;

namespace Ra2Mod.Characters.Allies;

/// <summary>
/// 盟军卡池 - 使用BaseLib的CustomCardPoolModel
/// </summary>
public sealed class AlliesCardPool : CustomCardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "allies";
    public override bool IsColorless => false;
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");
}
