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

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 坚固围墙 - 古老牙齿转化后的先古版本围墙
/// 盟军建筑，技能卡，先古卡
/// 使用普通围墙一样的图片
/// 效果：花费资金，获得3护盾（升级后5护盾），将此牌返回手牌
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
[RegisterCard(typeof(AlliesCardPool))]
public sealed class FortifiedWall : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.AlliedFortifiedWall;

    public FortifiedWall() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/wallicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new BlockVar(Values.Block, ValueProp.Move),
        new IntVar("DollarNumber", Values.DollarValue)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = new List<CardKeyword>();
            keywords.Add(CardKeyword.Retain);
            return keywords;
        }
    }

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
        }

        // 获得护盾（坚固围墙格挡更高）
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    /// <summary>
    /// 设置卡牌使用后的去向（返回手牌）- Beta版API
    /// </summary>
    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation result = base.GetResultLocationForCardPlay();
        if (result.pileType != PileType.Discard)
        {
            return result;
        }
        result.pileType = PileType.Hand;
        return result;
    }

    protected override void OnUpgrade()
    {
        // 升级后护盾提升到5
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}