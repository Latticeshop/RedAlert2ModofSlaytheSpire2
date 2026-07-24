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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Common.Cards;

[RegisterCard(typeof(RedAlert2ModCode.Allies.AlliesCardPool))]
[RegisterCard(typeof(RedAlert2ModCode.Soviet.SovietCardPool))]
public sealed class YuriPrimeCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.YuriPrime;

	public YuriPrimeCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/other/yurpicon.png";

	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	public override CardPoolModel VisualCardPool => Pool;

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

		List<CardModel> cards = await RandomUnitHelper.CreateRandomUnitCards(Owner, Values.MagicNumber, IsUpgraded, true);
		GD.Print($"[YuriPrimeCard] 生成了 {cards.Count} 张不同的随机单位卡牌");
	}
}
