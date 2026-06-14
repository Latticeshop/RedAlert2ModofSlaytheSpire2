using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 灰熊坦克 - 类似于铁斩波的攻击牌
/// 1费5攻击5防御
/// </summary>
public sealed class GrizzlyTank : CardModel
{
	public GrizzlyTank() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/grizzly_tank.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(5m, ValueProp.Move),
		new BlockVar(5m, ValueProp.Move)
	};

	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(play.Target)
			.Execute(ctx);
		
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
		DynamicVars.Block.UpgradeValueBy(3m);
	}
}