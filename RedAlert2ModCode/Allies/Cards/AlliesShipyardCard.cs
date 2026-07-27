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
using RedAlert2ModCode.Common;
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
public sealed class AlliesShipyardCard : CardModel, ICancellableCardPlay
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

			// 每次打出都需要花费建筑资金
			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.Shipyard.DollarValue)
				return false;

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

		// 使用自定义选择面板，支持多选和数量选择
		var cardValuesMap = AlliesCardValues.CreateShipValuesMap();
		var selectedResults = await CardSelectionSyncHelper.ShowSelectionWithQuantitySync(availableCards, Owner, cardValuesMap, FactionType.Allied);

		GD.Print($"[AlliesShipyardCard] 选择结果数量: {(selectedResults != null ? selectedResults.Count : 0)}");

		// 如果取消选择（selectedResults == null），返还能量，卡牌返回手中
		if (selectedResults == null)
		{
			GD.Print("[AlliesShipyardCard] 取消选择，返还能量，卡牌返回手中");
			await CardUtils.HandleCardCancellation(play, this, Owner);
			return;
		}

		// 选择确认后才扣除资金（空选也消耗资金）
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.Shipyard.DollarValue);
			GD.Print($"[AlliesShipyardCard] 扣除建筑资金 {AlliesCardValues.Shipyard.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		await PowerCmd.Apply<AlliedShipyardPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[AlliesShipyardCard] 添加船厂能力");

		// 如果玩家选择了卡牌，创建对应的生产序列能力（同一批相同单位叠层）
		if (selectedResults.Count > 0)
		{
			foreach (var result in selectedResults)
			{
				CardModel selectedCard = result.Card;
				int count = result.Count;
				
				GD.Print($"[AlliesShipyardCard] 创建生产序列 - CardId={selectedCard.Id.Entry}, Count={count}");
				
				// 获取单位价格
				int unitPrice = AlliesCardValues.GetDollarValue(selectedCard.Id.Entry);
				
				// 同一批相同单位合并为一个能力（叠层）
				await TrainingQueuePower.ApplyTrainingQueue(
					owner: Owner.Creature,
					cardId: selectedCard.Id.Entry,
					unitName: selectedCard.Title.ToString(),
					iconPath: selectedCard.PortraitPath,
					unitPrice: unitPrice,
					isUpgraded: base.IsUpgraded,
					sourceCard: this,
					amount: count
				);
			}
		}
		else
		{
			// 空选：仅获得建筑能力，不创建生产序列
			GD.Print("[AlliesShipyardCard] 空选，仅获得建筑能力");
		}

		// 无论是否选择了兵种，打出后都抽一张牌
		await CardPileCmd.Draw(ctx, 1, Owner);

		// 触发城市化能力（仅在确认选择后）
		await UrbanizationPower.TriggerOnSuccessfulPlay(ctx, Owner, this);
	}

	protected override void OnUpgrade()
	{
		// 升级效果：生成的单位序列卡牌也会升级（费用不变）
	}
}
