using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Relics;

/// <summary>
/// 先古刀乐遗物（先古事件选项）
/// 战斗开始时获得10000初始资金
/// </summary>
public class DollarAncientRelic : RelicModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesRelicValues.DollarAncientRelic;
	
	public override RelicRarity Rarity => RelicRarity.Starter;

	/// <summary>
	/// 战斗开始时触发
	/// </summary>
	public override async Task BeforeCombatStart()
	{
		GD.Print($"[DollarAncientRelic] 战斗开始，触发先古刀乐能力");
		Flash();
		
		// 获取玩家
		var player = base.Owner;
		
		// 查找是否已有刀乐能力
		var existingPower = player.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		
		if (existingPower != null)
		{
			// 能力已存在，增加资金
			existingPower.AddDollar(Values.DollarValue);
			GD.Print($"[DollarAncientRelic] 能力已存在，增加资金 {Values.DollarValue}");
		}
		else
		{
			// 首次应用，创建新能力
			var newPower = await PowerCmd.Apply<Powers.DollarPower>(new ThrowingPlayerChoiceContext(), player.Creature, 1m, player.Creature, null);
			if (newPower != null)
			{
				newPower.SetDollar(Values.DollarValue);
				GD.Print($"[DollarAncientRelic] 创建刀乐能力，初始资金 {Values.DollarValue}");
			}
		}
	}
}