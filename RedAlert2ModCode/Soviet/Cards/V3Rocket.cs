#nullable enable

using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class V3Rocket : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.V3Rocket;

    public V3Rocket() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/v3icon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("DamageUpgraded", Values.Damage + Values.DamageUpgraded)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Vehicle.CreateHoverTip(),
		HoverTipFactory.FromPower<TargetLockedPower>(),
		HoverTipFactory.FromPower<V3RocketPower>()
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[V3Rocket] 目标不是Creature");
            return;
        }

        await PowerCmd.Apply<TargetLockedPower>(ctx, target, 1m, Owner?.Creature, this);

        await V3RocketPower.ApplyV3Rocket(Owner!.Creature, IsUpgraded);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}