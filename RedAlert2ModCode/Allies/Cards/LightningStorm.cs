#nullable enable

using System.Collections.Generic;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 闪电风暴 - 盟军运转卡（超级武器）
/// 4费技能卡（升级3费），金卡
/// 效果：生成电球，然后触发"电流相生"效果
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class LightningStorm : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.LightningStorm;
    
    public LightningStorm() : base((int)Values.Cost, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/lightning_storm.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Block", Values.Block)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.AlliedSuperWeapon.CreateHoverTip(),
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<LightningOrb>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[LightningStorm] OnPlay 被调用");

        // 播放施法动画
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 从动态变量获取电球数量
        int orbCount = DynamicVars["Block"].IntValue;
        
        // 第一步：生成电球（数量从配置读取）
        GD.Print($"[LightningStorm] 开始生成 {orbCount} 个闪电球");
        for (int i = 0; i < orbCount; i++)
        {
            await OrbCmd.Channel<LightningOrb>(ctx, Owner);
            GD.Print($"[LightningStorm] 已生成第 {i + 1} 个闪电球");
        }

        // 第二步：模拟"电流相生"效果（根据本回合已引导的闪电球数量再生成相同数量）
        // 先计算本回合已引导的闪电球数量
        var lightningChanneledCount = CombatManager.Instance.History.Entries
            .OfType<MegaCrit.Sts2.Core.Combat.History.Entries.OrbChanneledEntry>()
            .Count(e => e.Actor.Player == Owner && e.Orb is LightningOrb);
        
        GD.Print($"[LightningStorm] 本回合已引导 {lightningChanneledCount} 个闪电球，开始触发电流相生效果");

        // 根据已引导的数量再生成相同数量的闪电球
        for (int i = 0; i < lightningChanneledCount; i++)
        {
            await OrbCmd.Channel<LightningOrb>(ctx, Owner);
            GD.Print($"[LightningStorm] 电流相生效果 - 额外生成第 {i + 1} 个闪电球");
        }

        GD.Print("[LightningStorm] 闪电风暴效果完成");
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy((int)Values.CostUpgraded); // 升级后费用不变（3+0=3）
        DynamicVars["Block"].UpgradeValueBy((int)Values.BlockUpgraded); // 升级后电球数量增加1（1+1=2）
    }
}