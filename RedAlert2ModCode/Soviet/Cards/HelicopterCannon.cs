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
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 武装直升机（炮形态）- 苏联空军单位卡（T3）
/// 1费攻击卡，Token衍生卡
/// 效果：造成 12(升级15) 点溅射伤害。[gold]部署[/gold]：切换为飞行形态。
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class HelicopterCannon : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.HelicopterCannon;

    public HelicopterCannon() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/helicopter_cannon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Aircraft.CreateHoverTip(),
        ModCardKeywords.Splash.CreateHoverTip()!,
        ModCardKeywords.Deploy.CreateHoverTip(),
        HoverTipFactory.FromCard<HelicopterFlight>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new()
            {
                Id = "attack",
                Title = new LocString("card_keywords", "ui.helicopter.attack_title"),
                Description = new LocString("card_keywords", "ui.helicopter.cannon_attack_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new()
            {
                Id = "deploy",
                Title = new LocString("card_keywords", "ui.helicopter.deploy_title"),
                Description = new LocString("card_keywords", "ui.helicopter.deploy_to_flight_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(ctx, Owner, new LocString("card_keywords", "ui.helicopter.title"), options, FactionType.Soviet);

        if (selectedIndex == 0)
        {
            await ExecuteCannonAttack(ctx, play);
        }
        else
        {
            await SwitchToFlightForm(ctx);
        }
    }

    private async Task ExecuteCannonAttack(PlayerChoiceContext ctx, CardPlay play)
    {
        // 随机 "-power" 语音 + "Vchoat2a-能力攻击" 攻击音效
        UnitVoiceHelper.PlayUnitVoice("HelicopterCannon", "Soviet");
        UnitVoiceHelper.PlayUnitVoice("HelicopterCannonAttack", "Soviet");

        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[HelicopterCannon] 目标不是Creature");
            return;
        }

        var allEnemies = CombatState.HittableEnemies.ToList();
        var otherEnemies = SplashDamageHelper.GetSplashTargets(target, allEnemies);

        decimal mainDamage = DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(mainDamage)
            .FromCard(this, play)
            .Targeting(target)
            .Execute(ctx);
        GD.Print($"[HelicopterCannon] 对主目标造成 {mainDamage} 点伤害");

        if (otherEnemies.Count > 0)
        {
            decimal splashDamage = SplashDamageHelper.CalculateSplashDamage(mainDamage);
            foreach (var otherEnemy in otherEnemies)
            {
                await DamageCmd.Attack(splashDamage)
                    .FromCard(this, play)
                    .Targeting(otherEnemy)
                    .Execute(ctx);
                GD.Print($"[HelicopterCannon] 对 {otherEnemy.Name} 造成 {splashDamage} 点溅射伤害");
            }
        }
    }

    /// <summary>
    /// 部署：移除自身并切换回飞行形态。
    /// </summary>
    private async Task SwitchToFlightForm(PlayerChoiceContext ctx)
    {
        UnitVoiceHelper.PlayUnitVoice("HelicopterDeploy", "Soviet");

        var flightTemplate = ModelDb.Card<HelicopterFlight>();
        var flightCard = Owner.Creature.CombatState.CreateCard(flightTemplate, Owner);
        if (IsUpgraded)
            CardCmd.Upgrade(flightCard);

        await CardPileCmd.RemoveFromCombat(this);
        await CardPileCmd.AddGeneratedCardToCombat(flightCard, PileType.Hand, Owner);
        GD.Print("[HelicopterCannon] 已切换为武装直升机（飞行形态）");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}
