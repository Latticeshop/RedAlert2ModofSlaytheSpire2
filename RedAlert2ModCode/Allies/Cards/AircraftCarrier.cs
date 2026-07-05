using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 航空母舰 - 盟军高科技海军单位攻击卡
/// 2费攻击卡，Token衍生卡，需要作战实验室解锁
/// 效果：选择一名敌人获得目标锁定，获得3架黄蜂舰载机
/// </summary>
public sealed class AircraftCarrier : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.AircraftCarrier;

    public AircraftCarrier() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/aircraft_carrier.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("HornetCount", 3)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Navy.CreateHoverTip(),
		HoverTipFactory.FromPower<HornetPower>(),
		HoverTipFactory.FromPower<TargetLockedPower>(),
	];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        GD.Print($"[AircraftCarrier] OnPlay 被调用 - IsUpgraded={IsUpgraded}");

        // 获取目标敌人
        Creature? target = play.Target as Creature;
        if (target == null)
        {
            GD.PrintErr("[AircraftCarrier] 目标不是Creature");
            return;
        }

        // 获取所有敌人
        var combatState = Owner.Creature.CombatState;
        var allEnemies = combatState.Enemies
            .Where(enemy => enemy.Side == CombatSide.Enemy && enemy.IsAlive)
            .ToList();

        // 先清除所有敌人的目标锁定能力（保持唯一性）
        GD.Print($"[AircraftCarrier] 清除所有敌人的目标锁定");
        foreach (var enemy in allEnemies)
        {
            var targetLockedPower = enemy.Powers.FirstOrDefault(p => p is TargetLockedPower) as TargetLockedPower;
            if (targetLockedPower != null)
            {
                GD.Print($"[AircraftCarrier] 移除敌人 {enemy.Name} 的目标锁定");
                await PowerCmd.Remove(targetLockedPower);
            }
        }

        // 为新目标添加目标锁定能力
        GD.Print($"[AircraftCarrier] 为敌人 {target.Name} 添加目标锁定");
        await PowerCmd.Apply<TargetLockedPower>(ctx, target, 1m, Owner.Creature, this);

        // 获得3架黄蜂舰载机
        GD.Print($"[AircraftCarrier] 获得3架黄蜂舰载机 - IsUpgraded={IsUpgraded}");
        await HornetPower.ApplyHornets(Owner.Creature, 3, IsUpgraded);
    }

    protected override void OnUpgrade()
    {
        // 升级后黄蜂舰载机升级（伤害提升）
        GD.Print("[AircraftCarrier] 卡牌升级");
    }
}
