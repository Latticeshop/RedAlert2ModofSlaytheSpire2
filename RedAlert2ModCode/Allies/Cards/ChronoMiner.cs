using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using System.Collections.Generic;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 超时空矿车 - 技能牌
/// 1费，获得8点护盾（升级后11点）
/// </summary>
public sealed class ChronoMiner : CardModel
{
	public ChronoMiner() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/ahrvicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(8m, ValueProp.Move)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}
