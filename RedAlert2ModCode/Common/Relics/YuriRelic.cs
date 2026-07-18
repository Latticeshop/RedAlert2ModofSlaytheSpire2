using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Common.Relics;

public sealed class YuriRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override bool HasUponPickupEffect => false;
}
