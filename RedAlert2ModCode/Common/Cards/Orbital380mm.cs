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
/// 轨道380MM - 轨道战备运转卡
/// 1费，Rare卡
/// 效果：赋予目标锁定。下回合对目标敌人造成伤害3次，溅射。
/// </summary>
public sealed class Orbital380mm : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.Orbital380mm;

    public Orbital380mm() : base((int)Values.Cost, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();
    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Orbital/Orbital380mm_card.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Damage", (int)Values.Damage),
        new IntVar("Repeat", (int)Values.Repeat)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.OrbitalReadiness.CreateHoverTip()!,
        ModCardKeywords.TargetLocked.CreateHoverTip()!,
        ModCardKeywords.Splash.CreateHoverTip()!
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
        GD.Print("[Orbital380mm] 卡牌打出开始");

        var power = await Orbital380mmPower.ApplyOrbital380mm(Owner.Creature, IsUpgraded);

        if (power != null)
        {
            GD.Print($"[Orbital380mm] 成功获得轨道380MM能力 - Damage={power.CurrentDamage}x{power.CurrentRepeat}");

            // 赋予目标锁定（轨道战备流程：下回合只对目标锁定敌人攻击）
            if (play.Target != null && play.Target.IsAlive)
            {
                await TargetLockedManager.ApplyTargetLocked(play.Target, Owner.Creature, this);
                GD.Print($"[Orbital380mm] 赋予目标锁定: {play.Target.Name}");
            }
        }
        else
        {
            GD.PrintErr("[Orbital380mm] 获得轨道380MM能力失败");
            return;
        }

        GD.Print("[Orbital380mm] 卡牌打出完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy((int)Values.DamageUpgraded);
    }
}