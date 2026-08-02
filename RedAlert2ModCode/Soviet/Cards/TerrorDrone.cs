using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 恐怖机器人 - 苏联装甲单位
/// 1费攻击卡，赋予恐怖机器人伤害能力 + 减速debuff
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class TerrorDrone : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.TerrorDrone;

    public TerrorDrone() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/dronicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("TerrorDroneStacks", Values.MagicNumber)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			ModCardKeywords.TechLevelT1.CreateHoverTip(),
			ModCardKeywords.Vehicle.CreateHoverTip(),
			HoverTipFactory.FromPower<SovietTerrorDronePower>(),
			HoverTipFactory.FromPower<DecelerationPower>()
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

        await PowerCmd.Apply<DecelerationPower>(
            new ThrowingPlayerChoiceContext(),
            play.Target,
            1m,
            Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TerrorDroneStacks"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}
