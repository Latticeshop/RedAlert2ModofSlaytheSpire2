using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Allies.Powers;

public sealed class ErasingPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/clegicon.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Amount", (int)Amount);
            return locString;
        }
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null) return;

        await CheckErase(choiceContext, combatState);
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        SetTransparency();
        await CheckErase(new ThrowingPlayerChoiceContext(), null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        ResetTransparency();
    }

    private void SetTransparency()
    {
        if (Owner == null) return;
        
        var nCreature = Owner.GetCreatureNode();
        if (nCreature?.Visuals != null)
        {
            var body = nCreature.Visuals.GetCurrentBody();
            body.Modulate = new Color(1f, 1f, 1f, 0.4f);
        }
    }

    private void ResetTransparency()
    {
        if (Owner == null) return;
        
        var nCreature = Owner.GetCreatureNode();
        if (nCreature?.Visuals != null)
        {
            var body = nCreature.Visuals.GetCurrentBody();
            body.Modulate = new Color(1f, 1f, 1f, 1f);
        }
    }

    private async Task CheckErase(PlayerChoiceContext choiceContext, ICombatState? combatState)
    {
        int eraseStacks = (int)Amount;
        if (eraseStacks <= 0) return;

        if (Owner == null) return;

        if (eraseStacks > (int)Owner.CurrentHp)
        {
            Flash();
            UnitVoiceHelper.PlayUnitVoice("ChronoLegionnaireKill", "Allied");
            
            await CreatureCmd.Kill(Owner);
        }
    }
}
