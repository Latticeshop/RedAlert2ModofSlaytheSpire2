using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
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
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class ApocalypseTank : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.ApocalypseTank;
	
	public ApocalypseTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/mtnkicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("VulnerableStacks", Values.MagicNumber),
		new IntVar("Repeat", Values.Repeat)
	};

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		UnitVoiceHelper.PlayUnitVoice("ApocalypseTankAttack", "Soviet");
		
		int repeat = Values.Repeat;
		for (int i = 0; i < repeat; i++)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, play)
				.Targeting(play.Target)
				.Execute(ctx);
		}
		
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
		
		int vulnerableStacks = Values.MagicNumber + (IsUpgraded ? Values.MagicNumberUpgraded : 0);
		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, vulnerableStacks, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
		DynamicVars["VulnerableStacks"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}