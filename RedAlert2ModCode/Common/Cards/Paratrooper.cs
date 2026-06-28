using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;
using Godot;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 伞兵 - 攻击卡（公共）
/// 1费（升级后0费），common白卡
/// 效果：将少许部队加入手牌。消耗。
/// 将6张美国大兵（盟军）或动员兵（苏军）添加到手牌，伞兵和添加的大兵都添加消耗词条
/// </summary>
public sealed class Paratrooper : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Paratrooper;

	public Paratrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/aparicon.png";

	/// <summary>
	/// 消耗词条
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

	protected override List<DynamicVar> CanonicalVars => new() { };
	
	private int GetSoldierCount()
	{
		if (Owner == null || Owner.Character == null || Owner.Character.Id == null)
			return 6;

		bool isSoviet = Owner.Character.Id.Entry?.Contains("SOVIET") ?? false;
		return isSoviet ? 9 : 6;
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

		// 根据玩家阵营决定添加哪种士兵和数量
		bool isSoviet = Owner.Character?.Id?.Entry?.Contains("SOVIET") ?? false;
		bool isAllies = !isSoviet && (Owner.Character?.Id?.Entry?.Contains("REDALERT") ?? false);
		
		int soldierCount = isSoviet ? 9 : 6;
		
		// 将士兵加入手牌
		for (int i = 0; i < soldierCount; i++)
		{
			var soldierCard = CreateSoldierCard(isAllies);
			if (soldierCard != null)
			{
				soldierCard.AddKeyword(CardKeyword.Exhaust);
				await CardPileCmd.AddGeneratedCardToCombat(soldierCard, PileType.Hand, Owner);
			}
		}
	}

	/// <summary>
	/// 创建士兵卡牌
	/// </summary>
	private CardModel? CreateSoldierCard(bool isAllies)
	{
		try
		{
			if (isAllies)
			{
				var template = ModelDb.Card<AmericanSoldier>();
				return Owner.Creature.CombatState.CreateCard(template, Owner);
			}
			else
			{
				var template = ModelDb.Card<Conscript>();
				return Owner.Creature.CombatState.CreateCard(template, Owner);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[Paratrooper] 创建士兵卡牌失败: {ex.Message}");
		}
		return null;
	}

	protected override void OnUpgrade()
	{
		// 升级效果：费用从1降低到0
		EnergyCost.UpgradeBy((int)Values.CostUpgraded);
	}
}