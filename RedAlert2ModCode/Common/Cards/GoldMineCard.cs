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

public class GoldMineCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMine;
    
    public GoldMineCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

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

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine.png";

            protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.GoldMine.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int amount = base.DynamicVars["Reserve"].IntValue;
        GD.Print($"[GoldMineCard] 打出黄金矿，获得储备 {amount}");

        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(amount);
            GD.Print($"[GoldMineCard] 存在黄金矿能力，储备已增加，当前储备: {goldMinePower.CurrentReserve}");
        }
        else
        {
            var newPower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, amount, Owner.Creature, null);
            if (newPower != null)
            {
                newPower.CurrentReserve = amount;
                newPower.IsUpgraded = IsUpgraded;
                GD.Print($"[GoldMineCard] 创建新黄金矿能力，初始储备: {newPower.CurrentReserve}");
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Reserve"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
        AddKeyword(CardKeyword.Innate);
    }
}
