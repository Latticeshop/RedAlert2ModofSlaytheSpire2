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
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietShipyardCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Shipyard;
	
	public SovietShipyardCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/yardicon.png";
	
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
		GD.Print($"[SovietShipyardCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");
		
		BuildingSoundHelper.PlayBuildingPlaceSound();

		List<CardModel> availableCards = SovietCardRegistry.CreateShips(Owner);
		GD.Print($"[SovietShipyardCard] 可用卡牌数量: {availableCards.Count}");

		var cardValuesMap = SovietCardValues.CreateShipValuesMap();
		CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(availableCards, Owner, cardValuesMap, FactionType.Soviet);

		GD.Print($"[SovietShipyardCard] 选择的卡牌: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");

		if (selectedCard != null)
		{
			ConfirmCardPlay();
			
			// 选择成功后才扣除建筑资金
			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower != null)
			{
				dollarPower.AddDollar(-(int)Values.DollarValue);
				GD.Print($"[SovietShipyardCard] 扣除建筑资金 {Values.DollarValue}");
			}

			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			
			await PowerCmd.Apply<SovietShipyardPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
			GD.Print("[SovietShipyardCard] 添加船厂能力");
			
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