using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Soviet.Relics;

public sealed class IraqRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => false;
}
