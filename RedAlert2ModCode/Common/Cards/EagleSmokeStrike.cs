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
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 飞鹰烟雾 - 绝地战备攻击牌
/// 1费，Uncommon蓝卡
/// 效果：赋予目标锁定。下回合对目标敌人施加虚弱，我方全体获得格挡。
/// </summary>
public sealed class EagleSmokeStrike : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.EagleSmokeStrike;

    public EagleSmokeStrike() : base((int)Values.Cost, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/Eagle_Smoke_Strike_card.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MagicNumber", (int)Values.MagicNumber),
        new BlockVar(Values.Block, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.DesperateMeasure.CreateHoverTip()!,
        ModCardKeywords.TargetLocked.CreateHoverTip()!
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[EagleSmokeStrike] 卡牌打出开始");

        var power = await EagleSmokeStrikePower.ApplyEagleSmokeStrike(Owner.Creature, IsUpgraded);

        if (power != null)
        {
            GD.Print($"[EagleSmokeStrike] 成功获得飞鹰烟雾能力 - Weak={power.CurrentWeak}, Block={power.CurrentBlock}");

            // 存储卡牌打出时的目标到能力 + 赋予目标锁定（指向性卡牌）
            if (play.Target != null && play.Target.IsAlive)
            {
                power.StoredTarget = play.Target;
                GD.Print($"[EagleSmokeStrike] 存储目标到能力: {play.Target.Name}");

                await TargetLockedManager.ApplyTargetLocked(play.Target, Owner.Creature, this);
                GD.Print($"[EagleSmokeStrike] 赋予目标锁定: {play.Target.Name}");
            }
        }
        else
        {
            GD.PrintErr("[EagleSmokeStrike] 获得飞鹰烟雾能力失败");
            return;
        }

        GD.Print("[EagleSmokeStrike] 卡牌打出完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MagicNumber"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
        DynamicVars["Block"].UpgradeValueBy(Values.BlockUpgraded);
    }
}