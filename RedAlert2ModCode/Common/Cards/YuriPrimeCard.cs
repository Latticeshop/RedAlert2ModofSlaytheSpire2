using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Random;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Common.Cards;

public sealed class YuriPrimeCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.YuriPrime;

	public YuriPrimeCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/other/yurpicon.png";

	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("CardCount", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Infiltrator.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("YuriAttack", "Yuri");
		UnitVoiceHelper.PlayUnitVoice("Yuri", "Yuri");

		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

		List<Type> unitPool = YuriCard.GetUnitPool(IsUpgraded);
		int cardCount = Values.MagicNumber;
		int actualCount = Math.Min(cardCount, unitPool.Count);

		if (actualCount == 0)
			return;

		Rng rng = Owner.RunState.Rng.CombatCardSelection;
		List<Type> shuffledPool = unitPool.OrderBy(_ => rng.NextInt(int.MaxValue)).ToList();
		List<Type> selectedTypes = shuffledPool.Take(actualCount).ToList();

		foreach (Type selectedUnitType in selectedTypes)
		{
			try
			{
				var template = (CardModel)typeof(ModelDb)
					.GetMethod("Card")
					.MakeGenericMethod(selectedUnitType)
					.Invoke(null, null);

				CardModel unitCard = Owner.Creature.CombatState.CreateCard(template, Owner);
				if (unitCard != null)
				{
					unitCard.AddKeyword(CardKeyword.Exhaust);
					await CardPileCmd.AddGeneratedCardToCombat(unitCard, PileType.Hand, Owner);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[YuriPrimeCard] 创建单位卡牌失败: {ex.Message}");
			}
		}

		GD.Print($"[YuriPrimeCard] 生成了 {selectedTypes.Count} 张不同的随机单位卡牌");
	}
}
