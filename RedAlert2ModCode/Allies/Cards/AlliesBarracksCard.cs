using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
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

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AlliesBarracksCard : CardModel, ICancellableCardPlay
	{
		private static readonly CardValueStore.CardValues Values = AlliesCardValues.Barracks;
		
		public AlliesBarracksCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

		public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
	};

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

				// 每次打出都需要花费建筑资金
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
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		// 注：A2 预选模式下，选择在打出前完成；此处只做打出表现，
		// 自动打出兜底：若没有手动 A2 的待结算标记，则本地补开预选面板（确认后由结算动作执行效果）
		if (BuildingPrePlayHelper.TryConsumePendingResolution(this))
			return;
		if (MultiplayerSyncHelper.IsLocalPlayer(Owner))
			BuildingPrePlayHelper.OpenAutoPlayPanel(this);
		// 扣费/兵营能力/生产序列由 BuildingResolutionAction 结算。
	}

		/// <summary>
		/// A2 预选面板候选：与结算动作共用同一套确定性候选构建。
		/// </summary>
		public static List<CardModel> GetPrePlayCandidates(Player owner, bool isUpgraded)
		{
			bool hasAirForceCommand = HasAirForceCommand(owner);
			bool hasAirUnitInDeck = HasAirUnitInDeck(owner);

			List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(owner);

			if (!hasAirForceCommand && !hasAirUnitInDeck)
			{
				availableCards = availableCards.Where(c => c.GetType() != typeof(RocketSoldier)).ToList();
			}

			if (!FlagManager.HasUK(owner))
			{
				availableCards = availableCards.Where(c => c.GetType() != typeof(Sniper)).ToList();
			}

			if (isUpgraded)
			{
				foreach (var card in availableCards)
					CardCmd.Upgrade(card);
			}

			return availableCards;
		}

		protected override void OnUpgrade()
		{
			// 升级效果：生成的单位序列卡牌也会升级（费用不变）
		}
		
		/// <summary>
		/// 检查玩家是否拥有空指部或雷达能力（T2科技解锁）
		/// </summary>
		private static bool HasAirForceCommand(Player owner)
		{
			if (owner?.Creature?.Powers == null)
				return false;
			
			// 检查是否有空指部能力
			if (owner.Creature.Powers.Any(p => p.GetType().Name == typeof(AlliedAirForceCommandPower).Name))
			{
				return true;
			}
			
			// 检查是否有雷达能力
			if (owner.Creature.Powers.Any(p => p.GetType().Name == typeof(Soviet.Powers.SovietRadarPower).Name))
			{
				return true;
			}
			
			// 兼容旧逻辑：检查是否有来自空指部的 TrainingQueuePower
			foreach (var power in owner.Creature.Powers)
			{
				if (power is TrainingQueuePower trainingPower)
				{
					if (trainingPower.TrainedCardId.Contains("INTRUDER"))
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
		private static bool HasAirUnitInDeck(Player owner)
		{
			if (owner == null)
				return false;
			
			// 检查牌库
			foreach (var card in owner.Deck.Cards)
			{
				if (card is Intruder)
				{
					return true;
				}
			}
			
			return false;
		}
	}
