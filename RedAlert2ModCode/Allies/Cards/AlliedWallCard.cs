using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 盟军围墙 - 盟军建筑卡
/// 1费技能卡
/// 效果：获得5点护盾（升级后8点）
/// </summary>
public sealed class AlliedWallCard : CardModel
{
    public AlliedWallCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/wallicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new BlockVar(5m, ValueProp.Unpowered)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        // 获得护盾
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade()
    {
        // 升级后护盾从5点提升到8点
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}