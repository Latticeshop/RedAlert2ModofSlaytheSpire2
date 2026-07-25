using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
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
using RedAlert2ModCode.Common.Powers;
namespace RedAlert2ModCode.Common.Cards;

public class GoldMineColumnCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMineColumn;
    
    public GoldMineColumnCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

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

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine_column.png";

            protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.GoldMine.CreateHoverTip(),
		HoverTipFactory.FromPower<GoldMineColumnPower>()
	];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue),
        new IntVar("PerTurn", Values.Stars)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int amount = base.DynamicVars["Reserve"].IntValue;
        GD.Print($"[GoldMineColumnCard] 打出黄金矿柱，获得储备 {amount}");

        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(amount);
            GD.Print($"[GoldMineColumnCard] 黄金矿储备增加 {amount}，当前储备: {goldMinePower.CurrentReserve}");
        }
        else
        {
            var newGoldMinePower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newGoldMinePower != null)
            {
                newGoldMinePower.CurrentReserve = amount;
                newGoldMinePower.IsUpgraded = IsUpgraded;
                GD.Print($"[GoldMineColumnCard] 创建新黄金矿能力，初始储备: {newGoldMinePower.CurrentReserve}");
            }
        }

        var goldMineColumnPower = Owner.Creature.Powers.OfType<GoldMineColumnPower>().FirstOrDefault();
        
        if (goldMineColumnPower != null)
        {
            goldMineColumnPower.IncreasePerTurn();
            GD.Print($"[GoldMineColumnCard] 存在黄金矿柱能力，每回合产量已增加，当前每回合产量: {goldMineColumnPower.ReservePerTurn}");
        }
        else
        {
            var newPower = await PowerCmd.Apply<GoldMineColumnPower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newPower != null)
            {
                GD.Print($"[GoldMineColumnCard] 创建新黄金矿柱能力，每回合产量: {newPower.ReservePerTurn}");
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Reserve"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
        AddKeyword(CardKeyword.Innate);
    }
}