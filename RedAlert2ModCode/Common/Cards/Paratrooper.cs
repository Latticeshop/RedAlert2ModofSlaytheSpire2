using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;
using Godot;

namespace RedAlert2ModCode.Common.Cards;

public class Paratrooper : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Paratrooper;

	public Paratrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/aparicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

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

		bool isSoviet = Owner.Character?.Id?.Entry?.Contains("SOVIET") ?? false;
		bool isAllies = !isSoviet && (Owner.Character?.Id?.Entry?.Contains("REDALERT") ?? false);
		
		int soldierCount = isSoviet ? 9 : 6;
		
		for (int i = 0; i < soldierCount; i++)
		{
			var soldierCard = CreateSoldierCard(isAllies);
			if (soldierCard != null)
			{
				soldierCard.AddKeyword(CardKeyword.Exhaust);
				soldierCard.AddKeyword(CardKeyword.Ethereal);
				await CardPileCmd.AddGeneratedCardToCombat(soldierCard, PileType.Hand, Owner);
			}
		}
	}

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
		EnergyCost.UpgradeBy((int)Values.CostUpgraded);
	}
}