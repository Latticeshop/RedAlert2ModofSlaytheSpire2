#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class GuardianGi : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GuardianGI;

    public GuardianGi() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/gdgiicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new BlockVar(Values.Block, ValueProp.Unpowered),
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("VulnerableStacks", 1)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Soldier.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

        var selectedChoice = await FlakTrackChoiceScreen.ShowSelectionWithSync(Owner,
            "选择重装大兵的行动",
            "部署",
            $"造成 {DynamicVars.Damage.BaseValue} 点伤害，赋予 1 层易伤",
            "防御",
            $"获得 {DynamicVars.Block} 点格挡");

        if (selectedChoice == FlakTrackChoiceScreen.ChoiceType.Deploy)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(play.Target)
                .Execute(ctx);

            await PowerCmd.Apply<VulnerablePower>(
                new ThrowingPlayerChoiceContext(),
                play.Target,
                1,
                Owner.Creature,
                this
            );
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}