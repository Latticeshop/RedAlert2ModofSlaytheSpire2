using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Common.Utils;

public static class RandomUnitHelper
{
    private static readonly HashSet<Type> ExcludedUnitTypes = new()
    {
        typeof(YuriCard),
        typeof(YuriPrimeCard),
        typeof(PsiCommandoCard),
        typeof(RoboTank)   // 遥控坦克为盟军控制中心解锁单位，尤里随机单位不能抽到
    };

    public static List<Type> GetUnitPool(bool includeT3)
    {
        List<Type> pool = new();
        
        pool.AddRange(AlliedCardRegistry.GetBasicUnitTypes());
        pool.AddRange(SovietCardRegistry.GetBasicUnitTypes());
        
        if (includeT3)
        {
            pool.AddRange(AlliedCardRegistry.GetT3UnitTypes());
            pool.AddRange(SovietCardRegistry.GetT3UnitTypes());
        }

        pool.RemoveAll(t => ExcludedUnitTypes.Contains(t));

        return pool;
    }

    public static async Task<CardModel?> CreateRandomUnitCard(Player owner, bool includeT3, bool exhaust = true)
    {
        List<Type> unitPool = GetUnitPool(includeT3);
        if (unitPool.Count == 0)
            return null;

        Rng rng = owner.RunState.Rng.CombatCardSelection;
        int randomIndex = rng.NextInt(unitPool.Count);
        Type selectedUnitType = unitPool[randomIndex];

        return await CreateUnitCard(owner, selectedUnitType, exhaust);
    }

    public static async Task<List<CardModel>> CreateRandomUnitCards(Player owner, int count, bool includeT3, bool exhaust = true)
    {
        List<CardModel> result = new();
        List<Type> unitPool = GetUnitPool(includeT3);
        
        if (unitPool.Count == 0)
            return result;

        Rng rng = owner.RunState.Rng.CombatCardSelection;
        List<Type> shuffledPool = unitPool.OrderBy(_ => rng.NextInt(int.MaxValue)).ToList();
        List<Type> selectedTypes = shuffledPool.Take(Math.Min(count, unitPool.Count)).ToList();

        foreach (Type selectedUnitType in selectedTypes)
        {
            CardModel? card = await CreateUnitCard(owner, selectedUnitType, exhaust);
            if (card != null)
            {
                result.Add(card);
            }
        }

        return result;
    }

    private static async Task<CardModel?> CreateUnitCard(Player owner, Type unitType, bool exhaust)
    {
        try
        {
            var template = (CardModel)typeof(ModelDb)
                .GetMethod("Card")
                .MakeGenericMethod(unitType)
                .Invoke(null, null);

            CardModel unitCard = owner.Creature.CombatState.CreateCard(template, owner);
            if (unitCard != null)
            {
                if (exhaust)
                {
                    unitCard.AddKeyword(CardKeyword.Exhaust);
                }
                await CardPileCmd.AddGeneratedCardToCombat(unitCard, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand, owner);
            }
            return unitCard;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[RandomUnitHelper] 创建单位卡牌失败: {ex.Message}");
            return null;
        }
    }
}
