#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Common.Cards;

public sealed class ChronoIvanCard : ChronoCardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.ChronoIvan;

    public ChronoIvanCard() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/other/ivncicon.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[0];

    protected override List<IHoverTip> GetExtraHoverTips()
    {
        return new List<IHoverTip>
        {
            ModCardKeywords.Infiltrator.CreateHoverTip(),
            ModCardKeywords.Soldier.CreateHoverTip(),
            HoverTipFactory.FromPower<TimedBombPower>(),
            ModCardKeywords.Deploy.CreateHoverTip(),
            ModCardKeywords.Unit.CreateHoverTip()
        };
    }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("BombStacks", Values.Damage),
        new IntVar("BombStacksUpgraded", Values.DamageUpgraded),
        new IntVar("DeployVigor", Values.DeployVigor),
        new IntVar("DeployVigorUpgraded", Values.DeployVigorUpgraded),
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        PlayChronoMoveSound();
        PlayCrazyIvanVoice();

        int bombStacks = base.IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;

        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "attack",
                Title = new LocString("card_keywords", "ui.crazy_ivan.attack_title").GetRawText(),
                Description = new LocString("card_keywords", "ui.crazy_ivan.attack_desc").GetRawText()
                    .Replace("{BombStacks}", bombStacks.ToString()),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new DeployChoiceScreen.ChoiceOption
            {
                Id = "deploy",
                Title = new LocString("card_keywords", "ui.crazy_ivan.deploy_title").GetRawText(),
                Description = new LocString("card_keywords", "ui.crazy_ivan.deploy_desc").GetRawText(),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

        var titleText = new LocString("card_keywords", "ui.crazy_ivan.title").GetRawText();
        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, titleText, options, FactionType.Soviet);

        if (selectedIndex == 0)
        {
            if (play.Target is Creature target)
            {
                var existingBomb = target.Powers.OfType<TimedBombPower>().FirstOrDefault();
                if (existingBomb != null)
                {
                    await PowerCmd.ModifyAmount(ctx, existingBomb, -1m, target, this);
                    GD.Print($"[ChronoIvanCard] 敌人已有定时炸弹，减少1层倒计时");
                }
                else
                    {
                        var bombPower = await PowerCmd.Apply<TimedBombPower>(ctx, target, (decimal)bombStacks, Owner.Creature, this);
                        if (bombPower != null)
                        {
                            bombPower.StartCountdownSound();
                        }
                        GD.Print($"[ChronoIvanCard] 赋予敌人{bombStacks}层定时炸弹");
                    }
            }
        }
        else if (selectedIndex == 1)
        {
            await ShowDeploySelection(ctx);
        }
    }

    private async Task ShowDeploySelection(PlayerChoiceContext ctx)
    {
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();

        HashSet<System.Type> unitTypes = CardUtils.GetUnitTypes();

        var unitCards = handCards.Where(c => unitTypes.Contains(c.GetType())).ToList();

        if (!unitCards.Any())
        {
            GD.Print("[ChronoIvanCard] 手牌中没有单位卡牌，跳过部署");
            return;
        }

        CardModel? selectedCard = await CardSelectionSyncHelper.ShowSelectionWithSync(unitCards, Owner, null, FactionType.Soviet);

        if (selectedCard != null)
        {
            PlayBombPlantSound();

            CardCmd.ApplyKeyword(selectedCard, CardKeyword.Exhaust);

            int vigorAmount = Values.DeployVigor + (IsUpgraded ? Values.DeployVigorUpgraded : 0);

            TimedBombManager.AddTimedBombEffect(selectedCard, vigorAmount);

            GD.Print($"[ChronoIvanCard] 部署：为选中卡牌添加定时炸弹词条（{vigorAmount}活力+消耗）");
        }
    }

    private void PlayCrazyIvanVoice()
    {
        UnitVoiceHelper.PlayUnitVoice(typeof(ChronoIvanCard), "Soviet");
    }

    private static AudioStreamPlayer? _bombPlantAudioPlayer;

    private static void EnsureBombPlantAudioPlayer()
    {
        if (_bombPlantAudioPlayer != null && GodotObject.IsInstanceValid(_bombPlantAudioPlayer))
            return;

        _bombPlantAudioPlayer = new AudioStreamPlayer();
        _bombPlantAudioPlayer.Name = "ChronoIvanBombPlantAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_bombPlantAudioPlayer);
    }

    private void PlayBombPlantSound()
    {
        try
        {
            EnsureBombPlantAudioPlayer();
            if (_bombPlantAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraatta_plant.mp3");
            if (soundFile != null)
            {
                _bombPlantAudioPlayer.Stream = soundFile;
                _bombPlantAudioPlayer.VolumeDb = -5;
                _bombPlantAudioPlayer.Play();
                GD.Print("[ChronoIvanCard] 播放炸弹安装音效");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[ChronoIvanCard] 播放炸弹安装音效失败: {ex.Message}");
        }
    }

    private void PlayChronoMoveSound()
    {
        CommonSoundHelper.PlayChronoMoveSound();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BombStacks"].UpgradeValueBy(Values.DamageUpgraded);
    }
}
