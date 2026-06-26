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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 幻影坦克 - 盟军高科技装甲单位卡
/// 1费攻击卡，Token衍生卡，需要作战实验室解锁
/// 效果：若敌人意图攻击，获得16(升级20)点格挡；否则造成10(升级15)点伤害
/// 使用热能射线火焰特效动画
/// </summary>
public sealed class MirageTank : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.MirageTank;

    public MirageTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/rtnkicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new BlockVar(Values.Block, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.Vehicle.CreateHoverTip()
        // HoverTipFactory.FromPower<IntangiblePower>()  // 已移除无实体效果
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        GD.Print("[MirageTank] OnPlay 被调用");

        // 获取目标敌人
        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[MirageTank] 目标不是Creature");
            return;
        }

        // 检查敌人是否意图攻击（通过MonsterModel.IntendsToAttack属性）
        bool intendsToAttack = target.Monster?.IntendsToAttack ?? false;
        GD.Print($"[MirageTank] 敌人意图攻击: {intendsToAttack}");

        if (intendsToAttack)
        {
            // 敌人意图攻击：获得格挡
            GD.Print($"[MirageTank] 敌人意图攻击，获得格挡: {DynamicVars.Block.BaseValue}");
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

            // === 原无实体逻辑（已注释） ===
            // 检查玩家是否已有无实体能力
            // bool hasIntangible = Owner.Creature.Powers.Any(p => p is IntangiblePower);
            // GD.Print($"[MirageTank] 玩家已有无实体: {hasIntangible}");
            //
            // if (!hasIntangible)
            // {
            //     // 获得一层无实体（不播放特效）
            //     GD.Print("[MirageTank] 获得1层无实体");
            //     await PowerCmd.Apply<IntangiblePower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
            // }
            // else
            // {
            //     // 已有无实体，改为造成伤害
            //     GD.Print("[MirageTank] 已有无实体，造成伤害");
            //     await DealDamage(ctx, target);
            // }
        }
        else
        {
            // 敌人不意图攻击，造成伤害
            GD.Print("[MirageTank] 敌人不意图攻击，造成伤害");
            await DealDamage(ctx, target);
        }
    }

    /// <summary>
    /// 造成伤害并播放火焰特效
    /// </summary>
    private async Task DealDamage(PlayerChoiceContext ctx, Creature target)
    {
        decimal damage = DynamicVars.Damage.BaseValue;
        
        // 播放火焰特效（热能射线动画）
        PlayFireVfx(target);
        
        // 造成伤害
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(ctx);
    }

    /// <summary>
    /// 播放火焰特效（热能射线动画）
    /// </summary>
    private void PlayFireVfx(Creature target)
    {
        try
        {
            // 使用NFireBurningVfx.Create方法创建火焰特效
            NFireBurningVfx? fireVfx = NFireBurningVfx.Create(target, scaleFactor: 1.0f, goingRight: true);
            
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(fireVfx);
                SfxCmd.Play("event:/sfx/characters/attack_fire");
                GD.Print("[MirageTank] 火焰特效播放成功");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MirageTank] 火焰特效播放失败: {ex.Message}");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后伤害增加5（10 -> 15）
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
        // 升级后格挡增加4（16 -> 20）
        DynamicVars.Block.UpgradeValueBy(Values.BlockUpgraded);
    }
}