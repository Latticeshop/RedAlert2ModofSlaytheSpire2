using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Yuri;
namespace RedAlert2ModCode.Common.Cards;

public sealed class SupportCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.Support;

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public SupportCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly) { }

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
    public override CardPoolModel VisualCardPool => Pool;

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/Ra2_Support.png";

        	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Unit.CreateHoverTip()
	];

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		AudioHelper.PlaySupportCheer();

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		int selectCount = IsUpgraded
			? (int)Values.MagicNumber + (int)Values.MagicNumberUpgraded
			: (int)Values.MagicNumber;

		HashSet<Type> unitTypes = CardUtils.GetUnitTypes();

		GD.Print($"[SupportCard] 选择 {selectCount} 张单位卡送给队友");

		// 原版 FromHand 会触发 CancelAllCardPlay（取消回手流程），联机中会阻塞其他玩家出牌；
		// 改用与超时空传送一致的 ExecuteSyncChoice + mod 选择 UI。
		var unitCards = PileType.Hand.GetPile(Owner).Cards.Where(c => unitTypes.Contains(c.GetType())).ToList();
		List<CardModel> cardsToGive = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(
			choiceContext, unitCards, selectCount, 0, Owner)
			?? new List<CardModel>();

		GD.Print($"[SupportCard] 选中了 {cardsToGive.Count} 张单位卡");

		Player targetPlayer = cardPlay.Target.Player;

		foreach (CardModel card in cardsToGive)
		{
			await GiveCardToAnotherPlayer(card, targetPlayer, PileType.Hand, CardPilePosition.Random);
			GD.Print($"[SupportCard] 将 {card.Id.Entry} 送给队友");
		}
	}

	private static async Task GiveCardToAnotherPlayer(CardModel card, Player targetPlayer, PileType pileType, CardPilePosition position)
	{
		await CardPileCmd.RemoveFromCombat(card);
		
		card.HasBeenRemovedFromState = false;
		
		card.GiveToAnotherPlayer(targetPlayer);
		
		await CardPileCmd.Add(new[] { card }, pileType.GetPile(targetPlayer), position, null, skipVisuals: false, isChangingOwners: true);
	}
}
