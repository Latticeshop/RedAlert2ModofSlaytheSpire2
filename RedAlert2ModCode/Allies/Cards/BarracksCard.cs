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

		GD.Print($"[BarracksCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 使用盟军卡牌注册管理器获取所有士兵单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(Owner);
		GD.Print($"[BarracksCard] 可用卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		GD.Print($"[BarracksCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
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
	}

		protected override void OnUpgrade()
		{
			// 升级效果：生成的单位序列卡牌也会升级（费用不变）
		}
	}
