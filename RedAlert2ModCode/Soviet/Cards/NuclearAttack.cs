#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class NuclearAttack : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.NuclearAttack;

    public NuclearAttack() : base((int)Values.Cost, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/nukeicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(Values.Damage, ValueProp.Move),
        new IntVar("Poison", Values.MagicNumber)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.SuperWeapon.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[NuclearAttack] OnPlay 被调用");

        PlayNuclearLaunchSound();
        PlayNuclearAlarmSound();

        await Cmd.Wait(10f);

        var combatState = Owner.Creature.CombatState;
        if (combatState != null)
        {
            var allEnemies = combatState.HittableEnemies.ToList();
            int damage = (int)(IsUpgraded ? Values.Damage + Values.DamageUpgraded : Values.Damage);
            int poisonAmount = (int)Values.MagicNumber;

            foreach (var enemy in allEnemies)
            {
                PlayNuclearExplosionEffect(enemy);
            }

            PlayNuclearExplosionSound();

            await Cmd.Wait(0.3f);

            foreach (var enemy in allEnemies)
            {
                await CreatureCmd.Damage(ctx, new List<Creature> { enemy },
                    (decimal)damage, ValueProp.Move, Owner.Creature, this);

                await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(ctx, enemy, (decimal)poisonAmount, Owner.Creature, this);
            }

            GD.Print($"[NuclearAttack] 对全部敌人造成 {damage} 点伤害，赋予 {poisonAmount} 层中毒");
        }
    }

    private void PlayNuclearLaunchSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "NuclearLaunchSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_launch.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[NuclearAttack] 播放核弹升空音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttack] 播放升空音效失败: {ex.Message}");
        }
    }

    private void PlayNuclearAlarmSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "NuclearAlarmSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_alarm.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[NuclearAttack] 播放核弹警报音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttack] 播放警报音效失败: {ex.Message}");
        }
    }

    private void PlayNuclearExplosionSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "NuclearExplosionSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/NuclearMissile/nuclear_explosion.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[NuclearAttack] 播放核弹爆炸音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttack] 播放爆炸音效失败: {ex.Message}");
        }
    }

    private void PlayNuclearExplosionEffect(Creature target)
    {
        try
        {
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_heavy_blunt");
            VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bloody_impact");

            var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
            if (fireVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
            }

            GD.Print($"[NuclearAttack] 爆炸特效播放完成 - 目标: {target.Name}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NuclearAttack] 播放特效失败: {ex.Message}");
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}