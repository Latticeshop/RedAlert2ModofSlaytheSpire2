using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 运输船 - 盟军海军单位卡
/// 1费技能卡，可存储手牌中的卡牌
/// </summary>
public sealed class TransportShip : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.TransportShip;

	public TransportShip() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png";

	/// <summary>
	/// 使用原版"保留"词条
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("StoreCount", IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber),
		new IntVar("DollarNumber", Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Navy.CreateHoverTip()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		GD.Print($"[TransportShip] OnPlay 被调用 - IsUpgraded={IsUpgraded}");

		// 获取或创建运输船能力
		var transportPower = Owner.Creature.Powers.OfType<TransportShipPower>().FirstOrDefault();
		if (transportPower == null)
		{
			GD.Print($"[TransportShip] 创建新的TransportShipPower");
			transportPower = await PowerCmd.Apply<TransportShipPower>(Owner.Creature, 1m, Owner.Creature, this);
		}

		// 检查是否有存储的卡牌
		if (transportPower.HasStoredCards)
		{
			GD.Print($"[TransportShip] 释放存储的卡牌，数量: {transportPower.StoredCount}");
			
			// 释放所有存储的卡牌到手牌
			await transportPower.ReleaseCards();
		}
		else
		{
			GD.Print($"[TransportShip] 准备存储卡牌");
			
			// 获取可存储的最大数量（升级后5张，基础3张）
			int maxStoreCount = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
			
			// 使用原版手牌选择UI，允许选择0到maxStoreCount张
			var prefs = new CardSelectorPrefs(new LocString("cards", "TRANSPORT_SHIP.select_description"), 0, maxStoreCount);
			var selectedCards = await CardSelectCmd.FromHand(ctx, Owner, prefs, null, this);
			
			if (selectedCards != null && selectedCards.Any())
			{
				var selectedList = selectedCards.ToList();
				GD.Print($"[TransportShip] 玩家选择了 {selectedList.Count} 张卡牌进行存储");
				
				// 存储选中的卡牌
				await transportPower.StoreCards(selectedList);
			}
			else
			{
				GD.Print($"[TransportShip] 玩家取消选择");
				// 取消选择：返还费用并将卡牌放回手牌
				await CardUtils.HandleCardCancellation(play, this, Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后存储数量从3张增加到5张
	}
}