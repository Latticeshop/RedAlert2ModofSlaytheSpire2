using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 机枪碉堡 - 盟军防御建筑
/// 1费技能卡（普通白卡）
/// 效果：获得能力：每回合开始时对随机敌人造成2点伤害，自己获得5点防御
/// </summary>
public sealed class PillboxCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.Pillbox;
	
	public PillboxCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/pillicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Unpowered),
		new BlockVar(Values.Block, ValueProp.Unpowered)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		GD.Print($"[PillboxCard] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

		// 应用机枪碉堡能力
		await PillboxPower.ApplyPillbox(Owner.Creature, base.IsUpgraded);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}