using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

public class SovietTeslaCoilChargePower : PowerModel
{
	private static readonly CardValueStore.CardValues Values = SovietPowerValues.TeslaCoilChargePower;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public SovietTeslaCoilChargePower()
	{
	}

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			locString.Add("Count", (int)Amount);
			return locString;
		}
	}

	public static async Task ApplyCharge(Creature owner)
	{
		var existingPower = owner.Powers.OfType<SovietTeslaCoilChargePower>().FirstOrDefault();

		if (existingPower != null)
		{
			int maxStacks = (int)Values.Repeat;
			if (existingPower.Amount < maxStacks)
			{
				await PowerCmd.Apply<SovietTeslaCoilChargePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
				GD.Print($"[TeslaCoilChargePower] 叠加充能 - Amount={existingPower.Amount}");
			}
		}
		else
		{
			var newPower = await PowerCmd.Apply<SovietTeslaCoilChargePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
			if (newPower != null)
			{
				GD.Print($"[TeslaCoilChargePower] 创建充能能力 - Amount={newPower.Amount}");
			}
		}
	}
}
