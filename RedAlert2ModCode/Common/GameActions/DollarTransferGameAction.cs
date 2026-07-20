using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Powers;

namespace RedAlert2ModCode.Common.GameActions;

public class DollarTransferGameAction : GameAction
{
    public override ulong OwnerId => Sender.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public Player Sender { get; }

    public ulong ReceiverNetId { get; }

    public int Amount { get; }

    public DollarTransferGameAction(Player sender, ulong receiverNetId, int amount)
    {
        Sender = sender;
        ReceiverNetId = receiverNetId;
        Amount = amount;
    }

    protected override async Task ExecuteAction()
    {
        var senderPower = Sender.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (senderPower == null)
        {
            return;
        }

        var combatState = Sender.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        var receiver = combatState.Players.FirstOrDefault(p => p.NetId == ReceiverNetId);
        if (receiver == null)
        {
            return;
        }

        var receiverPower = receiver.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (receiverPower == null)
        {
            await PowerCmd.Apply<DollarPower>(new ThrowingPlayerChoiceContext(), receiver.Creature, Amount, Sender.Creature, null);
        }
        else
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), receiverPower, Amount, Sender.Creature, null);
        }

        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), senderPower, -Amount, Sender.Creature, null);
    }

    public override INetAction ToNetAction()
    {
        return new NetDollarTransferGameAction
        {
            receiverNetId = ReceiverNetId,
            amount = Amount
        };
    }

    public override string ToString()
    {
        return $"DollarTransferGameAction sender={Sender.NetId} receiver={ReceiverNetId} amount={Amount}";
    }
}