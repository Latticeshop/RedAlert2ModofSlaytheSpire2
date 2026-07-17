using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Utils;
using Godot;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class Sniper : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Sniper;
	
	public Sniper() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/snipicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		UnitVoiceHelper.PlayUnitVoice("SniperAttack", "Allied");
		GD.Print("[Sniper] 卡牌打出开始");

		await CreatureCmd.Damage(ctx, play.Target, DynamicVars.Damage.BaseValue,
			ValueProp.Move | ValueProp.Unblockable, Owner.Creature, this);

		GD.Print("[Sniper] 卡牌打出完成");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
	}
}