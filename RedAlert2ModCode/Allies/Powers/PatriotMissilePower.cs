using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 爱国者导弹能力 - 盟军防御建筑能力
/// 效果：回合结束时，获得9点格挡（升级后12点）
/// </summary>
public class PatriotMissilePower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.PatriotMissile;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同升级状态的叠加逻辑在 ApplyPatriotMissile 中手动处理
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>
	/// 当前格挡值（每有一个攻击意图敌人获得的格挡）
	/// </summary>
	public int CurrentBlock { get; set; } = (int)Values.Block;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

	public PatriotMissilePower()
	{
		GD.Print($"[PatriotMissilePower] 构造函数被调用 - Block={CurrentBlock}");
	}

	/// <summary>
	/// 使用爱国者导弹卡牌的图标
	/// </summary>
	public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/samicon.png";

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Block", CurrentBlock);
			return locString;
		}
	}

	/// <summary>
	/// 应用爱国者导弹能力
	/// </summary>
	public static async Task ApplyPatriotMissile(Creature owner, bool isUpgraded = false)
	{
		GD.Print($"[PatriotMissilePower] ApplyPatriotMissile 被调用 - IsUpgraded={isUpgraded}");

        // 检查是否已有相同升级状态的爱国者导弹能力
        var existingPower = owner.Powers
            .OfType<PatriotMissilePower>()
            .FirstOrDefault(p => p.IsUpgraded == isUpgraded);

        if (existingPower != null)
        {
            // 已有相同升级状态的能力，增加层数
            GD.Print($"[PatriotMissilePower] 发现相同升级状态的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, null);
            GD.Print($"[PatriotMissilePower] 增加后层数: {existingPower.Amount}");
            return;
        }

		var newPower = await PowerCmd.Apply<PatriotMissilePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.CurrentBlock = (int)Values.Block + (isUpgraded ? (int)Values.BlockUpgraded : 0);
            newPower.IsUpgraded = isUpgraded;
			GD.Print($"[PatriotMissilePower] 创建成功 - Block={newPower.CurrentBlock}, IsUpgraded={newPower.IsUpgraded}");
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player)
			return;

		int stacks = (int)base.Amount;
		GD.Print($"[PatriotMissilePower] 回合结束触发 - 层数={stacks}, Block={CurrentBlock}");

		for (int i = 0; i < stacks; i++)
		{
			if (base.Owner != null)
			{
				UnitVoiceHelper.PlayUnitVoice("PatriotMissile", "Allied");
				GD.Print($"[PatriotMissilePower] 第{i+1}层 - 获得 {CurrentBlock} 点格挡");
				await CreatureCmd.GainBlock(base.Owner, (decimal)CurrentBlock, ValueProp.Unpowered, null);
			}
		}
	}
}