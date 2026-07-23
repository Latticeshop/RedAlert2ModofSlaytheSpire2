using System.Collections.Generic;
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
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 武装采矿车 - 攻击牌
/// 0费，对敌方造成2点伤害（升级后对全体敌人），获得1000资金（升级后1500）
/// 挖矿逻辑：优先挖宝石矿(2倍价值)，再挖黄金矿
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class WarMiner : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = SovietCardValues.WarMiner;
	
	public WarMiner() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

	public override TargetType TargetType
	{
		get
		{
			if (IsUpgraded)
			{
				return TargetType.AllEnemies;
			}
			return TargetType.AnyEnemy;
		}
	}

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/harvicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new IntVar("DollarValue", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		UnitVoiceHelper.PlayUnitVoice("WarMinerAttack", "Soviet");
		
		// 攻击效果：造成2点伤害（升级后对全体敌人）
		if (base.IsUpgraded)
		{
			// 升级后：对所有敌人造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, play)
				.TargetingAllOpponents(Owner.Creature.CombatState)
				.Execute(ctx);
			GD.Print($"[WarMiner] 升级：对所有敌人造成 {DynamicVars.Damage.BaseValue} 点伤害");
		}
		else
		{
			// 基础：对选中的敌人造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, play)
				.Targeting(play.Target)
				.Execute(ctx);
			GD.Print($"[WarMiner] 攻击 {play.Target} 造成 {DynamicVars.Damage.BaseValue} 点伤害");
		}
		
		// 检查是否有MCV能力获取资金
		var dollarPower = Owner.Creature.Powers.OfType<RedAlert2ModCode.Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			int amount = base.DynamicVars["DollarValue"].IntValue;
			GD.Print($"[WarMiner] 基础资金 {amount}");

			// 挖矿逻辑：优先挖宝石矿(2倍价值)，再挖黄金矿
			int miningBonus = MineResources();
			GD.Print($"[WarMiner] 挖矿额外获得 {miningBonus} 资金");

			// 计算总资金（矿车基础 + 挖矿收益）
			int totalAmount = amount + miningBonus;

			// 检查矿石精炼器加成（对总收益生效）
			var oreRefineryPower = Owner.Creature.Powers.OfType<Allies.Powers.OreRefineryPower>().FirstOrDefault();
			if (oreRefineryPower != null && totalAmount > 0)
			{
				float oreMultiplier = oreRefineryPower.GetOreMultiplier();
				totalAmount = Mathf.FloorToInt(totalAmount * oreMultiplier);
				GD.Print($"[WarMiner] 矿石精炼器加成 {oreMultiplier}，总资金从 {amount + miningBonus} 变为 {totalAmount}");
			}

			// 检查是否有提前倒矿debuff（本回合矿车收益为80%）
			var earlyMiningPower = Owner.Creature.Powers.OfType<EarlyMiningPower>().FirstOrDefault();
			var sovietEarlyMiningPower = Owner.Creature.Powers.OfType<SovietEarlyMiningPower>().FirstOrDefault();
			
			if (earlyMiningPower != null)
			{
				float multiplier = earlyMiningPower.GetMiningMultiplier();
				totalAmount = Mathf.FloorToInt(totalAmount * multiplier);
				GD.Print($"[WarMiner] 检测到提前倒矿debuff，总资金 * {multiplier} = {totalAmount}");
			}
			else if (sovietEarlyMiningPower != null)
			{
				float multiplier = sovietEarlyMiningPower.GetMiningMultiplier();
				totalAmount = Mathf.FloorToInt(totalAmount * multiplier);
				GD.Print($"[WarMiner] 检测到苏联提前倒矿debuff，总资金 * {multiplier} = {totalAmount}");
			}

			dollarPower.AddDollar(totalAmount);
			GD.Print($"[WarMiner] 总共获得 {totalAmount} 资金");
		}
	}

	/// <summary>
	/// 挖矿逻辑：优先挖宝石矿(2倍价值)，再挖黄金矿
	/// 每打出一张矿车，最多额外获得1000资金（通过挖矿）
	/// </summary>
	/// <returns>挖矿获得的额外资金</returns>
	private int MineResources()
	{
		int totalBonus = 0;
		int remainingToMine = 1000; // 每辆矿车最多额外挖1000价值的矿

		// 1. 优先挖宝石矿（2倍价值）
		var gemMinePower = Owner.Creature.Powers.OfType<GemMinePower>().FirstOrDefault();
		if (gemMinePower != null && gemMinePower.CurrentReserve > 0 && remainingToMine > 0)
		{
			// 计算能挖多少宝石矿（宝石矿价值是普通矿的2倍）
			int gemToMine = Mathf.Min(gemMinePower.CurrentReserve, remainingToMine);
			gemMinePower.SpendReserve(gemToMine);
			totalBonus += gemToMine * 2; // 宝石矿2倍价值
			remainingToMine -= gemToMine;
			GD.Print($"[WarMiner] 挖宝石矿 {gemToMine}，获得 {gemToMine * 2} 资金，剩余待挖 {remainingToMine}");
		}

		// 2. 挖黄金矿（黄金矿柱不管理储备，只负责每回合增加储备）
		if (remainingToMine > 0)
		{
			var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
			if (goldMinePower != null && goldMinePower.CurrentReserve > 0)
			{
				int goldToMine = Mathf.Min(goldMinePower.CurrentReserve, remainingToMine);
				goldMinePower.SpendReserve(goldToMine);
				totalBonus += goldToMine;
				remainingToMine -= goldToMine;
				GD.Print($"[WarMiner] 挖黄金矿 {goldToMine}，获得 {goldToMine} 资金，剩余待挖 {remainingToMine}");
			}
		}

		return totalBonus;
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
		base.DynamicVars["DollarValue"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
	}
}