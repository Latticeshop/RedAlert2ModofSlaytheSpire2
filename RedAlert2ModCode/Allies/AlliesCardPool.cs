using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies;

public sealed class AlliesCardPool : CardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "defect";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_blue";
    
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");

    protected override CardModel[] GenerateAllCards()
    {
        return AlliedCardRegistry.GetAllCards().ToArray();
    }
}