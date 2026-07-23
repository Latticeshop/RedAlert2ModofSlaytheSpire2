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
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class IronCurtain : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.IronCurtain;

    public IronCurtain() : base((int)Values.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/ircricon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.SovietSuperWeapon.CreateHoverTip(),
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.IntangiblePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[IronCurtain] OnPlay 被调用");

        PlayIronCurtainSound();

        await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.IntangiblePower>(ctx, Owner.Creature, 1m, Owner.Creature, this);

        GD.Print("[IronCurtain] 获得一层无实体");
    }

    private void PlayIronCurtainSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "IronCurtainSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/IronCurtain/iron_curtain_release.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[IronCurtain] 播放铁幕释放音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[IronCurtain] 播放音效失败: {ex.Message}");
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy((int)Values.CostUpgraded);
    }
}