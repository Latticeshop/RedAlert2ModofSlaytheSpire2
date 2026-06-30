#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class FlakSubmarine : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.FlakSubmarine;

    public FlakSubmarine() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/hovricon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new BlockVar(Values.Block, ValueProp.Move),
        new IntVar("DollarNumber", Values.DollarValue)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Navy.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

        int attackIntentCount = 0;
        foreach (var enemy in Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive))
        {
            if (enemy.Monster?.NextMove?.Intents != null)
            {
                foreach (var intent in enemy.Monster.NextMove.Intents)
                {
                    if (intent is AttackIntent)
                    {
                        attackIntentCount++;
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < attackIntentCount; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}