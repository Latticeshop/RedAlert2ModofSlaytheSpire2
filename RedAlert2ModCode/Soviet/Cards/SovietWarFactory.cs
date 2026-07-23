using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军重工 - 建筑卡
/// 0费，选择一张装甲单位，创建对应的生产序列
/// </summary>
public sealed class SovietWarFactory : CardModel
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

			bool hasPower = Owner.Creature.Powers.OfType<SovietWarFactoryPower>().Any();
			if (!hasPower)
			{
				var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
				if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
					return false;
			}

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		GD.Print($"[SovietWarFactory] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		List<CardModel> availableCards = SovietCardRegistry.CreateVehicles(Owner);
		GD.Print($"[SovietWarFactory] 可用卡牌数量: {availableCards.Count}");

		// 如果没有苏联国旗，移除磁能坦克选项
		if (!FlagManager.HasUSSR(Owner))
		{
			availableCards = availableCards.Where(c => c is not TeslaTank).ToList();
			GD.Print($"[SovietWarFactory] 无苏联国旗，移除磁能坦克选项，剩余卡牌数量: {availableCards.Count}");
		}

		// 如果没有利比亚国旗，移除自爆卡车选项
		if (!FlagManager.HasLibya(Owner))
		{
			availableCards = availableCards.Where(c => c is not DemolitionTruckCard).ToList();
			GD.Print($"[SovietWarFactory] 无利比亚国旗，移除自爆卡车选项，剩余卡牌数量: {availableCards.Count}");
		}

		var cardValuesMap = SovietCardValues.CreateVehicleValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, cardValuesMap, FactionType.Soviet);

		GD.Print($"[SovietWarFactory] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
			bool hasPower = Owner.Creature.Powers.OfType<SovietWarFactoryPower>().Any();
			if (!hasPower)
			{
				var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
				if (dollarPower != null)
				{
					dollarPower.AddDollar(-(int)Values.DollarValue);
					GD.Print($"[SovietWarFactory] 扣除建筑资金 {Values.DollarValue}");
				}
			}
			else
			{
				GD.Print("[SovietWarFactory] 已有重工能力，不扣除建筑资金");
			}

			// 添加重工能力（用于科技线检查），每次打出都增加层数
			await PowerCmd.Apply<SovietWarFactoryPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
			GD.Print("[SovietWarFactory] 添加重工能力");

			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			int unitPrice = SovietCardValues.GetDollarValue(selectedCard.Id.Entry);
			
			bool exhaustWhenPlayed = selectedCard is not WarMiner;
			
			await TrainingQueuePower.ApplyTrainingQueue(
				owner: Owner.Creature,
				cardId: selectedCard.Id.Entry,
				unitName: selectedCard.Title.ToString(),
				iconPath: selectedCard.PortraitPath,
				unitPrice: unitPrice,
				isUpgraded: base.IsUpgraded,
				sourceCard: this,
				exhaustWhenPlayed: exhaustWhenPlayed
			);
			
			GD.Print($"[SovietWarFactory] 应用训练队列 - CardId={selectedCard.Id.Entry}, ExhaustWhenPlayed={exhaustWhenPlayed}");

			await CardPileCmd.Draw(ctx, 1, Owner);
		}
		else
		{
			await CardUtils.HandleCardCancellation(play, this, Owner);
		}
	}

	protected override void OnUpgrade()
	{
	}
}