using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 大生产卡牌
/// 稀有金卡，3费，能力卡
/// 效果：每有一层大生产能力，每有一层生产序列，其单位价格减少100
/// </summary>
public sealed class MassProduction : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.MassProduction;

    public MassProduction() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/MassProduction.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.ProductionQueue.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reduction", (int)Values.Stars)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print($"[MassProduction] OnPlay 被调用 - IsUpgraded={base.IsUpgraded}");

        // 触发动画
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 应用大生产能力，传递升级状态
        await MassProductionPower.ApplyMassProduction(Owner.Creature, base.IsUpgraded);

        GD.Print($"[MassProduction] 大生产能力应用完成");
    }

    protected override void OnUpgrade()
    {
        // 升级效果：仅改变卡牌描述，费用不变，效果描述会通过本地化自动更新
        GD.Print($"[MassProduction] 卡牌升级 - 费用保持3费不变");
    }
}
