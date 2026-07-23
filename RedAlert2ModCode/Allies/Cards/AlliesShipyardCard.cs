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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common.Cards;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 船厂 - 盟军建筑卡
/// 0费，选择一张海军单位，创建对应的生产序列
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliesShipyardCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Shipyard;
	
	public AlliesShipyardCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
	};

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

			bool hasPower = Owner.Creature.Powers.OfType<AlliedShipyardPower>().Any();
			if (!hasPower)
			{
				var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
				if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.Shipyard.DollarValue)
					return false;
			}

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[AlliesShipyardCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");
		
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();

		// 使用盟军卡牌注册管理器获取所有海军单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateShips(Owner);
		
		GD.Print($"[AlliesShipyardCard] 可用卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板
		var cardValuesMap = AlliesCardValues.CreateShipValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, cardValuesMap, FactionType.Allied);

		GD.Print($"[AlliesShipyardCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			bool hasPower = Owner.Creature.Powers.OfType<AlliedShipyardPower>().Any();
			if (!hasPower)
			{
				var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
				if (dollarPower != null)
				{
					dollarPower.AddDollar(-(int)AlliesCardValues.Shipyard.DollarValue);
					GD.Print($"[AlliesShipyardCard] 扣除建筑资金 {AlliesCardValues.Shipyard.DollarValue}");
				}
			}
			else
			{
				GD.Print("[AlliesShipyardCard] 已有船厂能力，不扣除建筑资金");
			}

			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			await PowerCmd.Apply<AlliedShipyardPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
			GD.Print("[AlliesShipyardCard] 添加船厂能力");
			
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
