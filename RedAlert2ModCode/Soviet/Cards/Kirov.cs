#nullable enable

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
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class Kirov : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.Kirov;

    public Kirov() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/zepicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("DamageUpgraded", Values.Damage + Values.DamageUpgraded)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		ModCardKeywords.Aircraft.CreateHoverTip(),
		HoverTipFactory.FromPower<KirovPower>()
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[Kirov] 目标不是Creature");
            return;
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Smash", Owner.Character.CastAnimDelay);

        int damage = (int)Values.Damage;
        if (IsUpgraded)
            damage += (int)Values.DamageUpgraded;

        await KirovPower.ApplyKirov(target, Owner.Creature, this, damage);

        GD.Print($"[Kirov] 已对 {target.Name} 施加基洛夫debuff，伤害: {damage}");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}