using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 飞鹰烟雾能力 - 绝地战备
/// 效果：对目标敌人施加虚弱，我方全体获得格挡
/// 目标优先级：StoredTarget → TargetLocked → 随机敌人（由基类统一处理）
/// 战机可通过 IDesperateMeasurePower 接口触发替换攻击
/// </summary>
public sealed class EagleSmokeStrikePower : DesperateMeasurePowerBase
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.EagleSmokeStrikePower;

    /// <summary>
    /// 当前虚弱层数
    /// </summary>
    public int CurrentWeak { get; set; } = (int)Values.MagicNumber;

    /// <summary>
    /// 当前格挡值
    /// </summary>
    public decimal CurrentBlock { get; set; } = Values.Block;

    public override string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/Helldivers/Eagle/Eagle_Smoke_Strike.png";

    /// <summary>
    /// 描述文本 - 使用 MagicNumber(虚弱) 和 Block 变量
    /// </summary>
    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            UpdateDescriptionVars(locString);
            return locString;
        }
    }

    /// <summary>
    /// 更新描述中的动态变量（虚弱与格挡，而非伤害）
    /// </summary>
    protected override void UpdateDescriptionVars(LocString locString)
    {
        int displayWeak = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : CurrentWeak;
        decimal displayBlock = IsUpgraded ? Values.Block + Values.BlockUpgraded : CurrentBlock;
        locString.Add("MagicNumber", displayWeak);
        locString.Add("Block", displayBlock);
    }

    /// <summary>
    /// 应用飞鹰烟雾能力
    /// 独立叠层：相同(虚弱,格挡)值叠加层数，不同则独立存在
    /// </summary>
    public static async Task<EagleSmokeStrikePower?> ApplyEagleSmokeStrike(Creature owner, bool isUpgraded = false)
    {
        int weak = isUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
        decimal block = isUpgraded ? Values.Block + Values.BlockUpgraded : Values.Block;

        var existingPower = owner.Powers.OfType<EagleSmokeStrikePower>()
            .FirstOrDefault(p => p.CurrentWeak == weak && p.CurrentBlock == block);

        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[EagleSmokeStrikePower] 叠加到已存在的烟雾能力，层数: {existingPower.Amount}，Weak: {weak}, Block: {block}");
            return existingPower;
        }

        var power = await PowerCmd.Apply<EagleSmokeStrikePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentWeak = weak;
            power.CurrentBlock = block;
            power.IsUpgraded = isUpgraded;
            GD.Print($"[EagleSmokeStrikePower] 创建成功 - Weak={weak}, Block={block}, IsUpgraded={isUpgraded}");
        }
        return power;
    }

    /// <summary>
    /// 执行攻击效果：对目标施加虚弱，我方全体获得格挡
    /// 仅由 ExecuteDesperateMeasureAttack 调用（战机攻击时触发，不会自动结束）
    /// </summary>
    protected override async Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx)
    {
        // 播放烟雾特效
        VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_coin_explosion_small");
        await Cmd.Wait(0.2f);

        // 对目标施加虚弱
        await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(
            ctx, target, (decimal)CurrentWeak, Owner, null);

        // 我方全体获得格挡
        var allies = CombatState?.PlayerCreatures
            .Where(a => a.Side == CombatSide.Player && a.IsAlive)
            .ToList();

        if (allies != null)
        {
            foreach (var ally in allies)
            {
                await CreatureCmd.GainBlock(ally, CurrentBlock, ValueProp.Unpowered, null);
            }
        }

        GD.Print($"[EagleSmokeStrikePower] 对 {target.Name} 施加 {CurrentWeak} 层虚弱，我方全体获得 {CurrentBlock} 格挡");
    }
}
