using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace RedAlert2ModCode.Common.GameActions;

public struct NetDollarTransferUnlockAction : INetAction, IPacketSerializable
{
    public GameAction ToGameAction(Player player)
    {
        return new DollarTransferUnlockAction(player);
    }

    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
    }

    public override string ToString()
    {
        return $"NetDollarTransferUnlockAction";
    }
}