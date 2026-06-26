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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏联矿场 - 能力牌
/// 1费，将一张武装采矿车加入手牌
/// </summary>
public sealed class SovietRefinery : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietRefinery;
	
	public SovietRefinery() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/nreficon.png";
	
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

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietRefinery] 扣除资金 {Values.DollarValue}");
		}

		// 获取 WarMiner 模型
		var minerModel = ModelDb.Card<WarMiner>();
		if (minerModel == null)
		{
			GD.Print("[SovietRefinery] Error: WarMiner model is null");
			return;
		}

		// 创建武装采矿车卡牌
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
		// 升级后：获得的武装采矿车也会升级
	}
}