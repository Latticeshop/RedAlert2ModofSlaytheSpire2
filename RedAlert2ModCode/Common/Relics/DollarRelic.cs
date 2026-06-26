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
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Relics;

public class DollarRelic : RelicModel
{
	private static readonly CardValueStore.CardValues Values = CommonRelicValues.DollarRelic;
	
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override async Task BeforeCombatStart()
	{
		GD.Print($"[DollarRelic] 战斗开始，触发刀乐能力");
		Flash();
		
		var player = base.Owner;
		
		var existingPower = player.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		
		if (existingPower != null)
		{
			existingPower.AddDollar(Values.DollarValue);
			GD.Print($"[DollarRelic] 能力已存在，增加资金 {Values.DollarValue}");
		}
		else
		{
			var newPower = await PowerCmd.Apply<Powers.DollarPower>(new ThrowingPlayerChoiceContext(), player.Creature, 1m, player.Creature, null);
			if (newPower != null)
			{
				newPower.SetDollar(Values.DollarValue);
				GD.Print($"[DollarRelic] 创建刀乐能力，初始资金 {Values.DollarValue}");
			}
		}
	}
}