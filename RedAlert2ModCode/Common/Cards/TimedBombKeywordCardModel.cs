#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 定时炸弹词条卡牌基类
/// 自动处理定时炸弹词条效果：
/// 1. 卡牌获得消耗(Exhaust)词条
/// 2. 卡牌打出前获得指定层数的活力(Vigor)buff
/// 3. 自动添加定时炸弹悬浮提示
/// </summary>
public abstract class TimedBombKeywordCardModel : CardModel
{
    /// <summary>
    /// 活力数量，由卡牌传递
    /// </summary>
    protected int VigorAmount { get; private set; }

    protected TimedBombKeywordCardModel(int cost, CardType cardType, CardRarity cardRarity, TargetType targetType)
        : base(cost, cardType, cardRarity, targetType) { }

    /// <summary>
    /// 设置活力数量（由卡牌调用，传入最终计算好的数值）
    /// </summary>
    /// <param name="amount">活力数量</param>
    public void SetVigorAmount(int amount)
    {
        VigorAmount = amount;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = GetExtraHoverTips();
            tips.Add(ModCardKeywords.TimedBomb.CreateHoverTip());
            return tips;
        }
    }

    /// <summary>
    /// 子类重写此方法提供额外的悬浮提示
    /// </summary>
    protected abstract List<IHoverTip> GetExtraHoverTips();

    /// <summary>
    /// 在卡牌打出前触发：获得活力buff
    /// </summary>
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        await base.BeforeCardPlayed(cardPlay);

        if (cardPlay.Card == this && Owner != null && Owner.Creature != null)
        {
            // 获得活力buff（活力数值由卡牌传递）
            await PowerCmd.Apply<VigorPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                (decimal)VigorAmount,
                Owner.Creature,
                this
            );

            GD.Print($"[TimedBombKeywordCardModel] 卡牌打出前获得 {VigorAmount} 点活力");
        }
    }
}