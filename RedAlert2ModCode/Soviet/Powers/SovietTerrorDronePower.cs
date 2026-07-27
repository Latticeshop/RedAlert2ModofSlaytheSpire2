using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Powers;

/// <summary>
/// 恐怖机器人 - 敌人debuff能力
/// 每回合受到层数数值的伤害，获得一回合迟缓，回血时清除
/// </summary>
public sealed class SovietTerrorDronePower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = SovietPowerValues.TerrorDronePower;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>使用恐怖机器人卡牌的图标</summary>
    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/dronicon.png";

    /// <summary>每回合造成的伤害值（基于层数）</summary>
    public int DamagePerStack => (int)Values.Damage;

    /// <summary>
    /// 本地化描述
    /// </summary>
    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Count", DamagePerStack);
            return locString;
        }
    }

    /// <summary>能力应用时，检查并施加迟缓</summary>
    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        bool hasSlow = target.Powers?.Any(p => p is SlowPower) ?? false;
        if (!hasSlow)
        {
            await PowerCmd.Apply<SlowPower>(new ThrowingPlayerChoiceContext(), target, 1, applier, cardSource, silent: true);
        }
    }

    /// <summary>能力层数变化时，检查并施加迟缓</summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        
        bool hasSlow = Owner.Powers?.Any(p => p is SlowPower) ?? false;
        if (!hasSlow)
        {
            await PowerCmd.Apply<SlowPower>(choiceContext, Owner, 1, applier, cardSource, silent: true);
        }
    }

    /// <summary>敌人回合开始时触发伤害（按层数循环）</summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null) return;
        
        int stacks = (int)Amount;
        if (stacks <= 0) return;

        Flash();
        PlayDamageTriggerSound();
        
        for (int i = 0; i < stacks; i++)
        {
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                Owner,
                (decimal)DamagePerStack,
                ValueProp.Move,
                null,
                null);
        }
    }

    /// <summary>敌人回合结束时移除迟缓</summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner)) return;

        var slowPower = Owner.Powers?.FirstOrDefault(p => p is SlowPower) as SlowPower;
        if (slowPower != null)
        {
            await PowerCmd.Apply<SlowPower>(choiceContext, Owner, -1, Owner, null);
        }
    }

    private static AudioStreamPlayer? _audioPlayer;

    private static void EnsureAudioPlayer()
    {
        if (_audioPlayer != null && GodotObject.IsInstanceValid(_audioPlayer))
            return;

        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Name = "TerrorDroneAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_audioPlayer);
        GD.Print("[SovietTerrorDronePower] 创建音效播放器");
    }

    private void PlayDamageTriggerSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/TerrorDrone/damage_trigger.mp3");
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print("[SovietTerrorDronePower] 播放伤害触发音效");
            }
            else
            {
                GD.PrintErr("[SovietTerrorDronePower] 无法加载伤害触发音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SovietTerrorDronePower] 播放伤害音效失败: {ex.Message}");
        }
    }

    private void PlayHealRemoveSound()
    {
        try
        {
            EnsureAudioPlayer();
            if (_audioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/TerrorDrone/heal_remove.mp3");
            if (soundFile != null)
            {
                _audioPlayer.Stream = soundFile;
                _audioPlayer.VolumeDb = -5;
                _audioPlayer.Play();
                GD.Print("[SovietTerrorDronePower] 播放消失音效");
            }
            else
            {
                GD.PrintErr("[SovietTerrorDronePower] 无法加载消失音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SovietTerrorDronePower] 播放消失音效失败: {ex.Message}");
        }
    }

    public async Task OnOwnerHealed()
    {
        if (Owner == null) return;
        
        PlayHealRemoveSound();
        
        var slowPower = Owner.Powers?.FirstOrDefault(p => p is SlowPower) as SlowPower;
        if (slowPower != null)
        {
            await PowerCmd.Remove(slowPower);
            GD.Print("[SovietTerrorDronePower] 移除迟缓debuff");
        }
        
        await PowerCmd.Remove(this);
    }
}

[HarmonyPatch]
public static class SovietTerrorDroneHealPatch
{
    private static MethodBase TargetMethod()
    {
        return typeof(CreatureCmd).GetMethod("Heal", 
            BindingFlags.Public | BindingFlags.Static, 
            null, 
            new[] { typeof(Creature), typeof(decimal), typeof(bool) }, 
            null);
    }

    private static async void Postfix(Creature creature, decimal amount)
    {
        if (amount <= 0) return;
        
        var terrorDrone = creature.Powers?.FirstOrDefault(p => p is SovietTerrorDronePower) as SovietTerrorDronePower;
        if (terrorDrone != null)
        {
            await terrorDrone.OnOwnerHealed();
        }
    }
}