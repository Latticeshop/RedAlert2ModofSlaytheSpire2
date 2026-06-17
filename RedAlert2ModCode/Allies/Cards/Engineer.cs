using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 工程师 - 技能牌
/// 0费，获得2点覆甲（升级后3点）
/// </summary>
public sealed class Engineer : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Engineer;
	
	public Engineer() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/aengicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("PlatingAmount", Values.Block),
		new PowerVar<PlatingPower>(Values.Block)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Soldier.CreateHoverTip(),
		HoverTipFactory.FromPower<PlatingPower>()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 获得覆甲能力
		await PowerCmd.Apply<PlatingPower>(Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["PlatingPower"].UpgradeValueBy(Values.BlockUpgraded);
		DynamicVars["PlatingAmount"].UpgradeValueBy(Values.BlockUpgraded);
	}
}
