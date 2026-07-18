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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军兵营 - 建筑卡
/// 0费，选择一张士兵单位，创建对应的生产序列
/// </summary>
public sealed class SovietBarracksCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Barracks;
	
	public SovietBarracksCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/handicon.png";
	
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

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[SovietBarracksCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");
		
		BuildingSoundHelper.PlayBuildingPlaceSound();

		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietBarracksCard] 扣除资金 {Values.DollarValue}");
		}

		List<CardModel> availableCards = SovietCardRegistry.CreateSoldiers(Owner);
		GD.Print($"[SovietBarracksCard] 可用卡牌数量: {availableCards.Count}");

		// 检查是否有雷达能力，如果没有则移除T2科技士兵（磁暴步兵、辐射工兵、恐怖分子）
		bool hasRadarPower = Owner.Creature.Powers.Any(p => p is SovietRadarPower);
		if (!hasRadarPower)
		{
			availableCards = availableCards.Where(c => 
				c is not SovietTeslaTrooper &&
				c is not Desolator &&
				c is not TerrorMan
			).ToList();
			GD.Print($"[SovietBarracksCard] 无雷达能力，移除T2士兵，剩余卡牌数量: {availableCards.Count}");
		}

		var cardValuesMap = SovietCardValues.CreateSoldierValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, cardValuesMap, FactionType.Soviet);

		GD.Print($"[SovietBarracksCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			await PowerCmd.Apply<SovietBarracksPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
			GD.Print("[SovietBarracksCard] 添加兵营能力");
			
			int unitPrice = SovietCardValues.GetDollarValue(selectedCard.Id.Entry);
			
			await TrainingQueuePower.ApplyTrainingQueue(
				owner: Owner.Creature,
				cardId: selectedCard.Id.Entry,
				unitName: selectedCard.Title.ToString(),
				iconPath: selectedCard.PortraitPath,
				unitPrice: unitPrice,
				isUpgraded: base.IsUpgraded,
				sourceCard: this
			);

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