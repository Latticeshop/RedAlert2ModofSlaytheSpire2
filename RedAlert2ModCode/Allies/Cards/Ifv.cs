using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// IFV - 技能牌
/// 1费，抽1张牌（升级2张），获得5点护盾
/// </summary>
public sealed class Ifv : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Ifv;
	
	public Ifv() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/fvicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Unpowered),
		new IntVar("DrawCount", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		
		await CardPileCmd.Draw(ctx, (int)DynamicVars["DrawCount"].BaseValue, Owner);
		
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["DrawCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}
