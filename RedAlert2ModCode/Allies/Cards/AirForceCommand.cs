using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 空指部 - 能力牌
/// 效果类似兵营和盟军重工，但提供空军单位（入侵者战机等）
/// </summary>
public sealed class AirForceCommand : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AirForceCommand;
	
	public AirForceCommand() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/heliicon.png";
	
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.AirForceCommand.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[AirForceCommand] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.AirForceCommand.DollarValue);
			GD.Print($"[AirForceCommand] 扣除资金 {AlliesCardValues.AirForceCommand.DollarValue}");
		}

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
		// 传递数值映射，让UI面板能够正确显示费用和描述
		var cardValuesMap = AlliesCardValues.CreateAircraftValuesMap();
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards, cardValuesMap);

		GD.Print($"[AirForceCommand] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			// 获取单位价格
			int unitPrice = AlliesCardValues.GetDollarValue(selectedCard.Id.Entry);
			
			// 使用统一的训练队列能力应用方法
			await TrainingQueuePower.ApplyTrainingQueue(
				owner: Owner.Creature,
				cardId: selectedCard.Id.Entry,
				unitName: selectedCard.Title.ToString(),
				iconPath: selectedCard.PortraitPath,
				unitPrice: unitPrice,
				isUpgraded: base.IsUpgraded,
				sourceCard: this
			);
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
