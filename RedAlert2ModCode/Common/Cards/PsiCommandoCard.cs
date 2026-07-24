using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Common.Cards;

[RegisterCard(typeof(RedAlert2ModCode.Allies.AlliesCardPool))]
[RegisterCard(typeof(RedAlert2ModCode.Soviet.SovietCardPool))]
public sealed class PsiCommandoCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.PsiCommando;

	public PsiCommandoCard() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/other/Psi_Commando.png";

	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[0];

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("Damage", Values.MagicNumber),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Infiltrator.CreateHoverTip(),
		ModCardKeywords.Soldier.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		if (play.Target is not Creature target) return;

		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.CastAnimDelay);

		bool isAttackIntent = IsAttackIntent(target);

		if (isAttackIntent)
		{
			UnitVoiceHelper.PlayUnitVoice("YuriAttack", "Yuri");
			UnitVoiceHelper.PlayUnitVoice("Yuri", "Yuri");

			CardModel? unitCard = await RandomUnitHelper.CreateRandomUnitCard(Owner, IsUpgraded, true);
			if (unitCard != null)
			{
				GD.Print($"[PsiCommandoCard] 攻击意图触发，获得随机单位卡牌: {unitCard.GetType().Name}");
			}
		}
		else
		{
			UnitVoiceHelper.PlayUnitVoice("Yuri", "Yuri");
			AudioHelper.PlayRandomExplosionSound();

			await DamageCmd.Attack(DynamicVars["Damage"].IntValue)
				.FromCard(this, play)
				.Targeting(target)
				.Execute(ctx);
			GD.Print($"[PsiCommandoCard] 非攻击意图触发，造成 {DynamicVars["Damage"].IntValue} 点伤害");
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Damage"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}

	private bool IsAttackIntent(Creature target)
	{
		if (target.Monster?.NextMove?.Intents != null)
		{
			return target.Monster.NextMove.Intents.Any(intent => intent is AttackIntent);
		}
		return false;
	}
}
