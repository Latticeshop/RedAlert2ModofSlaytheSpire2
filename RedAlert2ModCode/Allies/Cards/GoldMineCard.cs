using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
using RedAlert2ModCode.Utils;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 黄金矿 - 运转卡/能力卡
/// 1费，获得黄金矿储备
/// 逻辑：打出时直接增加黄金矿储备（黄金矿柱只负责每回合增加储备）
/// </summary>
public sealed class GoldMineCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GoldMine;
    
    public GoldMineCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

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

        // 检查是否存在黄金矿能力
        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        
        if (goldMinePower != null)
        {
            // 如果存在黄金矿能力，增加储备
            goldMinePower.AddReserve(amount);
            GD.Print($"[GoldMineCard] 存在黄金矿能力，储备已增加，当前储备: {goldMinePower.CurrentReserve}");
        }
        else
        {
            // 创建新的黄金矿能力
            var newPower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
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
    }
}