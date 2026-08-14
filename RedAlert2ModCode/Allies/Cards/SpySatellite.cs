#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 间谍卫星 - 盟军高科技建筑卡（T3，需要盟军作战实验室）
/// 0费能力卡，rare 金卡，价格1500
/// 效果：免疫[gold]虚弱[/gold]和[gold]脆弱[/gold]（获得间谍卫星能力）。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class SpySatellite : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.SpySatellite;

    public SpySatellite() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/asaticon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip(),
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        HoverTipFactory.FromCard<AlliedBattleLab>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>()
    ];

    /// <summary>
    /// 需要 MCV（建造厂）、足够资金、以及盟军作战实验室能力（T3 科技）才可打出。
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            // 检查有“盟军作战实验室”能力
            if (!Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(BattleLabPower).Name))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[SpySatellite] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();
        UnitVoiceHelper.PlaySound("res://RedAlert2ModResources/audio/CommonSFX/vision_gain.wav");

        // 扣除资金
        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-Values.DollarValue);
            GD.Print($"[SpySatellite] 扣除资金 {Values.DollarValue}");
        }

        // 获得间谍卫星能力（升级/未升级独立叠层，叠层无额外效果）
        await SpySatellitePower.ApplySpySatellite(Owner.Creature, IsUpgraded);
        GD.Print($"[SpySatellite] 已获得间谍卫星能力（升级={IsUpgraded}）");
    }

    protected override void OnUpgrade()
    {
        // 间谍卫星无升级数值变化
    }
}
