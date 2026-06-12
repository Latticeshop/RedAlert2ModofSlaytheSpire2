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

public sealed class BarracksCard : CardModel
	{
		public BarracksCard() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

		public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";

		protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

			// 使用盟军卡牌注册管理器获取所有士兵单位卡
			List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(Owner);

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
