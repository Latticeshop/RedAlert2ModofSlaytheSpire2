using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace RedAlert2ModCode.Common.GameActions;

public struct NetDollarTransferGameAction : INetAction, IPacketSerializable
{
    public ulong receiverNetId;

    public int amount;

    public GameAction ToGameAction(Player player)
    {
        return new DollarTransferGameAction(player, receiverNetId, amount);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(receiverNetId);
        writer.WriteInt(amount);
    }

    public void Deserialize(PacketReader reader)
    {
        receiverNetId = reader.ReadULong();
        amount = reader.ReadInt();
    }

    public override string ToString()
    {
        return $"NetDollarTransferGameAction receiver={receiverNetId} amount={amount}";
    }
}