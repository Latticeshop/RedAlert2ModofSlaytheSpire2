using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军重工 - 建筑卡
/// 0费，选择一张装甲单位，创建对应的生产序列
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietWarFactory : CardModel, ICancellableCardPlay
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietWarFactory;
	
	public SovietWarFactory() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
	};

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwepicon.png";
	
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

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			// 每次打出都需要花费建筑资金
			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		GD.Print($"[SovietWarFactory] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		// 注：A2 预选模式下，选择在打出前完成；扣费/重工能力/生产序列由 BuildingResolutionAction 结算。
		// 自动打出兜底：若没有手动 A2 的待结算标记，则本地补开预选面板（确认后由结算动作执行效果）
		if (BuildingPrePlayHelper.TryConsumePendingResolution(this))
			return;
		if (MultiplayerSyncHelper.IsLocalPlayer(Owner))
			BuildingPrePlayHelper.OpenAutoPlayPanel(this);
	}

	/// <summary>
	/// A2 预选面板候选：与结算动作共用同一套确定性候选构建。
	/// </summary>
	public static List<CardModel> GetPrePlayCandidates(Player owner, bool isUpgraded)
	{
		List<CardModel> availableCards = SovietCardRegistry.CreateVehicles(owner);

		bool hasRepairDepot = owner.Creature.Powers.OfType<SovietRepairDepotPower>().Any();
		if (hasRepairDepot)
		{
			var mcvCard = owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietMCV>(), owner);
			if (isUpgraded)
				CardCmd.Upgrade(mcvCard);
			availableCards.Add(mcvCard);
		}

		if (!FlagManager.HasUSSR(owner))
		{
			availableCards = availableCards.Where(c => c is not TeslaTank).ToList();
		}

		if (!FlagManager.HasLibya(owner))
		{
			availableCards = availableCards.Where(c => c is not DemolitionTruckCard).ToList();
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
	}
}
