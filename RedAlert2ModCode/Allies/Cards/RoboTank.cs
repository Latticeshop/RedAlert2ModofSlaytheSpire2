#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 遥控坦克 - 盟军高科技装甲单位卡（控制中心解锁）
/// 0费攻击卡，Token衍生卡
/// 效果：造成 4(升级7) 点伤害，获得 5(升级8) 点格挡，给予敌人 1 层易伤。
/// 资金价格 600 为重工生产价格（生产序列扣费），打出不扣资金；
/// 打出检查角色能量：当前能量为 0 时无法打出。
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class RoboTank : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.RoboTank;

    public RoboTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/roboicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new BlockVar(Values.Block, ValueProp.Move),
        new IntVar("VulnerableStacks", Values.MagicNumber),
        new EnergyVar(1)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT2.CreateHoverTip(),
        ModCardKeywords.Vehicle.CreateHoverTip(),
        HoverTipFactory.FromPower<EnergyNextTurnPower>()
    ];

    /// <summary>
    /// 检查角色能量费用：当前能量为 0 时禁止打出。
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            // 战斗外 PlayerCombatState 为空时不可打出；战斗中当前能量为 0 时禁止打出
            if (Owner?.PlayerCombatState == null || Owner.PlayerCombatState.Energy <= 0)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 有能量打出：播放随机无后缀语音 + 攻击音效
        UnitVoiceHelper.PlayUnitVoice("RoboTank", "Allied");
        UnitVoiceHelper.PlayUnitVoice("RoboTankAttack", "Allied");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[RoboTank] 目标不是Creature");
            return;
        }

        // 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(target)
            .Execute(ctx);

        // 获得格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        // 给予 1 层易伤
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), target, Values.MagicNumber, Owner.Creature, this);

        GD.Print($"[RoboTank] 造成 {DynamicVars.Damage.BaseValue} 点伤害，获得 {DynamicVars.Block.BaseValue} 点格挡，赋予 {Values.MagicNumber} 层易伤");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}
