using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.GameActions;

public class DollarTransferUnlockAction : GameAction
{
    public override ulong OwnerId => Sender.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public Player Sender { get; }

    public DollarTransferUnlockAction(Player sender)
    {
        Sender = sender;
    }

    protected override async Task ExecuteAction()
    {
        DollarTransferManager.ResetTransferLock();
    }

    public override INetAction ToNetAction()
    {
        return new NetDollarTransferUnlockAction();
    }

    public override string ToString()
    {
        return $"DollarTransferUnlockAction sender={Sender.NetId}";
    }
}