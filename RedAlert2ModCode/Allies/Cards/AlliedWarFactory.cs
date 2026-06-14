using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军重工 - 能力牌
/// 效果类似兵营，但提供装甲单位（灰熊坦克、IFV）
/// </summary>
public sealed class AlliedWarFactory : CardModel
{
	public AlliedWarFactory() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/gwepicon.png";

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[AlliedWarFactory] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 使用盟军卡牌注册管理器获取所有装甲单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateVehicles(Owner);
		GD.Print($"[AlliedWarFactory] 可用卡牌数量: {availableCards.Count}");
		
		// 如果盟军重工是升级过的，创建的卡牌也显示为升级版本
		if (base.IsUpgraded)
		{
			foreach (var card in availableCards)
			{
				CardCmd.Upgrade(card);
			}
		}

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		GD.Print($"[AlliedWarFactory] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			// 如果选择的是超时空矿车，直接加入摸牌堆
			if (selectedCard is ChronoMiner)
			{
				GD.Print($"[AlliedWarFactory] 选择超时空矿车，直接加入摸牌堆");
				// 克隆卡牌并加入摸牌堆
				var minerCard = selectedCard.CreateClone();
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(minerCard);
				}
				await CardPileCmd.AddGeneratedCardToCombat(minerCard, PileType.Draw, addedByPlayer: true);
			}
			else
			{
				// 其他单位：创建生产序列能力
				// 首先设置当前活跃的图标路径（这样克隆对象也能获取）
				PowerIconManager.SetCurrentIconPath(selectedCard.PortraitPath);
				
				var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(Owner.Creature, 1m, Owner.Creature, this);
				
				GD.Print($"[AlliedWarFactory] 创建的Power: {(trainingPower != null ? trainingPower.GetType().Name : "null")}");
				
				if (trainingPower != null)
				{
					GD.Print($"[AlliedWarFactory] 设置属性 - TrainedCardId={selectedCard.Id.Entry}, PortraitPath={selectedCard.PortraitPath}");
					// 使用新的 SetTrainedUnit 方法设置属性
					trainingPower.SetTrainedUnit(
						selectedCard.Id.Entry,
						selectedCard.Title.ToString(),
						selectedCard.PortraitPath,
						base.IsUpgraded
					);
				}
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
}
