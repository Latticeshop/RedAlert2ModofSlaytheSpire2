using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

public class ExplosionCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.ExplosionCrate;

    public ExplosionCrate() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        ModCardKeywords.Splash.CreateHoverTip()!
    };

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move)
    };

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayRandomExplosionSound();

        decimal damage = DynamicVars.Damage.BaseValue;
        GD.Print($"[ExplosionCrate] 对自己造成 {damage} 点伤害");

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(Owner.Creature)
            .Execute(ctx);

        // 溅射：对其他队友（玩家方其他生物）以及全体怪物造成50%溅射伤害
        List<Creature> splashTargets = CombatState.PlayerCreatures
            .Where(c => c != Owner.Creature && c.IsAlive)
            .Concat(CombatState.HittableEnemies.Where(e => e.IsAlive))
            .ToList();

        if (splashTargets.Count > 0)
        {
            decimal splashDamage = SplashDamageHelper.CalculateSplashDamage(damage);
            GD.Print($"[ExplosionCrate] 对 {splashTargets.Count} 个目标造成 {splashDamage} 点溅射伤害");

            foreach (Creature target in splashTargets)
            {
                await DamageCmd.Attack(splashDamage)
                    .FromCard(this, play)
                    .Targeting(target)
                    .Execute(ctx);
            }
        }
    }
}
