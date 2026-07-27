using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// IFV - 技能牌
/// 1费，抽2(升级3)张牌，弃0-2(升级3)张牌
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Ifv : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Ifv;
	
	public Ifv() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fvicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DrawCount", Values.MagicNumber),
		new IntVar("DiscardCount", Values.Stars)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		
		// 抽牌
		await CardPileCmd.Draw(ctx, (int)DynamicVars["DrawCount"].BaseValue, Owner);
		
		// 弃牌选择：参考苏联维修厂的原版UI
		int maxDiscard = (int)DynamicVars["DiscardCount"].BaseValue;
		var selectPrompt = new LocString("cards", "RED_ALERT2_MOD_CARD_IFV.select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", maxDiscard);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, maxDiscard)
		{
			RequireManualConfirmation = true
		};

		var selectedCards = (await CardSelectCmd.FromHand(
			ctx,
			Owner,
			prefs,
			c => c != this,
			this
		)).ToList();

		// 弃掉选中的牌
		foreach (var card in selectedCards)
		{
			await CardPileCmd.Add(card, PileType.Discard);
			GD.Print($"[Ifv] 弃牌: {card.Title}");
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["DrawCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
		DynamicVars["DiscardCount"].UpgradeValueBy(Values.StarsUpgraded);
	}
}
