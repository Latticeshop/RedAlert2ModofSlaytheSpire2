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

namespace RedAlert2ModCode.Allies.Cards;

public sealed class AlliesBarracksCard : CardModel
	{
		private static readonly CardValueStore.CardValues Values = AlliesCardValues.Barracks;
		
		public AlliesBarracksCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

		public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";
		
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

				var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
				if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.Barracks.DollarValue)
					return false;

				return true;
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[AlliesBarracksCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");
		
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();

		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.Barracks.DollarValue);
			GD.Print($"[AlliesBarracksCard] 扣除资金 {AlliesCardValues.Barracks.DollarValue}");
		}

		// 检查是否有空指部能力或牌库中有空军单位
		bool hasAirForceCommand = HasAirForceCommand();
		bool hasAirUnitInDeck = HasAirUnitInDeck();
		
		GD.Print($"[AlliesBarracksCard] 有空指部能力: {hasAirForceCommand}, 牌库有空军单位: {hasAirUnitInDeck}");

		// 使用盟军卡牌注册管理器获取所有士兵单位卡
		List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(Owner);
		
		// 如果没有空指部且牌库中没有空军单位，移除火箭飞行兵选项
		if (!hasAirForceCommand && !hasAirUnitInDeck)
		{
			availableCards = availableCards.Where(c => c.GetType() != typeof(RocketSoldier)).ToList();
			GD.Print($"[AlliesBarracksCard] 移除火箭飞行兵选项，剩余卡牌数量: {availableCards.Count}");
		}
		
		// 如果没有空指部/雷达，移除狙击手选项（T2科技）
		if (!hasAirForceCommand)
		{
			availableCards = availableCards.Where(c => c.GetType() != typeof(Sniper)).ToList();
			GD.Print($"[AlliesBarracksCard] 移除狙击手选项，剩余卡牌数量: {availableCards.Count}");
		}
		
		GD.Print($"[AlliesBarracksCard] 可用卡牌数量: {availableCards.Count}");

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		// 传递数值映射，让UI面板能够正确显示费用
		var cardValuesMap = AlliesCardValues.CreateSoldierValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, cardValuesMap, FactionType.Allied);

		GD.Print($"[AlliesBarracksCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			// 添加兵营能力（用于出售检查和生产序列管理）
			await PowerCmd.Apply<AlliedBarracksPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
			GD.Print("[AlliesBarracksCard] 添加兵营能力");
			
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
		
		/// <summary>
		/// 检查玩家是否拥有空指部能力
		/// </summary>
		private bool HasAirForceCommand()
		{
			if (Owner?.Creature?.Powers == null)
				return false;
			
			// 检查是否有空指部能力
			if (Owner.Creature.Powers.Any(p => p is AlliedAirForceCommandPower))
			{
				return true;
			}
			
			// 兼容旧逻辑：检查是否有来自空指部的 TrainingQueuePower
			foreach (var power in Owner.Creature.Powers)
			{
				if (power is TrainingQueuePower trainingPower)
				{
					if (trainingPower.TrainedCardId == "INTRUDER")
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// 检查玩家牌库中是否有空军单位卡
		/// </summary>
		private bool HasAirUnitInDeck()
		{
			if (Owner == null)
				return false;
			
			// 检查牌库
			foreach (var card in Owner.Deck.Cards)
			{
				if (card is Intruder)
				{
					return true;
				}
			}
			
			return false;
		}
	}
