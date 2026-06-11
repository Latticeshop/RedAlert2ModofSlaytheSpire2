using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class TrainingQueuePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool IsInstanced => true;

    public string TrainedCardId { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
    }

    public new string PackedIconPath
    {
        get
        {
            CardModel? cardModel = GetCardModel(TrainedCardId);
            if (cardModel != null && !string.IsNullOrEmpty(cardModel.PortraitPath))
            {
                return cardModel.PortraitPath;
            }
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";
        }
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("UnitName", UnitName);
            return locString;
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (string.IsNullOrEmpty(TrainedCardId))
            return;

        CardModel? cardModel = GetCardModel(TrainedCardId);
        if (cardModel == null)
            return;

        CardModel tempCard = combatState.CreateCard(cardModel, base.Owner.Player);
        
        tempCard.EnergyCost.SetCustomBaseCost(0);
        
        // 使用AddKeyword方法添加消耗词条
        tempCard.AddKeyword(CardKeyword.Exhaust);

        await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Draw, addedByPlayer: true, CardPilePosition.Top);
    }

    private CardModel? GetCardModel(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
            return null;

        string[] parts = cardId.Split('_');
        string typeName = string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        
        var cardType = System.Reflection.Assembly.GetExecutingAssembly()
            .GetType($"RedAlert2ModCode.Allies.Cards.{typeName}");
        
        if (cardType == null)
        {
            cardType = typeof(CardModel).Assembly.GetType($"MegaCrit.Sts2.Core.Models.Cards.{typeName}");
        }
        
        if (cardType != null)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)
                ?.MakeGenericMethod(cardType);
            return method?.Invoke(null, null) as CardModel;
        }
        
        return null;
    }
}