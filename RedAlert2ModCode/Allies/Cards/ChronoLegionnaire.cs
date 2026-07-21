using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class ChronoLegionnaire : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoLegionnaire;
    private bool _chronoConsumed;

    public ChronoLegionnaire() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/clegicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("ErasePercent", Values.MagicNumber),
        new IntVar("DollarNumber", Values.DollarValue),
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>
            {
                ModCardKeywords.TechLevelT3.CreateHoverTip(),
                ModCardKeywords.Soldier.CreateHoverTip(),
                ModCardKeywords.Erase.CreateHoverTip(),
                HoverTipFactory.FromPower<ErasingPower>()
            };
            
            if (!_chronoConsumed)
            {
                tips.Insert(2, ModCardKeywords.Chrono.CreateHoverTip());
            }
            
            return tips;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allied");
        UnitVoiceHelper.PlayUnitVoice("ChronoLegionnaireAttack", "Allied");

        if (play.Target is not Creature target) return;

        int erasePercent = DynamicVars["ErasePercent"].IntValue;
        int maxErase = 50;
        int eraseAmount = (int)Math.Ceiling(target.MaxHp * erasePercent / 100m);
        eraseAmount = Math.Min(eraseAmount, maxErase);

        bool hasDebuffBefore = target.Powers.Any(p => p.Type == PowerType.Debuff);

        var existingPower = target.Powers.OfType<ErasingPower>().FirstOrDefault();
        if (existingPower != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, eraseAmount, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<ErasingPower>(
                new ThrowingPlayerChoiceContext(),
                target,
                eraseAmount,
                Owner.Creature,
                this
            );

            if (!hasDebuffBefore)
            {
                await CreatureCmd.Stun(target);
            }
        }

        bool hasExhaustKeyword = play.Card.Keywords.Contains(CardKeyword.Exhaust);
        if (hasExhaustKeyword && !_chronoConsumed)
        {
            _chronoConsumed = true;
            if (DynamicVars["ChronoTitle"] is StringVar chronoTitleVar)
            {
                chronoTitleVar.StringValue = string.Empty;
            }
            await CardPileCmd.Add(play.Card, PileType.Draw);
            GD.Print($"[ChronoLegionnaire] 检测到消耗关键字，触发最后一次超时空进入摸牌堆，超时空效果已消耗");
        }
        else if (!hasExhaustKeyword)
        {
            await CardPileCmd.Add(play.Card, PileType.Draw);
            GD.Print($"[ChronoLegionnaire] 超时空效果生效，进入摸牌堆");
        }
        else
        {
            GD.Print($"[ChronoLegionnaire] 超时空效果已消耗，卡牌正常消耗");
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ErasePercent"].UpgradeValueBy(Values.MagicNumberUpgraded);
    }
}