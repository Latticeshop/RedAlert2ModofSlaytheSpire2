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

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
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

		List<ProductionQueueItem> productionQueues = GetProductionQueues();

		if (productionQueues.Count == 0)
		{
			GD.Print("[StopProductionCard] 没有可选择的生产序列");
			return;
		}

		int maxSelect = base.IsUpgraded ? productionQueues.Count : (int)Values.Repeat;
		var selectedItems = await ProductionQueueSelectionScreen.ShowSelectionWithSync(productionQueues, maxSelect, Owner);

		if (selectedItems != null && selectedItems.Count > 0)
		{
			GD.Print($"[StopProductionCard] 选择了 {selectedItems.Count} 个生产序列选项");

			var groupedByPower = selectedItems.GroupBy(item => item.Power);

			foreach (var group in groupedByPower)
			{
				var trainingPower = group.Key as TrainingQueuePower;
				if (trainingPower == null) continue;

				int selectedCount = group.Count();
				bool wasStopped = trainingPower.IsStopped;
				string cardId = trainingPower.TrainedCardId;
				string unitName = trainingPower.UnitName;
				bool isUpgraded = trainingPower.IsUpgraded;
				string iconPath = trainingPower.TrainedUnitIconPath;
				int unitPrice = trainingPower.UnitPrice;
				bool exhaustWhenPlayed = trainingPower.ExhaustWhenPlayed;
				int totalAmount = trainingPower.Amount;
				Creature owner = trainingPower.Owner;

				GD.Print($"[StopProductionCard] 训练队列: {unitName}, 总层数: {totalAmount}, 选中层数: {selectedCount}");

				owner.RemovePowerInternal(trainingPower);

				bool newStopped = !wasStopped;

				if (selectedCount == totalAmount)
				{
					var newPower = await TrainingQueuePower.ApplyTrainingQueue(
						owner: owner,
						cardId: cardId,
						unitName: unitName,
						iconPath: iconPath,
						unitPrice: unitPrice,
						isUpgraded: isUpgraded,
						sourceCard: this,
						exhaustWhenPlayed: exhaustWhenPlayed,
						isStopped: newStopped
					);

					if (newPower != null && totalAmount > 1)
					{
						await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), newPower, totalAmount - 1, owner, this);
					}

					GD.Print($"[StopProductionCard] 训练队列 {unitName} 全部反转状态: {newStopped}");
				}
				else
				{
					int remainingAmount = totalAmount - selectedCount;

					var selectedPower = await TrainingQueuePower.ApplyTrainingQueue(
						owner: owner,
						cardId: cardId,
						unitName: unitName,
						iconPath: iconPath,
						unitPrice: unitPrice,
						isUpgraded: isUpgraded,
						sourceCard: this,
						exhaustWhenPlayed: exhaustWhenPlayed,
						isStopped: newStopped
					);

					if (selectedPower != null && selectedCount > 1)
					{
						await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), selectedPower, selectedCount - 1, owner, this);
					}

					GD.Print($"[StopProductionCard] 训练队列 {unitName} 创建 {selectedCount} 层{(newStopped ? "停产" : "正常")}状态");

					if (remainingAmount > 0)
					{
						var remainingPower = await TrainingQueuePower.ApplyTrainingQueue(
							owner: owner,
							cardId: cardId,
							unitName: unitName,
							iconPath: iconPath,
							unitPrice: unitPrice,
							isUpgraded: isUpgraded,
							sourceCard: this,
							exhaustWhenPlayed: exhaustWhenPlayed,
							isStopped: wasStopped
						);

						if (remainingPower != null && remainingAmount > 1)
						{
							await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), remainingPower, remainingAmount - 1, owner, this);
						}

						GD.Print($"[StopProductionCard] 训练队列 {unitName} 创建 {remainingAmount} 层{(wasStopped ? "停产" : "正常")}状态");
					}
				}
			}
		}
		else
		{
			GD.Print("[StopProductionCard] 未选择任何生产序列");
		}
	}

	protected override void OnUpgrade()
	{
	}

	private List<ProductionQueueItem> GetProductionQueues()
	{
		List<ProductionQueueItem> queues = new();

		if (Owner?.Creature?.Powers == null)
			return queues;

		foreach (var power in Owner.Creature.Powers.OfType<TrainingQueuePower>())
		{
			for (int i = 0; i < power.Amount; i++)
			{
				queues.Add(new ProductionQueueItem
				{
					Power = power,
					Name = power.UnitName,
					IconPath = power.PackedIconPath,
					IsStopped = power.IsStopped,
					Type = "训练队列",
					StackIndex = i + 1,
					TotalStacks = power.Amount
				});
			}
		}

		return queues;
	}

	public class ProductionQueueItem
	{
		public PowerModel Power { get; set; }
		public string Name { get; set; } = string.Empty;
		public string IconPath { get; set; } = string.Empty;
		public bool IsStopped { get; set; }
		public string Type { get; set; } = string.Empty;
		public int StackIndex { get; set; } = 0;
		public int TotalStacks { get; set; } = 0;
	}
}