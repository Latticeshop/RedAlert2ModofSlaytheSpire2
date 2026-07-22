using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Relics;

public sealed class PsiCommandoRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool HasUponPickupEffect => false;

	public override LocString Title => new("relics", "PSI_COMMANDO_RELIC.title");
}
