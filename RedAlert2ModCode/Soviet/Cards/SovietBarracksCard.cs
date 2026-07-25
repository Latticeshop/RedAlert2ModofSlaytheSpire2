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
using RedAlert2ModCode.Soviet.Relics;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军兵营 - 建筑卡
/// 0费，选择一张士兵单位，创建对应的生产序列
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class SovietBarracksCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.Barracks;
	
	public SovietBarracksCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
	};

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

			// 每次打出都需要花费建筑资金
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

		List<CardModel> availableCards = SovietCardRegistry.CreateSoldiers(Owner);
		GD.Print($"[SovietBarracksCard] 可用卡牌数量: {availableCards.Count}");

		// 检查是否有雷达或空指部能力（T2科技解锁），如果没有则移除T2科技士兵（磁暴步兵、辐射工兵、恐怖分子）
		bool hasRadarPower = Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(SovietRadarPower).Name) ||
							 Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(Allies.Powers.AlliedAirForceCommandPower).Name);
		if (!hasRadarPower)
		{
			availableCards = availableCards.Where(c => 
				c is not SovietTeslaTrooper &&
				c is not Desolator &&
				c is not TerrorMan
			).ToList();
			GD.Print($"[SovietBarracksCard] 无雷达能力，移除T2士兵，剩余卡牌数量: {availableCards.Count}");
		}

		// 如果没有伊拉克国旗，移除辐射工兵选项
		if (!FlagManager.HasIraq(Owner))
		{
			availableCards = availableCards.Where(c => c is not Desolator).ToList();
			GD.Print($"[SovietBarracksCard] 无伊拉克国旗，移除辐射工兵选项，剩余卡牌数量: {availableCards.Count}");
		}

		// 如果没有古巴国旗，移除恐怖人选项
		if (!FlagManager.HasCuba(Owner))
		{
			availableCards = availableCards.Where(c => c is not TerrorMan).ToList();
			GD.Print($"[SovietBarracksCard] 无古巴国旗，移除恐怖人选项，剩余卡牌数量: {availableCards.Count}");
		}

		// 超时空伊文已在 CreateSoldiers 中根据遗物情况添加，此处无需重复添加

		var cardValuesMap = SovietCardValues.CreateSoldierValuesMap();
		var selectedResults = await CardSelectionSyncHelper.ShowSelectionWithQuantitySync(availableCards, Owner, cardValuesMap, FactionType.Soviet);

		GD.Print($"[SovietBarracksCard] 选择结果数量: {(selectedResults != null ? selectedResults.Count : 0)}");

		// 如果取消选择（selectedResults == null），返还能量，卡牌返回手中
		if (selectedResults == null)
		{
			GD.Print("[SovietBarracksCard] 取消选择，返还能量，卡牌返回手中");
			await CardUtils.HandleCardCancellation(play, this, Owner);
			return;
		}

		// 选择确认后才扣除资金（空选也消耗资金）
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietBarracksCard] 扣除建筑资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		await PowerCmd.Apply<SovietBarracksPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
		GD.Print("[SovietBarracksCard] 添加兵营能力");

		// 如果玩家选择了卡牌，创建对应的生产序列能力（同一批相同单位叠层）
		if (selectedResults.Count > 0)
		{
			foreach (var result in selectedResults)
			{
				CardModel selectedCard = result.Card;
				int count = result.Count;
				
				GD.Print($"[SovietBarracksCard] 创建生产序列 - CardId={selectedCard.Id.Entry}, Count={count}");
				
				int unitPrice = SovietCardValues.GetDollarValue(selectedCard.Id.Entry);
				
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
			GD.Print("[SovietBarracksCard] 空选，仅获得建筑能力");
		}

		// 无论是否选择了兵种，打出后都抽一张牌
		await CardPileCmd.Draw(ctx, 1, Owner);
	}

	protected override void OnUpgrade()
	{
	}
}