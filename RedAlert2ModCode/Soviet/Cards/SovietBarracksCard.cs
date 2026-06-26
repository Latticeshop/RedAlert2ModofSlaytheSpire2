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

		var cardValuesMap = SovietCardValues.CreateSoldierValuesMap();
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards, cardValuesMap);

		GD.Print($"[SovietBarracksCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
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