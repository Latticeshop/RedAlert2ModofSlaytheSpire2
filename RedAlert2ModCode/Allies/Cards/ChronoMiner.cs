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
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 超时空矿车 - 技能牌
/// 0费，获得500资金（升级后800），使用后加入摸牌堆
/// </summary>
public sealed class ChronoMiner : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoMiner;
	
	public ChronoMiner() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/ahrvicon.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarValue", Values.DollarValue)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 获取资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			int amount = base.DynamicVars["DollarValue"].IntValue;
			dollarPower.AddDollar(amount);
			GD.Print($"[ChronoMiner] 获得 {amount} 资金");
		}

		// 将此牌加入摸牌堆（而不是弃牌堆）
		await CardPileCmd.Add(play.Card, PileType.Draw);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["DollarValue"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
	}
}
