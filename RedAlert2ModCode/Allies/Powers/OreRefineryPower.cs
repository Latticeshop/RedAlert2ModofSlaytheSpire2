using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class OreRefineryPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".smartDescription");
            locString.Add("Bonus", (int)base.Amount);
            return locString;
        }
    }

    public float GetOreMultiplier()
    {
        return 1.0f + (int)base.Amount / 100.0f;
    }
}