#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 光棱坦克 - 盟军高科技装甲单位卡
/// 1费攻击卡，Token衍生卡，需要作战实验室解锁
/// 效果：造成15(升级20)点伤害。[gold]溅射[/gold]
/// 使用扫荡射线动画展示射线感
/// </summary>
public sealed class PrismTank : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.PrismTank;

    public PrismTank() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/sreficon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Vehicle.CreateHoverTip(),
        ModCardKeywords.Splash.CreateHoverTip()!
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        UnitVoiceHelper.PlayUnitVoice("PrismTankAttack", "Allied");
        GD.Print("[PrismTank] OnPlay 被调用");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[PrismTank] 目标不是Creature");
            return;
        }

        List<Creature> allEnemies = CombatState.HittableEnemies.ToList();
        List<Creature> otherEnemies = SplashDamageHelper.GetSplashTargets(target, allEnemies);

        GD.Print($"[PrismTank] 主目标: {target.Name}, 其他敌人数量: {otherEnemies.Count}");

        await PlayBeamVfx(target, otherEnemies);

        decimal mainDamage = DynamicVars.Damage.BaseValue;
        GD.Print($"[PrismTank] 对主目标造成 {mainDamage} 点伤害");
        await DamageCmd.Attack(mainDamage)
            .FromCard(this, play)
            .Targeting(target)
            .Execute(ctx);

        if (otherEnemies.Count > 0)
        {
            decimal splashDamage = SplashDamageHelper.CalculateSplashDamage(mainDamage);
            GD.Print($"[PrismTank] 对其他敌人造成 {splashDamage} 点溅射伤害");

            foreach (Creature otherEnemy in otherEnemies)
            {
                await DamageCmd.Attack(splashDamage)
                    .FromCard(this, play)
                    .Targeting(otherEnemy)
                    .Execute(ctx);
            }
        }
    }

    /// <summary>
    /// 播放射线动画（扫荡射线特效）
    /// </summary>
    private async Task PlayBeamVfx(Creature mainTarget, List<Creature> otherTargets)
    {
        try
        {
            // 创建所有目标列表（主目标 + 其他目标）
            List<Creature> allTargets = new List<Creature> { mainTarget };
            allTargets.AddRange(otherTargets);

            // 使用扫荡射线动画展示射线感
            NSweepingBeamVfx? beamVfx = NSweepingBeamVfx.Create(Owner.Creature, allTargets);
            if (beamVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(beamVfx);
                GD.Print("[PrismTank] 射线动画播放成功");
                await Cmd.Wait(0.5f);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[PrismTank] 射线动画播放失败: {ex.Message}");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后伤害增加5（15 -> 20）
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}