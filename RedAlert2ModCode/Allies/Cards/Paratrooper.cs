using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 伞兵 - 攻击卡
/// 1费（升级后0费），common白卡
/// 效果：将少许部队加入手牌。消耗。
/// 将6张美国大兵添加到手牌，伞兵和添加的大兵都添加消耗词条
/// </summary>
public sealed class Paratrooper : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.Paratrooper;

    public Paratrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/aparicon.png";

    /// <summary>
    /// 消耗词条
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("SoldierCount", Values.Repeat)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

        int soldierCount = IsUpgraded ? Values.GetRepeat(true) : Values.Repeat;
        
        // 将美国大兵加入手牌
        for (int i = 0; i < soldierCount; i++)
        {
            var soldierCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AmericanSoldier>(), Owner);
            // 给添加的大兵也添加消耗词条
            soldierCard.AddKeyword(CardKeyword.Exhaust);
            await CardPileCmd.AddGeneratedCardToCombat(soldierCard, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果：费用从1降低到0
        EnergyCost.SetCustomBaseCost(Values.GetCost(true));
    }
}
