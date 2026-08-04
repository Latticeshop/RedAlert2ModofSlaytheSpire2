using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace RedAlert2ModCode.Common.GameActions;

/// <summary>
/// 配置同步网络动作：携带某个玩家的角色配置，同步到主机后按 NetId 独立保存。
/// </summary>
public struct NetConfigSyncAction : INetAction, IPacketSerializable
{
    public string characterId;
    public bool enableCustomDeck;
    public List<string> customDeckCardTypes;
    public string baseCarMode;
    public bool luckyCrateMode;
    public string cratePoolMode;

    public GameAction ToGameAction(Player player)
    {
        return new ConfigSyncGameAction(
            player,
            characterId,
            enableCustomDeck,
            customDeckCardTypes ?? new List<string>(),
            baseCarMode,
            luckyCrateMode,
            cratePoolMode);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(characterId);
        writer.WriteBool(enableCustomDeck);
        writer.WriteInt(customDeckCardTypes?.Count ?? 0);
        if (customDeckCardTypes != null)
        {
            foreach (var cardType in customDeckCardTypes)
            {
                writer.WriteString(cardType);
            }
        }
        writer.WriteString(baseCarMode);
        writer.WriteBool(luckyCrateMode);
        writer.WriteString(cratePoolMode);
    }

    public void Deserialize(PacketReader reader)
    {
        characterId = reader.ReadString();
        enableCustomDeck = reader.ReadBool();
        int count = reader.ReadInt();
        customDeckCardTypes = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            customDeckCardTypes.Add(reader.ReadString());
        }
        baseCarMode = reader.ReadString();
        luckyCrateMode = reader.ReadBool();
        cratePoolMode = reader.ReadString();
    }

    public override string ToString()
    {
        return $"NetConfigSyncAction character={characterId}";
    }
}
