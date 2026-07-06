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
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 天气控制器 - 盟军建筑卡（高科技T2）
/// 0费能力卡，金卡，需要作战实验室解锁
/// 效果：每过3回合（升级后2回合），将一张"虚无"词条的"闪电风暴"加入手牌
/// </summary>
public sealed class WeatherController : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.WeatherController;

    public WeatherController() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/wethicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("Interval", Values.Repeat),           // 基础间隔回合
        new IntVar("IntervalUpgraded", Values.RepeatUpgraded)  // 升级后间隔回合
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.AlliedSuperWeapon.CreateHoverTip(),
		HoverTipFactory.FromPower<WeatherControllerPower>(),
		HoverTipFactory.FromCard<LightningStorm>()
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

            // 检查是否拥有作战实验室能力
            if (!Owner.Creature.Powers.Any(p => p is BattleLabPower))
                return false;

            // 检查资金是否足够
            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[WeatherController] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        // 扣除资金
        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[WeatherController] 扣除资金 {Values.DollarValue}");
        }

        // 获得天气控制器能力
        var weatherControllerPower = await PowerCmd.Apply<WeatherControllerPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
        
        // 设置升级状态
        if (weatherControllerPower != null)
        {
            weatherControllerPower.IsUpgraded = IsUpgraded;
            GD.Print($"[WeatherController] 已获得天气控制器能力，升级状态: {weatherControllerPower.IsUpgraded}");
        }

        // 打出后抽一张牌
        await CardPileCmd.Draw(ctx, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级效果：间隔回合从3变为2
    }
}