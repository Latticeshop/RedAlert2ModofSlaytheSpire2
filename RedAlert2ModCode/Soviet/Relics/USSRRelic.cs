using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Soviet.Relics;

public sealed class USSRRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => false;

	public override LocString Title => new("relics", "USSR_RELIC.title");
}
