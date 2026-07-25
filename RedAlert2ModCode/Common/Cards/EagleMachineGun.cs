using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
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
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using Godot;
namespace RedAlert2ModCode.Common.Cards;

public sealed class EagleMachineGun : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.EagleMachineGun;

    public EagleMachineGun() : base((int)Values.Cost, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/EagleMachineGun.png";

            protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage + (IsUpgraded ? Values.DamageUpgraded : 0m), ValueProp.Move),
        new RepeatVar(Values.Repeat)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.DesperateMeasure.CreateHoverTip()!,
		HoverTipFactory.FromPower<TargetLockedPower>()
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[EagleMachineGun] 卡牌打出开始");

        var power = await EagleMachineGunPower.ApplyEagleMachineGun(Owner.Creature, IsUpgraded);
        if (power != null)
        {
            GD.Print($"[EagleMachineGun] 成功获得飞鹰机枪扫射能力 - Damage={power.CurrentDamage}");
        }
        else
        {
            GD.PrintErr("[EagleMachineGun] 获得飞鹰机枪扫射能力失败");
        }

        if (play.Target != null && play.Target.IsAlive)
        {
            await TargetLockedManager.ApplyTargetLocked(play.Target, Owner.Creature, this);
        }
        else
        {
            GD.PrintErr("[EagleMachineGun] 没有有效目标");
        }

        GD.Print("[EagleMachineGun] 卡牌打出完成");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
        GD.Print($"[EagleMachineGun] 卡牌升级 - 伤害增加 {Values.DamageUpgraded}");
    }
}