using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Relics;

public sealed class UKRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => false;

	public override LocString Title => new("relics", "UK_RELIC.title");
}
