using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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

/// <summary>
/// 恐怖机器人 - 苏联装甲单位
/// 1费攻击卡，赋予恐怖机器人+缓慢
/// </summary>
public sealed class TerrorDrone : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.TerrorDrone;

    public TerrorDrone() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/dronicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("TerrorDroneStacks", Values.MagicNumber),
        new IntVar("SlowStacks", 1)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip(),
		HoverTipFactory.FromPower<SovietTerrorDronePower>(),
		HoverTipFactory.FromPower<SlowPower>()
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
        
        int terrorDroneStacks = DynamicVars["TerrorDroneStacks"].IntValue;

        await PowerCmd.Apply<SovietTerrorDronePower>(
            new ThrowingPlayerChoiceContext(),
            play.Target,
            terrorDroneStacks,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<SlowPower>(
            new ThrowingPlayerChoiceContext(),
            play.Target,
            1,
            Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TerrorDroneStacks"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}
