#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 武装直升机（飞行形态）- 苏联空军单位卡（T3，雷达+作战实验室解锁）
/// 1费攻击卡，Token衍生卡
/// 效果：本回合获得 2(升级3) 点敏捷，造成 3 点伤害 2(升级3) 次。[gold]部署[/gold]：切换为炮形态。
/// </summary>
[RegisterCard(typeof(SovietCardPool))]
public sealed class HelicopterFlight : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.HelicopterFlight;

    public HelicopterFlight() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/schpicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new RepeatVar(Values.Repeat),
        new IntVar("Dexterity", Values.MagicNumber)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT3.CreateHoverTip(),
        ModCardKeywords.Aircraft.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip(),
        HoverTipFactory.FromCard<HelicopterCannon>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new()
            {
                Id = "attack",
                Title = new LocString("card_keywords", "ui.helicopter.attack_title"),
                Description = new LocString("card_keywords", "ui.helicopter.flight_attack_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new()
            {
                Id = "deploy",
                Title = new LocString("card_keywords", "ui.helicopter.deploy_title"),
                Description = new LocString("card_keywords", "ui.helicopter.deploy_to_cannon_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(ctx, Owner, new LocString("card_keywords", "ui.helicopter.title"), options, FactionType.Soviet);

        if (selectedIndex == 0)
        {
            await ExecuteAttack(ctx, play);
        }
        else
        {
            await SwitchToCannonForm(ctx);
        }
    }

    private async Task ExecuteAttack(PlayerChoiceContext ctx, CardPlay play)
    {
        // 随机无后缀语音 + 随机直升机机枪音效
        UnitVoiceHelper.PlayUnitVoice("HelicopterFlight", "Soviet");
        UnitVoiceHelper.PlayUnitVoice("HelicopterFlightMG", "Soviet");

        // 本回合获得敏捷
        int dexterity = IsUpgraded ? Values.MagicNumber + Values.MagicNumberUpgraded : Values.MagicNumber;
        await PowerCmd.Apply<HelicopterTemporaryDexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, dexterity, Owner.Creature, this);

        // 造成 3 点伤害 2(升级3) 次
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(ctx);

        GD.Print($"[HelicopterFlight] 获得 {dexterity} 点敏捷，造成 {DynamicVars.Damage.BaseValue} 点伤害 {DynamicVars.Repeat.IntValue} 次");
    }

    /// <summary>
    /// 部署：移除自身并切换为炮形态（参考 IFV 部署转化）。
    /// </summary>
    private async Task SwitchToCannonForm(PlayerChoiceContext ctx)
    {
        UnitVoiceHelper.PlayUnitVoice("HelicopterDeploy", "Soviet");

        var cannonTemplate = ModelDb.Card<HelicopterCannon>();
        var cannonCard = Owner.Creature.CombatState.CreateCard(cannonTemplate, Owner);
        if (IsUpgraded)
            CardCmd.Upgrade(cannonCard);

        await CardPileCmd.RemoveFromCombat(this);
        await CardPileCmd.AddGeneratedCardToCombat(cannonCard, PileType.Hand, Owner);
        GD.Print("[HelicopterFlight] 已切换为武装直升机（炮形态）");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(Values.RepeatUpgraded);
        DynamicVars["Dexterity"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}
