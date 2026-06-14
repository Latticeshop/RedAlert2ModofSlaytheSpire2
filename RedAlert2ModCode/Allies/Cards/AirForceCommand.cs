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
/// 空指部 - 能力牌
/// 效果类似兵营和盟军重工，但提供空军单位（入侵者战机等）
/// </summary>
public sealed class AirForceCommand : CardModel
{
	public AirForceCommand() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/heliicon.png";

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		GD.Print($"[AirForceCommand] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 使用盟军卡牌注册管理器获取所有空军单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateAirUnits(Owner);
		GD.Print($"[AirForceCommand] 可用卡牌数量: {availableCards.Count}");
		
		// 如果空指部是升级过的，创建的卡牌也显示为升级版本
		if (base.IsUpgraded)
		{
			foreach (var card in availableCards)
			{
				CardCmd.Upgrade(card);
			}
		}

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		GD.Print($"[AirForceCommand] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
			// 首先设置当前活跃的图标路径（这样克隆对象也能获取）
			PowerIconManager.SetCurrentIconPath(selectedCard.PortraitPath);
			
			var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(Owner.Creature, 1m, Owner.Creature, this);
			
			GD.Print($"[AirForceCommand] 创建的Power: {(trainingPower != null ? trainingPower.GetType().Name : "null")}");
			
			if (trainingPower != null)
			{
				GD.Print($"[AirForceCommand] 设置属性 - TrainedCardId={selectedCard.Id.Entry}, PortraitPath={selectedCard.PortraitPath}");
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

	protected override void OnUpgrade()
	{
		// 升级效果：生成的单位序列卡牌也会升级（费用不变）
	}
}
