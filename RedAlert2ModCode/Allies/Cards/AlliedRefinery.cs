using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军矿场 - 能力牌
/// 1费，将一张超时空矿车加入手牌
/// </summary>
public sealed class AlliedRefinery : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.AlliedRefinery;
	
	public AlliedRefinery() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/reficon.png";
	
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < AlliesCardValues.AlliedRefinery.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)AlliesCardValues.AlliedRefinery.DollarValue);
			GD.Print($"[AlliedRefinery] 扣除资金 {AlliesCardValues.AlliedRefinery.DollarValue}");
		}

		// 检查 Owner 和相关对象
		if (Owner == null)
		{
			GD.Print("[AlliedRefinery] Error: Owner is null");
			return;
		}
		
		if (Owner.Creature == null)
		{
			GD.Print("[AlliedRefinery] Error: Owner.Creature is null");
			return;
		}
		
		if (Owner.Creature.CombatState == null)
		{
			GD.Print("[AlliedRefinery] Error: Owner.Creature.CombatState is null");
			return;
		}
		
		// 获取 ChronoMiner 模型
		var minerModel = ModelDb.Card<ChronoMiner>();
		if (minerModel == null)
		{
			GD.Print("[AlliedRefinery] Error: ChronoMiner model is null");
			return;
		}

		// 创建超时空矿车卡牌
		var minerCard = Owner.Creature.CombatState.CreateCard(minerModel, Owner);
		// 如果矿场是升级过的，矿车也升级
		if (base.IsUpgraded)
		{
			CardCmd.Upgrade(minerCard);
		}
		
		// 将矿车加入手牌
		await CardPileCmd.AddGeneratedCardToCombat(minerCard, PileType.Hand, Owner);

		// 打出后抽一张牌
		await CardPileCmd.Draw(ctx, 1, Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级后：获得的超时空矿车也会升级
	}
}