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
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 神盾巡洋舰 - 盟军海军单位技能卡
/// 1费技能卡，获得8点格挡（升级12点）。若敌人意图攻击，多获得1轮。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Agisicon : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Agisicon;

	public Agisicon() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/agisicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Navy.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		GD.Print($"[Agisicon] OnPlay 被调用 - IsUpgraded={IsUpgraded}");

		// 检查是否有敌人意图攻击
		bool hasAttackIntent = false;
		foreach (var enemy in Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
		{
			if (enemy.Monster?.NextMove?.Intents != null)
			{
				foreach (var intent in enemy.Monster.NextMove.Intents)
				{
					if (intent is AttackIntent)
					{
						hasAttackIntent = true;
						GD.Print($"[Agisicon] 发现敌人有攻击意图");
						break;
					}
				}
				if (hasAttackIntent)
					break;
			}
		}

		// 获得格挡
			int blockAmount = (int)DynamicVars.Block.BaseValue;
			GD.Print($"[Agisicon] 获得 {blockAmount} 点格挡");
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

		// 如果敌人意图攻击，额外获得格挡
		if (hasAttackIntent)
		{
			GD.Print($"[Agisicon] 敌人意图攻击，额外获得格挡");
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}