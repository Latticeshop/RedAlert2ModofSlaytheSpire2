using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;
using System.Linq;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军围墙 - 苏联建筑卡
/// 0费技能卡
/// 效果：花费资金，获得护盾，将此牌返回手牌
/// </summary>
public sealed class SovietWallCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietWall;
	
	public SovietWallCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwalicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Move),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip()
	];

	/// <summary>
	/// 检查是否可以打出（资金是否足够）
	/// </summary>
	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			// 检查资金是否足够
			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[SovietWallCard] 扣除资金 {Values.DollarValue}");
		}
		
		// 获得护盾
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
	}

	/// <summary>
	/// 设置卡牌使用后的去向（返回手牌）
	/// </summary>
	protected override PileType GetResultPileTypeForCardPlay()
	{
		PileType resultPileType = base.GetResultPileTypeForCardPlay();
		if (resultPileType != PileType.Discard)
		{
			return resultPileType;
		}
		return PileType.Hand;
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
	}
}