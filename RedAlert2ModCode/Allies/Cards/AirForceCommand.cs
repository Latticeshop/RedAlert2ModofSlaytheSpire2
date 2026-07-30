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
/// 空指部 - 能力牌
/// 效果类似兵营和盟军重工，但提供空军单位（入侵者战机等）
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class AirForceCommand : CardModel, ICancellableCardPlay
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AirForceCommand;
	
	public AirForceCommand() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
	};

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/heliicon.png";
	
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
			if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.AirForceCommand.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		GD.Print($"[AirForceCommand] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 使用盟军卡牌注册管理器获取所有空军单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateAirUnits(Owner);
		GD.Print($"[AirForceCommand] 可用卡牌数量: {availableCards.Count}");

		// 如果没有韩国国旗，移除黑鹰战机选项
		if (!FlagManager.HasSouthKorea(Owner))
		{
			availableCards = availableCards.Where(c => c.GetType() != typeof(BlackHawk)).ToList();
			GD.Print($"[AirForceCommand] 无韩国国旗，移除黑鹰战机选项，剩余卡牌数量: {availableCards.Count}");
		}
		
		// 如果空指部是升级过的，创建的卡牌也显示为升级版本
		if (base.IsUpgraded)
		{
			foreach (var card in availableCards)
			{
				CardCmd.Upgrade(card);
			}
		}

		// 使用自定义选择面板，支持多选和数量选择
		var cardValuesMap = AlliesCardValues.CreateAircraftValuesMap();
		var selectedResults = await CardSelectionSyncHelper.ShowSelectionWithQuantitySync(availableCards, Owner, cardValuesMap, FactionType.Allied);

		GD.Print($"[AirForceCommand] 选择结果数量: {(selectedResults != null ? selectedResults.Count : 0)}");

		// 如果取消选择（selectedResults == null），返还能量，卡牌返回手中
		if (selectedResults == null)
		{
			GD.Print("[AirForceCommand] 取消选择，返还能量，卡牌返回手中");
			await CardUtils.HandleCardCancellation(play, this, Owner);
			return;
		}

		// 选择确认后才扣除资金（空选也消耗资金）
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.AirForceCommand.DollarValue);
			GD.Print($"[AirForceCommand] 扣除建筑资金 {AlliesCardValues.AirForceCommand.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		await PowerCmd.Apply<AlliedAirForceCommandPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[AirForceCommand] 添加空指部能力");

		// 如果玩家选择了卡牌，创建对应的生产序列能力（同一批相同单位叠层）
		if (selectedResults.Count > 0)
		{
			foreach (var result in selectedResults)
			{
				CardModel selectedCard = result.Card;
				int count = result.Count;
				
				GD.Print($"[AirForceCommand] 创建生产序列 - CardId={selectedCard.Id.Entry}, Count={count}");
				
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

			// 添加一张空降部队到手牌（需要美国国旗）
			if (FlagManager.HasUSA(Owner))
			{
				var airborneTemplate = ModelDb.Card<AirborneDivision>();
				var airborneCard = Owner.Creature.CombatState.CreateCard(airborneTemplate, Owner);
				if (base.IsUpgraded && !airborneCard.IsUpgraded)
				{
					CardCmd.Upgrade(airborneCard);
				}
				await CardPileCmd.AddGeneratedCardToCombat(airborneCard, PileType.Hand, Owner);
				GD.Print("[AirForceCommand] 添加空降部队到手牌");
			}
			else
			{
				GD.Print("[AirForceCommand] 无美国国旗，跳过添加空降部队");
			}
		}
		else
		{
			// 空选：仅获得建筑能力，不创建生产序列
			GD.Print("[AirForceCommand] 空选，仅获得建筑能力");
		}

	}

	protected override void OnUpgrade()
	{
		// 升级效果：生成的单位序列卡牌也会升级（费用不变）
	}
}
