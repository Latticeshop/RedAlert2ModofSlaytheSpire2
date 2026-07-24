#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 超时空传送 - 盟军运转卡（超级武器）
/// 1费技能卡（升级0费），金卡，消耗
/// 效果：从摸牌/手牌/弃牌堆选任意张牌到摸牌/手牌/弃牌堆
/// </summary>
[RegisterCard(typeof(AlliesCardPool))]
public sealed class ChronoWarp : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoWarp;
    
    public ChronoWarp() : base((int)Values.Cost, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        // 不要在构造函数中调用 AddKeyword，会在规范模型上抛出异常
    }

    public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/chroicon.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.AlliedSuperWeapon.CreateHoverTip()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[ChronoWarp] OnPlay 被调用");

        PlayChronoReleaseSound();

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 第一步：选择源牌堆
        var sourcePileInt = await ChronoWarpScreen.ShowPileSelectionWithSync(new LocString("card_keywords", "ui.chrono_warp.source").GetFormattedText(), Owner);
        if (sourcePileInt == null)
        {
            GD.Print("[ChronoWarp] 取消选择");
            return;
        }

        var sourcePile = ConvertToPileType(sourcePileInt.Value);

        // 获取源牌堆的卡牌
        var cardsInSource = GetCardsInPile(sourcePile);
        if (!cardsInSource.Any())
        {
            GD.Print("[ChronoWarp] 源牌堆为空");
            return;
        }

        // 选择要移动的卡牌（可多选）
        var selectedCards = await CardSelectionSyncHelper.ShowMultiSelectionWithSync(cardsInSource, cardsInSource.Count, 1, Owner);
        if (selectedCards == null || !selectedCards.Any())
        {
            GD.Print("[ChronoWarp] 未选择任何卡牌");
            return;
        }

        // 第二步：选择目标牌堆
        var targetPileInt = await ChronoWarpScreen.ShowPileSelectionWithSync(new LocString("card_keywords", "ui.chrono_warp.target").GetFormattedText(), Owner);
        if (targetPileInt == null)
        {
            GD.Print("[ChronoWarp] 取消选择目标");
            return;
        }

        var targetPile = ConvertToPileType(targetPileInt.Value);

        // 移动卡牌到目标牌堆
        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, targetPile);
            GD.Print($"[ChronoWarp] 移动卡牌 {card.Id.Entry} 到 {targetPile}");
        }
    }

    private PileType ConvertToPileType(int choice)
    {
        return choice switch
        {
            0 => PileType.Draw,
            1 => PileType.Hand,
            2 => PileType.Discard,
            _ => PileType.Hand
        };
    }

    private List<CardModel> GetCardsInPile(PileType pileType)
    {
        var pile = pileType.GetPile(Owner);
        return pile.Cards.ToList();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy((int)Values.CostUpgraded);
    }

    private void PlayChronoReleaseSound()
    {
        try
        {
            var audioPlayer = new AudioStreamPlayer();
            audioPlayer.Name = "ChronoReleaseSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/AlliedUnits/ChronoWarp/chrono_release.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[ChronoWarp] 播放超时空传送释放音效");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ChronoWarp] 播放音效失败: {ex.Message}");
        }
    }
}
