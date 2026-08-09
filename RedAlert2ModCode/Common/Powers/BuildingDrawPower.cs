using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Common.Powers;

/// <summary>
/// 建筑抽牌能力 - 隐藏能力，所有玩家默认持有（通过 DollarPower.AfterApplied 自动挂载）。
/// 打出非围墙且非防御塔的建筑牌时抽1张牌（防御塔不抽牌）。
/// 与 UrbanizationPower（城市化能力，需打出 UrbanizationCard 获得）独立运作：
///   - 本能力：所有玩家打出非围墙建筑牌时抽1张牌（从抽牌堆顶），防御塔不触发
///   - UrbanizationPower：拥有城市化能力的玩家打出非围墙建筑/防御塔牌时额外从牌堆中抽取建筑牌
/// 两者均在 AfterCardPlayed 钩子中独立触发，互不干扰。
/// </summary>
public sealed class BuildingDrawPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>隐藏能力，不在UI上展示</summary>
    protected override bool IsVisibleInternal => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { };

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // MCV 的抽牌由 BuildingResolutionAction 在“先获得建筑、后抽牌”的顺序中处理，
        // 避免手牌满时先抽牌导致建筑卡无法入手；这里跳过，防止重复抽牌。
        if (cardPlay.Card is AlliedMCV or SovietMCV)
        {
            GD.Print($"[BuildingDrawPower] MCV 卡牌抽牌延后到获得建筑之后，跳过此处");
            return;
        }

        // 只有非围墙且非防御塔的建筑才触发抽牌（围墙和防御塔都不触发）
        if (!CardUtils.IsNonWallNonDefenseTowerBuilding(cardPlay.Card))
            return;

        // 选择面板类建筑卡（重工、兵营、MCV 等）在玩家取消选择时会调用 CardUtils.HandleCardCancellation，
        // 并标记本次打出已取消。取消则跳过抽牌，仅在成功打出时触发。
        if (CardUtils.WasCardPlayCancelled(cardPlay))
        {
            GD.Print($"[BuildingDrawPower] 卡牌 {cardPlay.Card.Id.Entry} 已取消选择，跳过建筑抽牌");
            return;
        }

        GD.Print($"[BuildingDrawPower] 打出建筑牌 {cardPlay.Card.Id.Entry}，抽1张牌");
        await CardPileCmd.Draw(choiceContext, 1, base.Owner.Player);
    }
}
