using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 策略：塔防 - 运转卡（能力卡）
/// 2费，升级后1费
/// 效果：获得"策略：塔防"能力，加入一张带消耗的光棱塔
/// 策略：塔防能力效果：打出围墙时，若有光棱塔能力则获得1回合残影
/// </summary>
public sealed class StrategyTowerDefense : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.StrategyTowerDefense;

	public StrategyTowerDefense() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/strategy_tower_defense.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("Cost", Values.Cost)
	};

	/// <summary>
	/// 卡牌上显示策略：塔防和残影两个tip
	/// </summary>
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		ModCardKeywords.StrategyTowerDefense.CreateHoverTip(),
		HoverTipFactory.Static(StaticHoverTip.Block)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 应用策略：塔防能力
		await PowerCmd.Apply<StrategyTowerDefensePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
		
		GD.Print("[StrategyTowerDefense] 应用策略：塔防能力");

		// 创建带消耗词条的光棱塔卡牌
		var prismTowerCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<PrismTowerCard>(), Owner);
		
		if (prismTowerCard != null)
		{
			// 添加消耗词条
			prismTowerCard.AddKeyword(CardKeyword.Exhaust);
			GD.Print("[StrategyTowerDefense] 成功为光棱塔添加消耗词条");
			
			// 将卡牌加入手牌
			await CardPileCmd.AddGeneratedCardToCombat(prismTowerCard, PileType.Hand, Owner);
			GD.Print("[StrategyTowerDefense] 成功添加消耗光棱塔到手牌");
		}
	}

	protected override void OnUpgrade()
	{
		// 升级效果：费用从2降低到1
	}
}
