using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Common;

public static class CommonCardRegistry
{
    public static List<Func<CardModel>> SharedPowerCards { get; } = new()
    {
        () => ModelDb.Card<SellMCV>(),
        () => ModelDb.Card<Ra2Rally>(),
        () => ModelDb.Card<StopProductionCard>(),
        () => ModelDb.Card<OilDerrickCard>(),
        () => ModelDb.Card<GoldMineCard>(),
        () => ModelDb.Card<GemMineCard>(),
        () => ModelDb.Card<GoldMineColumnCard>(),
        () => ModelDb.Card<SupportCard>(),
    };

    public static List<Func<CardModel>> SharedSpecialCards { get; } = new()
    {
        () => ModelDb.Card<Paratrooper>(),
    };

    public static List<CardModel> GetAllSharedPowerCards()
    {
        return SharedPowerCards.Select(s => s()).ToList();
    }

    public static List<CardModel> GetAllSharedSpecialCards()
    {
        return SharedSpecialCards.Select(s => s()).ToList();
    }

    public static List<CardModel> CreateSharedPowerCards(Player owner)
    {
        return SharedPowerCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> CreateSharedSpecialCards(Player owner)
    {
        return SharedSpecialCards.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }

    public static List<CardModel> GetAllSharedCards()
    {
        List<CardModel> cards = new();
        cards.AddRange(GetAllSharedPowerCards());
        cards.AddRange(GetAllSharedSpecialCards());
        return cards;
    }

    public static List<CardModel> CreateAllSharedCards(Player owner)
    {
        List<CardModel> cards = new();
        cards.AddRange(CreateSharedPowerCards(owner));
        cards.AddRange(CreateSharedSpecialCards(owner));
        return cards;
    }

    public static void RegisterSharedPowerCards(List<Func<CardModel>> targetList)
    {
        foreach (var cardFunc in SharedPowerCards)
        {
            if (!targetList.Contains(cardFunc))
            {
                targetList.Add(cardFunc);
            }
        }
    }

    public static void RegisterSharedSpecialCards(List<Func<CardModel>> targetList)
    {
        foreach (var cardFunc in SharedSpecialCards)
        {
            if (!targetList.Contains(cardFunc))
            {
                targetList.Add(cardFunc);
            }
        }
    }

    public static List<Func<CardModel>> GetAllPowerCardsForAllies()
    {
        return new List<Func<CardModel>>(SharedPowerCards);
    }

    public static List<Func<CardModel>> GetAllPowerCardsForSoviet()
    {
        return new List<Func<CardModel>>(SharedPowerCards);
    }

    public static List<Func<CardModel>> GetAllSpecialCardsForBoth()
    {
        return new List<Func<CardModel>>(SharedSpecialCards);
    }
}
