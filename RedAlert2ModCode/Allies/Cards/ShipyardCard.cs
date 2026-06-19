using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 船厂 - 盟军建筑卡
/// 0费，选择一张海军单位，创建对应的生产序列
/// </summary>
public sealed class ShipyardCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Shipyard;
	
	public ShipyardCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/ayaricon.png";
	
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.ProductionQueue.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.Shipyard.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[ShipyardCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.Shipyard.DollarValue);
			GD.Print($"[ShipyardCard] 扣除资金 {AlliesCardValues.Shipyard.DollarValue}");
		}

		// 使用盟军卡牌注册管理器获取所有海军单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateShips(Owner);
		
		GD.Print($"[ShipyardCard] 可用卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板
		var cardValuesMap = AlliesCardValues.CreateShipValuesMap();
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards, cardValuesMap);

		GD.Print($"[ShipyardCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			// 如果选择的是运输船，直接扣除资金并加入手牌
			if (selectedCard is TransportShip)
			{
				GD.Print($"[ShipyardCard] 选择运输船，直接扣除资金并加入手牌");
				
				// 获取运输船价格
				int transportPrice = AlliesCardValues.GetDollarValue(selectedCard.Id.Entry);
				
				// 扣除运输船费用
				if (dollarPower != null)
				{
					dollarPower.AddDollar(-transportPrice);
					GD.Print($"[ShipyardCard] 扣除运输船费用 {transportPrice}");
				}
				
				// 克隆卡牌并加入手牌
				var transportCard = selectedCard.CreateClone();
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(transportCard);
				}
				await CardPileCmd.AddGeneratedCardToCombat(transportCard, PileType.Hand, Owner);
			}
			else
			{
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

			// 打出后抽一张牌
			await CardPileCmd.Draw(ctx, 1, Owner);
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
