using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Common;

public sealed class CommonCardPool : CardPoolModel
{
    public override string Title => "common";
    public override string EnergyColorName => "colorless";
    public override bool IsColorless => true;
    
    public override string CardFrameMaterialPath => "card_frame_colorless";
    
    public static readonly Color Color = new("808080");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("606060");

    protected override CardModel[] GenerateAllCards()
    {
        return CommonCardRegistry.GetAllSharedCards().ToArray();
    }
}