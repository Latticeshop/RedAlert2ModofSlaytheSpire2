#nullable enable

using System.Collections.Generic;
using System.Linq;
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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 作战实验室 - 盟军建筑卡
/// 0费能力卡，解锁高级兵种，价格2000
/// </summary>
public sealed class BattleLab : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.BattleLab;

    public BattleLab() : base((int)Values.Cost, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/techicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.BattleLab.CreateHoverTip()
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

            // 检查资金是否足够
            var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[BattleLab] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        // 扣除资金
        var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[BattleLab] 扣除资金 {Values.DollarValue}");
        }

        // 获得作战实验室能力
        await PowerCmd.Apply<BattleLabPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
        
        GD.Print("[BattleLab] 已获得作战实验室能力");

        // 打出后抽一张牌（与其他建筑卡保持一致）
        await CardPileCmd.Draw(ctx, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        // 作战实验室不需要升级效果
    }
}
