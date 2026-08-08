using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class StopProductionCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.StopProduction;

    public StopProductionCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/stop_production.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.ProductionQueue.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("SelectCount", Values.Repeat)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 获取去重后的生产序列列表
        List<ProductionQueueItem> productionQueues = GetProductionQueues();

        if (productionQueues.Count == 0)
        {
            GD.Print("[StopProductionCard] 没有可选择的生产序列");
            return;
        }

        var selectedResult = await ProductionQueueSelectionScreen.ShowSelectionWithSync(ctx, productionQueues, Owner);

        if (selectedResult != null && selectedResult.Items.Count > 0)
        {
            GD.Print($"[StopProductionCard] 选择了 {selectedResult.Items.Count} 个生产序列选项，操作类型: {selectedResult.Action}");

            if (selectedResult.Action == ProductionQueueAction.ToggleStop)
            {
                await HandleToggleStop(selectedResult.Items);
            }
            else if (selectedResult.Action == ProductionQueueAction.CancelQueue)
            {
                await HandleCancelQueue(selectedResult.Items);
            }
        }
        else
        {
            GD.Print("[StopProductionCard] 未选择任何生产序列");
        }
    }

    /// <summary>
    /// 处理停产/恢复操作
    /// 查找所有匹配的能力（同一单位+同一状态）并正确处理
    /// </summary>
    private async Task HandleToggleStop(List<ProductionQueueItem> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            var trainingPower = item.Power as TrainingQueuePower;
            if (trainingPower == null) continue;

            int selectedCount = item.SelectedCount;
            bool wasStopped = trainingPower.IsStopped;
            string cardId = trainingPower.TrainedCardId;
            string unitName = trainingPower.UnitName;
            string iconPath = trainingPower.TrainedUnitIconPath;
            int unitPrice = trainingPower.UnitPrice;
            bool exhaustWhenPlayed = trainingPower.ExhaustWhenPlayed;
            Creature owner = trainingPower.Owner;

            // 查找所有同一单位同一状态的能力
            var matchingPowers = owner.Powers.OfType<TrainingQueuePower>()
                .Where(p => p.TrainedCardId == cardId && p.IsStopped == wasStopped)
                .ToList();

            // 计算该状态下的总层数
            int totalAmount = matchingPowers.Sum(p => p.Amount);

            GD.Print($"[StopProductionCard] 训练队列: {unitName}, 状态: {(wasStopped ? "停产" : "运行中")}, 总层数: {totalAmount}, 选中层数: {selectedCount}");

            // 移除所有匹配的能力
            foreach (var power in matchingPowers)
            {
                owner.RemovePowerInternal(power);
            }

            bool newStopped = !wasStopped;

            if (selectedCount == totalAmount)
            {
                // 全部层数状态反转
                await TrainingQueuePower.ApplyTrainingQueue(
                    owner: owner,
                    cardId: cardId,
                    unitName: unitName,
                    iconPath: iconPath,
                    unitPrice: unitPrice,
                    isUpgraded: trainingPower.IsUpgraded,
                    sourceCard: this,
                    exhaustWhenPlayed: exhaustWhenPlayed,
                    isStopped: newStopped,
                    amount: totalAmount
                );

                GD.Print($"[StopProductionCard] 训练队列 {unitName} 全部{(newStopped ? "停产" : "恢复")}");
            }
            else
            {
                int remainingAmount = totalAmount - selectedCount;

                // 创建选中层数的能力（状态反转）
                await TrainingQueuePower.ApplyTrainingQueue(
                    owner: owner,
                    cardId: cardId,
                    unitName: unitName,
                    iconPath: iconPath,
                    unitPrice: unitPrice,
                    isUpgraded: trainingPower.IsUpgraded,
                    sourceCard: this,
                    exhaustWhenPlayed: exhaustWhenPlayed,
                    isStopped: newStopped,
                    amount: selectedCount
                );

                GD.Print($"[StopProductionCard] 训练队列 {unitName} 创建 {selectedCount} 层{(newStopped ? "停产" : "运行")}状态");

                // 创建剩余层数的能力（保持原状态）
                if (remainingAmount > 0)
                {
                    await TrainingQueuePower.ApplyTrainingQueue(
                        owner: owner,
                        cardId: cardId,
                        unitName: unitName,
                        iconPath: iconPath,
                        unitPrice: unitPrice,
                        isUpgraded: trainingPower.IsUpgraded,
                        sourceCard: this,
                        exhaustWhenPlayed: exhaustWhenPlayed,
                        isStopped: wasStopped,
                        amount: remainingAmount
                    );

                    GD.Print($"[StopProductionCard] 训练队列 {unitName} 创建 {remainingAmount} 层{(wasStopped ? "停产" : "运行")}状态");
                }
            }
        }
    }

    /// <summary>
    /// 处理取消队列操作（直接移除选择的层数）
    /// 查找所有匹配的能力（同一单位+同一状态）并正确处理
    /// </summary>
    private async Task HandleCancelQueue(List<ProductionQueueItem> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            var trainingPower = item.Power as TrainingQueuePower;
            if (trainingPower == null) continue;

            int selectedCount = item.SelectedCount;
            string unitName = trainingPower.UnitName;
            Creature owner = trainingPower.Owner;

            // 查找所有同一单位同一状态的能力
            var matchingPowers = owner.Powers.OfType<TrainingQueuePower>()
                .Where(p => p.TrainedCardId == trainingPower.TrainedCardId && 
                           p.IsStopped == trainingPower.IsStopped)
                .ToList();

            // 计算该状态下的总层数
            int totalAmount = matchingPowers.Sum(p => p.Amount);

            GD.Print($"[StopProductionCard] 取消队列: {unitName}, 状态: {(trainingPower.IsStopped ? "停产" : "运行中")}, 总层数: {totalAmount}, 取消层数: {selectedCount}");

            // 移除所有匹配的能力
            foreach (var power in matchingPowers)
            {
                owner.RemovePowerInternal(power);
            }

            int remainingAmount = totalAmount - selectedCount;

            if (remainingAmount > 0)
            {
                // 重建剩余层数的能力（保持原状态）
                await TrainingQueuePower.ApplyTrainingQueue(
                    owner: owner,
                    cardId: trainingPower.TrainedCardId,
                    unitName: trainingPower.UnitName,
                    iconPath: trainingPower.TrainedUnitIconPath,
                    unitPrice: trainingPower.UnitPrice,
                    isUpgraded: trainingPower.IsUpgraded,
                    sourceCard: this,
                    exhaustWhenPlayed: trainingPower.ExhaustWhenPlayed,
                    isStopped: trainingPower.IsStopped,
                    amount: remainingAmount
                );

                GD.Print($"[StopProductionCard] 训练队列 {unitName} 取消 {selectedCount} 层，剩余 {remainingAmount} 层");
            }
            else
            {
                GD.Print($"[StopProductionCard] 训练队列 {unitName} 已完全取消");
            }
        }
    }

    protected override void OnUpgrade()
    {
    }

    /// <summary>
    /// 获取去重后的生产序列列表
    /// 按单位名称和状态（运行/停产）双重分组，分开显示不同状态的序列
    /// </summary>
    private List<ProductionQueueItem> GetProductionQueues()
    {
        List<ProductionQueueItem> queues = new();

        if (Owner?.Creature?.Powers == null)
            return queues;

        // 按单位名称和状态双重分组，运行中和停产的序列分开显示
        var groupedPowers = Owner.Creature.Powers.OfType<TrainingQueuePower>()
            .GroupBy(p => new { p.TrainedCardId, p.IsStopped });

        foreach (var group in groupedPowers)
        {
            // 取第一个作为代表
            var firstPower = group.First();
            
            // 计算该状态下的总层数
            int totalAmount = group.Sum(p => p.Amount);
            
            string statusText = group.Key.IsStopped ? "停产" : "运行中";

            queues.Add(new ProductionQueueItem
            {
                Power = firstPower,
                Name = firstPower.UnitName,
                IconPath = firstPower.PackedIconPath,
                IsStopped = group.Key.IsStopped,
                Type = "训练队列",
                TotalStacks = totalAmount,
                SelectedCount = 0
            });

            GD.Print($"[StopProductionCard] 生产序列: {firstPower.UnitName}, 状态: {statusText}, 总层数: {totalAmount}");
        }

        return queues;
    }
}
