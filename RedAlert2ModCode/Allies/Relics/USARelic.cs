using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Relics;

public sealed class USARelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => false;

	public override LocString Title => new("relics", "USA_RELIC.title");
}
