using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 轨道毒气 - 轨道战备运转卡
/// 1费，Uncommon卡
/// 效果：回合开始对全体敌人赋予中毒层数。
/// AOE模式：不需要目标选择
/// </summary>
public sealed class OrbitalGasStrike : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.OrbitalGasStrike;

    public OrbitalGasStrike() : base((int)Values.Cost, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();
    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Orbital/OrbitalGasStrike_card.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Poison", (int)Values.MagicNumber)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.OrbitalReadiness.CreateHoverTip()!,
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>()
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            // 轨道战备系列：必须要雷达/空指部能力（T2科技），仅作战实验室（T3科技）不可打出
            bool hasRadar = Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(SovietRadarPower).Name) ||
                            Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(RedAlert2ModCode.Allies.Powers.AlliedAirForceCommandPower).Name);
            if (!hasRadar)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[OrbitalGasStrike] 卡牌打出开始");

        var power = await OrbitalGasStrikePower.ApplyOrbitalGasStrike(Owner.Creature, IsUpgraded);

        if (power != null)
        {
            GD.Print($"[OrbitalGasStrike] 成功获得轨道毒气能力 - Poison={power.CurrentPoison}");
        }
        else
        {
            GD.PrintErr("[OrbitalGasStrike] 获得轨道毒气能力失败");
            return;
        }

        GD.Print("[OrbitalGasStrike] 卡牌打出完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Poison"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
    }
}
