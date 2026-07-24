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

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
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
        ModCardKeywords.TechLevelT1.CreateHoverTip(),
        ModCardKeywords.Soldier.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");
        UnitVoiceHelper.PlayUnitVoice("GuardianGiAttack", "Allies");

        var deployDesc = new LocString("card_keywords", "ui.guardian_gi.deploy_desc");
            deployDesc.Add("Damage", DynamicVars.Damage.BaseValue);
            var defendDesc = new LocString("card_keywords", "ui.guardian_gi.defend_desc");
            defendDesc.Add("Block", DynamicVars.Block.BaseValue);

            var options = new List<DeployChoiceScreen.ChoiceOption>
            {
                new DeployChoiceScreen.ChoiceOption
                {
                    Id = "deploy",
                    Title = new LocString("card_keywords", "ui.guardian_gi.deploy_title"),
                    Description = deployDesc,
                    IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
                },
                new DeployChoiceScreen.ChoiceOption
                {
                    Id = "defend",
                    Title = new LocString("card_keywords", "ui.guardian_gi.defend_title"),
                    Description = defendDesc
                }
            };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", "ui.guardian_gi.title"), options, FactionType.Allied);

        if (selectedIndex == 0)
        {
            UnitVoiceHelper.PlayUnitVoice("GuardianGiDeploy", "Allies");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
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
