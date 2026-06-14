using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class BarracksCard : CardModel
	{
		public BarracksCard() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

		public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";

		protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[BarracksCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 检查是否有空指部能力或牌库中有空军单位
		bool hasAirForceCommand = HasAirForceCommand();
		bool hasAirUnitInDeck = HasAirUnitInDeck();
		
		GD.Print($"[BarracksCard] 有空指部能力: {hasAirForceCommand}, 牌库有空军单位: {hasAirUnitInDeck}");

		// 使用盟军卡牌注册管理器获取所有士兵单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(Owner);
		
		// 如果没有空指部且牌库中没有空军单位，移除火箭飞行兵选项
		if (!hasAirForceCommand && !hasAirUnitInDeck)
		{
			availableCards = availableCards.Where(c => c.GetType() != typeof(RocketSoldier)).ToList();
			GD.Print($"[BarracksCard] 移除火箭飞行兵选项，剩余卡牌数量: {availableCards.Count}");
		}
		
		GD.Print($"[BarracksCard] 可用卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		GD.Print($"[BarracksCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			// 首先设置当前活跃的图标路径（这样克隆对象也能获取）
			PowerIconManager.SetCurrentIconPath(selectedCard.PortraitPath);
			
			var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(Owner.Creature, 1m, Owner.Creature, this);
			
			GD.Print($"[BarracksCard] 创建的Power: {(trainingPower != null ? trainingPower.GetType().Name : "null")}");
			
			if (trainingPower != null)
			{
				GD.Print($"[BarracksCard] 设置属性 - TrainedCardId={selectedCard.Id.Entry}, PortraitPath={selectedCard.PortraitPath}");
				trainingPower.TrainedCardId = selectedCard.Id.Entry;
				trainingPower.UnitName = selectedCard.Title.ToString();
				trainingPower.IsUpgraded = base.IsUpgraded;
				// 直接存储图标路径，确保克隆后仍然有效
				trainingPower.TrainedUnitIconPath = selectedCard.PortraitPath;
				
				// 使用图标管理器设置能力图标（原始对象可用）
				PowerIconManager.SetIcon(trainingPower, selectedCard.PortraitPath);
				
				GD.Print($"[BarracksCard] 属性设置完成 - TrainedCardId={trainingPower.TrainedCardId}, TrainedUnitIconPath={trainingPower.TrainedUnitIconPath}");
			}
		}
		else
		{
			// 取消选择：返还费用并将卡牌放回手牌
			await CardUtils.HandleCardCancellation(play, this, Owner);
		}
	}

		protected override void OnUpgrade()
		{
			// 升级效果：生成的单位序列卡牌也会升级（费用不变）
		}
		
		/// <summary>
		/// 检查玩家是否拥有空指部能力
		/// </summary>
		private bool HasAirForceCommand()
		{
			if (Owner?.Creature?.Powers == null)
				return false;
			
			// 检查是否有来自空指部的 TrainingQueuePower
			foreach (var power in Owner.Creature.Powers)
			{
				if (power is TrainingQueuePower trainingPower)
				{
					// 检查能力的源卡牌是否是空指部
					if (trainingPower.TrainedCardId == "INTRUDER")
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// 检查玩家牌库中是否有空军单位卡
		/// </summary>
		private bool HasAirUnitInDeck()
		{
			if (Owner == null)
				return false;
			
			// 检查牌库
			foreach (var card in Owner.Deck.Cards)
			{
				if (card is Intruder)
				{
					return true;
				}
			}
			
			return false;
		}
	}
