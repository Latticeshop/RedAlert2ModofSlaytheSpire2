#nullable enable

using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Soviet.Relics;

public sealed class ChronoIvanRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => false;

    public override LocString Title => new("relics", "CHRONO_IVAN_RELIC.title");
}
