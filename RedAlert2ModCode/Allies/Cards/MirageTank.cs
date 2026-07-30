#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 幻影坦克 - 盟军高科技装甲单位卡
/// 1费攻击卡，Token衍生卡，需要作战实验室解锁
/// 效果：[gold]伪装[/gold]（本回合未造成伤害时），或者造成8(升级12)点伤害
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class MirageTank : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.MirageTank;

    public MirageTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/rtnkicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Vehicle.CreateHoverTip(),
        HoverTipFactory.FromPower<CamouflagePower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        GD.Print("[MirageTank] OnPlay 被调用");

        // 检查本回合是否已造成伤害
        bool hasDealtDamage = Owner.Creature.Powers.Any(p => p is DamageDealtTrackerPower);
        GD.Print($"[MirageTank] 本回合是否已造成伤害: {hasDealtDamage}");

        if (!hasDealtDamage)
        {
            // 本回合未造成伤害：赋予伪装能力（BeforeApplied会自动赋予无实体）
            GD.Print("[MirageTank] 赋予伪装能力");
            AudioHelper.PlayCamouflageSound();
            await PowerCmd.Apply<CamouflagePower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            // 本回合已造成伤害：改为造成伤害
            Creature? target = play.Target as Creature;
            if (target == null)
            {
                GD.PrintErr("[MirageTank] 目标不是Creature");
                return;
            }

            GD.Print("[MirageTank] 已造成伤害，改为造成伤害");
            await DealDamage(ctx, target, play);
        }
    }

    /// <summary>
    /// 造成伤害
    /// </summary>
    private async Task DealDamage(PlayerChoiceContext ctx, Creature target, CardPlay play)
    {
        // 播放幻影坦克攻击音效
        AudioHelper.PlayMirageTankAttackSound();
        
        decimal damage = DynamicVars.Damage.BaseValue;
        
        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(target)
            .Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        // 升级后伤害增加4（8 -> 12）
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}
