using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Relics;

public sealed class FranceRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => false;
}
