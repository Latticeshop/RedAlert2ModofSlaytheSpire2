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
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;
using Godot;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AirborneDivision : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AirborneDivision;

	public AirborneDivision() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/paraicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

	protected override List<DynamicVar> CanonicalVars => new()
    {
        new RepeatVar(Values.Repeat)
    };

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipHelper.FromCardWithUpgrade<AmericanSoldier>(() => IsUpgraded),
		HoverTipHelper.FromCardWithUpgrade<GuardianGi>(() => IsUpgraded)
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice("ParatrooperPlane", "Allies");
		UnitVoiceHelper.PlayUnitVoice("Paratrooper", "Allies");
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

		for (int i = 0; i < Values.GetRepeat(IsUpgraded); i++)
		{
			var soldierCard = CreateSoldierCard<AmericanSoldier>();
			if (soldierCard != null)
			{
				soldierCard.AddKeyword(CardKeyword.Exhaust);
				await CardPileCmd.AddGeneratedCardToCombat(soldierCard, PileType.Hand, Owner);
			}
		}

		var guardianCard = CreateSoldierCard<GuardianGi>();
		if (guardianCard != null)
		{
			guardianCard.AddKeyword(CardKeyword.Exhaust);
			await CardPileCmd.AddGeneratedCardToCombat(guardianCard, PileType.Hand, Owner);
		}
	}

	private CardModel? CreateSoldierCard<T>() where T : CardModel
	{
		try
		{
			var template = ModelDb.Card<T>();
			CardModel soldierCard = Owner.Creature.CombatState.CreateCard(template, Owner);

			if (IsUpgraded && !soldierCard.IsUpgraded)
			{
				CardCmd.Upgrade(soldierCard);
			}

			return soldierCard;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[AirborneDivision] 创建士兵卡牌失败: {ex.Message}");
		}
		return null;
	}

	protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
    }
}
