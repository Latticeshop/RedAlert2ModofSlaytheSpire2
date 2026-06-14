using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军基地车 - 能力牌
/// 0费，打出后在["兵营", "盟军重工", "发电厂"]中选择一张加入手牌
/// 升级后：获得的卡牌为升级版本
/// </summary>
public sealed class AlliedMCV : CardModel
{
	public AlliedMCV() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self) { }

	// 修正图片路径为实际文件名 mcvicon.png
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";

	/// <summary>
	/// 固有词条 - 每场战斗开始时自动出现在手牌
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 使用 CombatState.CreateCard 创建正确初始化的卡牌副本
		List<CardModel> availableCards = new();
		
		// 如果基地车是升级过的，创建的卡牌也显示为升级版本
		var powerPlantCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<PowerPlantCard>(), Owner);
		var refineryCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedRefinery>(), Owner);
		var barracksCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<BarracksCard>(), Owner);
		var warFactoryCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedWarFactory>(), Owner);
		
		if (base.IsUpgraded)
		{
			CardCmd.Upgrade(powerPlantCard);
			CardCmd.Upgrade(refineryCard);
			CardCmd.Upgrade(barracksCard);
			CardCmd.Upgrade(warFactoryCard);
		}
		
		availableCards.Add(powerPlantCard);
		availableCards.Add(refineryCard);
		availableCards.Add(barracksCard);
		availableCards.Add(warFactoryCard);

		// 使用自定义选择面板，支持滚轮滚动选择任意数量卡牌
		CardModel? selectedCard = await CardSelectionScreen.ShowSelection(availableCards);

		// 如果玩家选择了卡牌，才执行能力效果
		if (selectedCard != null)
		{
			// 应用基地车能力（用于显示图标）
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<AlliedMCVPower>(Owner.Creature, 1m, Owner.Creature, this);
			
			// 将选择的卡牌加入手牌
			await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, addedByPlayer: true);
		}
		else
		{
			// 取消选择：返还费用并将卡牌放回手牌
			await CardUtils.HandleCardCancellation(play, this, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后：获得的卡牌为升级版本（费用不变，仍为0费）
	}
}
