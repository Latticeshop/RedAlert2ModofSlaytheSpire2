using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class BarracksCard : CardModel
{
    public BarracksCard() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        List<CardModel> availableCards = new()
        {
            Owner.Creature.CombatState.CreateCard(ModelDb.Card<AmericanSoldier>(), Owner)
        };

        CardModel? selectedCard = await CardSelectCmd.FromChooseACardScreen(ctx, availableCards, Owner, canSkip: false);

        if (selectedCard != null)
        {
            var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(Owner.Creature, 1m, Owner.Creature, this);
            
            if (trainingPower != null)
            {
                trainingPower.TrainedCardId = selectedCard.Id.Entry;
                trainingPower.UnitName = selectedCard.Title.ToString();
                trainingPower.IsUpgraded = base.IsUpgraded;
                
                // 使用图标管理器设置能力图标
                PowerIconManager.SetIcon(trainingPower, selectedCard.PortraitPath);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果：生成的单位序列卡牌也会升级（费用不变）
    }
}