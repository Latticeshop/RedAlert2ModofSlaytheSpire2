using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军基地车 - 苏军建筑卡
/// 0费，稀有卡
/// 展开：从当前建筑中选择一张加入手牌
/// </summary>
public sealed class SovietMCV : CardModel
{
	public SovietMCV() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";

	/// <summary>
	/// 固有词条 - 每场战斗开始时自动出现在手牌
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Innate };

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		// 获取可建造的建筑卡牌列表
		var buildingCards = SovietCardRegistry.CreateBuildingCards(Owner);
		
		if (buildingCards.Count > 0)
		{
			// 显示建筑选择界面
			var selectedCard = await CardSelectionScreen.ShowMultiSelection(buildingCards, 1, 1);
			
			if (selectedCard != null && selectedCard.Any())
			{
				await CardPileCmd.AddGeneratedCardToCombat(selectedCard[0], PileType.Hand, Owner);
			}
		}
	}
}