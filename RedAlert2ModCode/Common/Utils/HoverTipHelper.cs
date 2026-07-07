using System;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Common.Utils;

public static class HoverTipHelper
{
    public static IHoverTip FromCardWithUpgrade<T>(Func<bool> isUpgradedFunc) where T : CardModel
    {
        var model = ModelDb.Card<T>();
        var mutable = model.ToMutable();
        
        if (isUpgradedFunc())
        {
            mutable.UpgradeInternal();
        }
        
        return HoverTipFactory.FromCard(mutable);
    }
}