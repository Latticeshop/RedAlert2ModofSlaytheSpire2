using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Powers;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军基地车 - 能力牌
/// 1费，升级后0费，打出后在["美国大兵", "灰熊坦克"]中选择一张加入手牌，并显示能力图标
/// </summary>
public sealed class AlliedMCV : CardModel
{
    public AlliedMCV() : base(1, CardType.Power, CardRarity.Basic, TargetType.Self) { }

    // 修正图片路径为实际文件名 mcvicon.png
    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";

    /// <summary>
    /// 固有词条 - 每场战斗开始时自动出现在手牌
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 应用基地车能力（用于显示图标）
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<AlliedMCVPower>(Owner.Creature, 1m, Owner.Creature, this);

        // 使用 CombatState.CreateCard 创建正确初始化的卡牌副本
        List<CardModel> availableCards = new()
        {
            Owner.Creature.CombatState.CreateCard(ModelDb.Card<AmericanSoldier>(), Owner),
            Owner.Creature.CombatState.CreateCard(ModelDb.Card<GrizzlyTank>(), Owner)
        };

        // 显示选牌界面，让玩家选择一张卡牌
        CardModel? selectedCard = await CardSelectCmd.FromChooseACardScreen(ctx, availableCards, Owner, canSkip: false);

        // 如果玩家选择了卡牌，将其加入手牌
        if (selectedCard != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后费用从1变为0
        base.EnergyCost.UpgradeBy(-1);
    }
}