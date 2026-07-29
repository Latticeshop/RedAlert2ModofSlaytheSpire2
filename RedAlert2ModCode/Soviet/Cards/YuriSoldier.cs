using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 尤里新兵 - 尤里士兵单位，通过心灵能力灼烧敌人
/// 1费，攻击卡，Token类型，T1科技
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class YuriSoldier : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.YuriSoldier;

    public YuriSoldier() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/yuri/initicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("BurnAmount", Values.Damage)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT1.CreateHoverTip(),
        ModCardKeywords.Soldier.CreateHoverTip(),
        ModCardKeywords.Burn.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 播放攻击音效+随机语音
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
        UnitVoiceHelper.PlayUnitVoice("YuriSoldierAttack", "Soviet");

        // 播放火焰攻击特效
        VfxCmd.PlayOnCreatureCenter(play.Target, "vfx/vfx_fire_burst");

        // 赋予敌人灼烧层数
        int burnAmount = DynamicVars["BurnAmount"].IntValue;
        await PowerCmd.Apply<BurnPower>(ctx, play.Target, burnAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnAmount"].UpgradeValueBy(Values.DamageUpgraded);
    }
}
