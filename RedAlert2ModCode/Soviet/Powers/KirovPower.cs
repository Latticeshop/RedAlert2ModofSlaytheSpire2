#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class KirovPower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.KirovPower;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/zepicon.png";

    public int CurrentDamage { get; set; } = (int)Values.Damage;

    public int DamagePerStack => CurrentDamage;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Count", CurrentDamage);
            return locString;
        }
    }

    public static async Task<KirovPower?> ApplyKirov(Creature target, Creature source, CardModel sourceCard, int damage)
    {
        var power = await PowerCmd.Apply<KirovPower>(new ThrowingPlayerChoiceContext(), target, 1m, source, sourceCard);
        if (power != null)
        {
            power.CurrentDamage = damage;
        }
        return power;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null) return;

        int stacks = (int)Amount;
        if (stacks <= 0) return;

        for (int i = 0; i < stacks; i++)
        {
            await Task.Delay(300);
            Flash();
            PlayDamageTriggerSound();

            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                new List<Creature> { Owner },
                (decimal)DamagePerStack,
                ValueProp.Move,
                null,
                null);
        }
    }

    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Name = "KirovAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
        GD.Print("[KirovPower] 创建音效播放器");
    }

    private void PlayDamageTriggerSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/Kirov/kirov_attack.mp3");
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print("[KirovPower] 播放攻击音效");
            }
            else
            {
                GD.PrintErr("[KirovPower] 无法加载攻击音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[KirovPower] 播放攻击音效失败: {ex.Message}");
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented) return;
        
        var combatState = Owner?.CombatState;
        if (combatState == null) return;

        var otherEnemies = combatState.HittableEnemies
            .Where(e => e != Owner && e.IsAlive)
            .ToList();

        if (otherEnemies.Count == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        Creature newTarget = otherEnemies[0];
        int stacks = (int)Amount;

        await PowerCmd.Remove(this);

        await PowerCmd.Apply<KirovPower>(
            new ThrowingPlayerChoiceContext(),
            newTarget,
            stacks,
            null,
            null
        );

        GD.Print($"[KirovPower] 基洛夫已转移到 {newTarget.Name}，层数: {stacks}");
    }
}