using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军卡池 - 使用RitsuLib的TypeListCardPoolModel
/// 卡牌通过RegisterOwnedCardPoolAttribute属性自动注册到卡池
/// </summary>
public sealed class SovietCardPool : TypeListCardPoolModel
{
    public override string Title => "soviet";
    public override string EnergyColorName => "ironclad";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_red";
    
    public static readonly Color Color = new("a02020");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("801010");
}