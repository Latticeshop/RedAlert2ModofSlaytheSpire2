using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class GiantSquid : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.GiantSquid;

    public GiantSquid() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/sqdicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("WeakStacks", 1),
        new IntVar("GiantSquidStacks", Values.MagicNumber),
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT1.CreateHoverTip(),
        ModCardKeywords.Navy.CreateHoverTip(),
        HoverTipFactory.FromPower<SovietGiantSquidPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        int weakStacks = DynamicVars["WeakStacks"].IntValue;
        int squidStacks = DynamicVars["GiantSquidStacks"].IntValue;

        await PowerCmd.Apply<WeakPower>(
            new ThrowingPlayerChoiceContext(),
            play.Target,
            weakStacks,
            Owner.Creature,
            this
        );

        var existingPower = (play.Target as Creature)?.Powers.OfType<SovietGiantSquidPower>().FirstOrDefault();
        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, squidStacks, Owner.Creature, this);
        }
        else
        {
            var power = await PowerCmd.Apply<SovietGiantSquidPower>(
                new ThrowingPlayerChoiceContext(),
                play.Target,
                squidStacks,
                Owner.Creature,
                this
            );
            if (power != null)
            {
                power.CurrentStacks = squidStacks;
                power.IsUpgraded = IsUpgraded;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakStacks"].UpgradeValueBy(1);
        DynamicVars["GiantSquidStacks"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}
