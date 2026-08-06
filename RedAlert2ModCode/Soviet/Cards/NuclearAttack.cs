#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
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
        ModCardKeywords.SovietSuperWeapon.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[NuclearAttack] OnPlay 被调用");

        // 打出音效：警报 + 升空
        PlayNuclearAlarmSound();
        PlayNuclearLaunchSound();

        // 改为获得核弹攻击能力，回合结束时触发（避免硬等待）
        await NuclearAttackPower.ApplyNuclearAttack(Owner.Creature, IsUpgraded);
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

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    }
}
