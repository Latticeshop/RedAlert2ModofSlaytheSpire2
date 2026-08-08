using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace RedAlert2ModCode.Common.GameActions;

/// <summary>
/// A2 预选模式的结算动作网络载荷：携带打出建筑 + 玩家选择结果（卡牌 Entry 列表 + 数量列表）。
/// 由 ActionTypes 反射自动注册，跨端可序列化。
/// </summary>
public struct NetBuildingResolutionAction : INetAction, IPacketSerializable
{
    public string buildingEntry;
    public bool isUpgraded;
    public string[] entries;
    public int[] counts;

    public GameAction ToGameAction(Player player)
    {
        return new BuildingResolutionAction(
            player,
            buildingEntry,
            isUpgraded,
            entries ?? System.Array.Empty<string>(),
            counts ?? System.Array.Empty<int>());
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(buildingEntry);
        writer.WriteBool(isUpgraded);
        int eCount = entries?.Length ?? 0;
        writer.WriteInt(eCount);
        for (int i = 0; i < eCount; i++)
            writer.WriteString(entries[i]);
        int cCount = counts?.Length ?? 0;
        writer.WriteInt(cCount);
        for (int i = 0; i < cCount; i++)
            writer.WriteInt(counts[i]);
    }

    public void Deserialize(PacketReader reader)
    {
        buildingEntry = reader.ReadString();
        isUpgraded = reader.ReadBool();
        int eCount = reader.ReadInt();
        entries = new string[eCount];
        for (int i = 0; i < eCount; i++)
            entries[i] = reader.ReadString();
        int cCount = reader.ReadInt();
        counts = new int[cCount];
        for (int i = 0; i < cCount; i++)
            counts[i] = reader.ReadInt();
    }

    public override string ToString()
    {
        return $"NetBuildingResolutionAction building={buildingEntry} entries={entries?.Length}";
    }
}
