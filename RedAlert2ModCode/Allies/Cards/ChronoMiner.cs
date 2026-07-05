using Godot;
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
using System.Linq;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 超时空矿车 - 技能牌
/// 0费，获得500资金（升级后1000），使用后加入摸牌堆
/// 挖矿逻辑：优先挖宝石矿(2倍价值)，再挖黄金矿
/// </summary>
public sealed class ChronoMiner : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoMiner;
	
	public ChronoMiner() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/ahrvicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Chrono.CreateHoverTip()
	];

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarValue", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType());
		// 检查是否有MCV能力获取资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			int amount = base.DynamicVars["DollarValue"].IntValue;
			GD.Print($"[ChronoMiner] 基础资金 {amount}");

			// 挖矿逻辑：优先挖宝石矿(2倍价值)，再挖黄金矿
			int miningBonus = MineResources();
			GD.Print($"[ChronoMiner] 挖矿额外获得 {miningBonus} 资金");

			// 计算总资金
			int totalAmount = amount + miningBonus;

			// 检查是否有提前倒矿debuff（本回合矿车收益为80%）
			var earlyMiningPower = Owner.Creature.Powers.OfType<EarlyMiningPower>().FirstOrDefault();
			if (earlyMiningPower != null)
			{
				float multiplier = earlyMiningPower.GetMiningMultiplier();
				totalAmount = Mathf.FloorToInt(totalAmount * multiplier);
				GD.Print($"[ChronoMiner] 检测到提前倒矿debuff，总资金 * {multiplier} = {totalAmount}");
			}

			dollarPower.AddDollar(totalAmount);
			GD.Print($"[ChronoMiner] 总共获得 {totalAmount} 资金");
		}

		// 将此牌加入摸牌堆（而不是弃牌堆）
		await CardPileCmd.Add(play.Card, PileType.Draw);
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
			GD.Print($"[ChronoMiner] 挖宝石矿 {gemToMine}，获得 {gemToMine * 2} 资金，剩余待挖 {remainingToMine}");
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
				GD.Print($"[ChronoMiner] 挖黄金矿 {goldToMine}，获得 {goldToMine} 资金，剩余待挖 {remainingToMine}");
			}
		}

		return totalBonus;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["DollarValue"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
	}
}
