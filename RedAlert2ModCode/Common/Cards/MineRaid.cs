using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
namespace RedAlert2ModCode.Common.Cards;

public class MineRaid : CardModel
{
	private static readonly CardValueStore.CardValues Values = CommonCardValues.MineRaid;

	public MineRaid() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

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

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/mine_raid.png";

        	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Miner.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		HoverTipFactory.FromCard<RedAlert2ModCode.Soviet.Cards.WarMiner>()
	];

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		int stacks = IsUpgraded
			? (int)CommonPowerValues.MineRaidPower.Stars + (int)CommonPowerValues.MineRaidPower.StarsUpgraded
			: (int)CommonPowerValues.MineRaidPower.Stars;

		await PowerCmd.Apply<MineRaidPower>(ctx, Owner.Creature, stacks, Owner.Creature, play.Card);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}