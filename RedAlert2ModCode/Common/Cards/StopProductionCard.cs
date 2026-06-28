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
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public sealed class StopProductionCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.StopProduction;

	public StopProductionCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

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
			GD.Print($"[StopProductionCard] 选择了 {selectedItems.Count} 个生产序列");

			foreach (var item in selectedItems)
			{
				if (item.Power is TrainingQueuePower trainingPower)
				{
					bool wasStopped = trainingPower.IsStopped;
					string cardId = trainingPower.TrainedCardId;
					string unitName = trainingPower.UnitName;
					bool isUpgraded = trainingPower.IsUpgraded;
					string iconPath = trainingPower.TrainedUnitIconPath;
					int unitPrice = trainingPower.UnitPrice;
					bool exhaustWhenPlayed = trainingPower.ExhaustWhenPlayed;
					int amount = trainingPower.Amount;
					Creature owner = trainingPower.Owner;

					owner.RemovePowerInternal(trainingPower);
					GD.Print($"[StopProductionCard] 移除训练队列能力: {unitName}, 层数: {amount}");

					bool newStopped = !wasStopped;

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

					if (newPower != null && amount > 1)
					{
						await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), newPower, amount - 1, owner, this);
						GD.Print($"[StopProductionCard] 恢复训练队列层数: {newPower.Amount}");
					}

					GD.Print($"[StopProductionCard] 训练队列 {unitName} 停产状态反转: {newStopped}");
				}
				else if (item.Power is RepairDepotPower repairPower)
				{
					bool wasStopped = repairPower.IsStopped;
					Creature owner = repairPower.Owner;

					owner.RemovePowerInternal(repairPower);
					GD.Print($"[StopProductionCard] 移除修理厂能力");

					bool newStopped = !wasStopped;

					await RepairDepotPower.ApplyRepairDepot(
						owner: owner,
						isStopped: newStopped
					);

					GD.Print($"[StopProductionCard] 修理厂停产状态反转: {newStopped}");
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
			queues.Add(new ProductionQueueItem
			{
				Power = power,
				Name = power.UnitName,
				IconPath = power.PackedIconPath,
				IsStopped = power.IsStopped,
				Type = "训练队列"
			});
		}

		foreach (var power in Owner.Creature.Powers.OfType<RepairDepotPower>())
		{
			queues.Add(new ProductionQueueItem
			{
				Power = power,
				Name = "维修厂",
				IconPath = power.PackedIconPath,
				IsStopped = power.IsStopped,
				Type = "修理厂"
			});
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
	}
}
