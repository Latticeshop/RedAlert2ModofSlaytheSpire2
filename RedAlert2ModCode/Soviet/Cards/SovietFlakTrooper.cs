using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 防空步兵 - 苏联士兵单位
/// 1费攻击卡，每有一个攻击意图的敌人，获得一遍格挡（每个敌人3格挡，升级后4格挡）
/// </summary>
public sealed class SovietFlakTrooper : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakTrooper;

	public SovietFlakTrooper() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/flkticon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Soldier.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		
		// 计算攻击意图的敌人数量
		int attackIntentCount = 0;
		foreach (var enemy in Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
		{
			if (enemy.Monster?.NextMove?.Intents != null)
			{
				foreach (var intent in enemy.Monster.NextMove.Intents)
				{
					if (intent is AttackIntent)
					{
						attackIntentCount++;
						break;
					}
				}
			}
		}

		// 每有一个攻击意图敌人，获得一遍格挡
		for (int i = 0; i < attackIntentCount; i++)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}