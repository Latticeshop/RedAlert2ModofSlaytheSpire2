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
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军围墙 - 防御卡
/// 0费，获得护甲并返回手牌
/// </summary>
public sealed class SovietWallCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.SovietWall;
	
	public SovietWallCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwalicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new BlockVar(Values.Block, ValueProp.Move)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 不播放建筑音效，因为这是围墙
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