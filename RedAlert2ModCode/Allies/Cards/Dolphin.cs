using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 海豚 - 盟军海军单位卡
/// 1费，对所有敌人造成2伤害1层易伤，升级后2层易伤
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Dolphin : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Dolphin;

	public Dolphin() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AllEnemies) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/dolphin.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new RepeatVar(Values.Repeat),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Navy.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

		// 对所有敌人造成伤害并施加易伤
		foreach (var enemy in Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
		{
			// 造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, play)
				.Targeting(enemy)
				.Execute(ctx);
			
			// 添加易伤
			await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), enemy, DynamicVars.Repeat.IntValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
	}
}
