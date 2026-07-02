using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Soviet;

public sealed class SovietCardPool : CardPoolModel
{
    public override string Title => "soviet";
    public override string EnergyColorName => "ironclad";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_red";
    
    public static readonly Color Color = new("a02020");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("801010");

    protected override CardModel[] GenerateAllCards()
    {
        return SovietCardRegistry.GetAllCards().ToArray();
    }
}