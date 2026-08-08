using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.DeckConfig;

namespace RedAlert2ModCode.Common.GameActions;

/// <summary>
/// 配置同步动作：把某玩家的角色配置按 NetId 记录到 ModConfigManager，
/// 使多人模式下每个玩家能按自己的配置开局（主机按各玩家 NetId 应用）。
/// </summary>
public class ConfigSyncGameAction : GameAction
{
    public override ulong OwnerId => Sender.NetId;

    public override GameActionType ActionType => GameActionType.Any;

    public Player Sender { get; }

    public string CharacterId { get; }

    public bool EnableCustomDeck { get; }

    public List<string> CustomDeckCardTypes { get; }

    public bool EnableCustomRelics { get; }

    public List<string> StartingRelicTypes { get; }

    public string BaseCarMode { get; }

    public bool LuckyCrateMode { get; }

    public string CratePoolMode { get; }

    public int StartingGold { get; }

    public int MaxHp { get; }

    public ConfigSyncGameAction(Player sender, CharacterConfig config)
    {
        Sender = sender;
        CharacterId = config.CharacterId;
        EnableCustomDeck = config.EnableCustomDeck;
        CustomDeckCardTypes = new List<string>(config.CustomDeckCardTypes);
        EnableCustomRelics = config.EnableCustomRelics;
        StartingRelicTypes = new List<string>(config.StartingRelicTypes);
        BaseCarMode = config.BaseCarMode.ToString();
        LuckyCrateMode = config.LuckyCrateMode;
        CratePoolMode = config.CratePoolMode.ToString();
        StartingGold = config.StartingGold;
        MaxHp = config.MaxHp;
    }

    public ConfigSyncGameAction(
        Player sender,
        string characterId,
        bool enableCustomDeck,
        List<string> customDeckCardTypes,
        bool enableCustomRelics,
        List<string> startingRelicTypes,
        string baseCarMode,
        bool luckyCrateMode,
        string cratePoolMode,
        int startingGold = 0,
        int maxHp = 0)
    {
        Sender = sender;
        CharacterId = characterId;
        EnableCustomDeck = enableCustomDeck;
        CustomDeckCardTypes = customDeckCardTypes ?? new List<string>();
        EnableCustomRelics = enableCustomRelics;
        StartingRelicTypes = startingRelicTypes ?? new List<string>();
        BaseCarMode = baseCarMode;
        LuckyCrateMode = luckyCrateMode;
        CratePoolMode = cratePoolMode;
        StartingGold = startingGold;
        MaxHp = maxHp;
    }

    protected override async Task ExecuteAction()
    {
        var config = new CharacterConfig
        {
            CharacterId = CharacterId,
            EnableCustomDeck = EnableCustomDeck,
            CustomDeckCardTypes = new List<string>(CustomDeckCardTypes),
            EnableCustomRelics = EnableCustomRelics,
            StartingRelicTypes = new List<string>(StartingRelicTypes),
            LuckyCrateMode = LuckyCrateMode,
            CratePoolMode = RedAlert2ModCode.DeckConfig.CratePoolMode.None,
            StartingGold = StartingGold,
            MaxHp = MaxHp,
        };
        if (Enum.TryParse<BaseCarMode>(BaseCarMode, true, out var baseCar))
        {
            config.BaseCarMode = baseCar;
        }
        if (Enum.TryParse<CratePoolMode>(CratePoolMode, true, out var crateMode))
        {
            config.CratePoolMode = crateMode;
        }
        ModConfigManager.SetRemoteCharacterConfig(Sender.NetId, config);
        await Task.CompletedTask;
    }

    public override INetAction ToNetAction()
    {
        return new NetConfigSyncAction
        {
            characterId = CharacterId,
            enableCustomDeck = EnableCustomDeck,
            customDeckCardTypes = CustomDeckCardTypes,
            enableCustomRelics = EnableCustomRelics,
            startingRelicTypes = StartingRelicTypes,
            baseCarMode = BaseCarMode,
            luckyCrateMode = LuckyCrateMode,
            cratePoolMode = CratePoolMode,
            startingGold = StartingGold,
            maxHp = MaxHp,
        };
    }

    public override string ToString()
    {
        return $"ConfigSyncGameAction sender={Sender.NetId} character={CharacterId}";
    }
}
