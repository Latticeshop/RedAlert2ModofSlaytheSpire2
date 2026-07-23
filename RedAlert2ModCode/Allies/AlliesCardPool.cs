using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军卡池 - 使用RitsuLib的TypeListCardPoolModel
/// 卡牌通过RegisterOwnedCardPoolAttribute属性自动注册到卡池
/// </summary>
public sealed class AlliesCardPool : TypeListCardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "defect";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_blue";
    
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");
}