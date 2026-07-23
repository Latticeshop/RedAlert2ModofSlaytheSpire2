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
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 驱逐舰 - 盟军海军单位攻击卡
/// 1费攻击卡，造成8点伤害（升级12点）。若敌人意图防御，改为给予1层易伤，造成2点（升级3点）伤害5次。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
[RegisterCard(typeof(AlliesCardPool))]
public sealed class Destroyer : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Destroyer;

	public Destroyer() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/destroyer.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new DamageVar("DefendDamage", Values.MagicNumber, ValueProp.Move),
		new IntVar("RepeatCount", Values.Repeat)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Navy.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		GD.Print($"[Destroyer] OnPlay 被调用 - IsUpgraded={IsUpgraded}");

		// 检查目标敌人是否有防御意图
		bool hasDefendIntent = false;
		if (play.Target?.Monster?.NextMove?.Intents != null)
		{
			foreach (var intent in play.Target.Monster.NextMove.Intents)
			{
				if (intent is DefendIntent)
				{
					hasDefendIntent = true;
					GD.Print($"[Destroyer] 目标敌人有防御意图");
					break;
				}
			}
		}

		if (hasDefendIntent)
		{
			// 敌人意图防御：给予1层易伤，造成2点（升级3点）伤害5次
			GD.Print($"[Destroyer] 执行防御意图效果：给予易伤，多次伤害");

			// 给予1层易伤
			await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, 1m, Owner.Creature, this);

			// 造成多次伤害
			int defendDamage = (int)DynamicVars["DefendDamage"].BaseValue;
			int repeatCount = Values.Repeat;

			for (int i = 0; i < repeatCount; i++)
			{
				await DamageCmd.Attack(defendDamage)
					.FromCard(this, play)
					.Targeting(play.Target)
					.Execute(ctx);
				GD.Print($"[Destroyer] 第 {i + 1} 次伤害：{defendDamage}");
			}
		}
		else
		{
			// 正常情况：造成8点（升级12点）伤害
			GD.Print($"[Destroyer] 执行正常伤害：{DynamicVars.Damage.BaseValue}");
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, play)
				.Targeting(play.Target)
				.Execute(ctx);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars["DefendDamage"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}