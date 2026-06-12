using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// IFV临时敏捷能力
/// </summary>
public sealed class IfvTemporaryDexterityPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Ifv>();
}