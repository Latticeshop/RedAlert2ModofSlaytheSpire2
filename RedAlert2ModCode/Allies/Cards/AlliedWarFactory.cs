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
using RedAlert2ModCode.Allies.UI;

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
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 使用盟军卡牌注册管理器获取所有装甲单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateVehicles(Owner);
		
		// 如果盟军重工是升级过的，创建的卡牌也显示为升级版本
		if (base.IsUpgraded)
		{
			foreach (var card in availableCards)
			{
				CardCmd.Upgrade(card);
			}
		}

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		if (selectedCard != null)
		{
			var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(Owner.Creature, 1m, Owner.Creature, this);
			
			if (trainingPower != null)
			{
				trainingPower.TrainedCardId = selectedCard.Id.Entry;
				trainingPower.UnitName = selectedCard.Title.ToString();
				trainingPower.IsUpgraded = base.IsUpgraded;
				
				// 使用图标管理器设置能力图标
				PowerIconManager.SetIcon(trainingPower, selectedCard.PortraitPath);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级效果：生成的单位序列卡牌也会升级（费用不变）
	}
}
