using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Combat;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using Godot;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 飞鹰空袭 - 绝地战备攻击牌
/// 1费，Uncommon蓝卡
/// 效果：获得飞鹰空袭能力
/// 描述：[gold]绝地战备[/gold]。\n对全部敌人造成8点(升级12点)伤害。
/// </summary>
public sealed class EagleAirStrike : CardModel
{
    // 数值引用
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.EagleAirStrike;

    public EagleAirStrike() : base((int)Values.Cost, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/EagleAirStrike.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage + (IsUpgraded ? Values.DamageUpgraded : 0m), ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.DesperateMeasure.CreateHoverTip()!
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[EagleAirStrike] 卡牌打出开始");

        // 获得飞鹰空袭能力
        var power = await EagleAirStrikePower.ApplyEagleAirStrike(Owner.Creature, IsUpgraded);
        if (power != null)
        {
            GD.Print($"[EagleAirStrike] 成功获得飞鹰空袭能力 - Damage={power.CurrentDamage}");
        }
        else
        {
            GD.PrintErr("[EagleAirStrike] 获得飞鹰空袭能力失败");
        }

        GD.Print("[EagleAirStrike] 卡牌打出完成");
    }

    protected override void OnUpgrade()
    {
        // 升级后伤害增加
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
        GD.Print($"[EagleAirStrike] 卡牌升级 - 伤害增加 {Values.DamageUpgraded}");
    }
}
