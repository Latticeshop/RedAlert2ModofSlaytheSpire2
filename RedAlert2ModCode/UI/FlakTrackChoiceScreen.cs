#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class FlakTrackChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<ChoiceType?> _completionSource = new();
    private bool _choiceLocked;

    public enum ChoiceType
    {
        Deploy,
        Attack
    }

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private FlakTrackChoiceScreen()
    {
        Name = nameof(FlakTrackChoiceScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
    }

    private string _title = "选择行动";
    private string _deployTitle = "部署";
    private string _deployDesc = "存储当前手牌中的士兵单位";
    private string _attackTitle = "攻击";
    private string _attackDesc = "获得敏捷和攻击";

    private Label? _titleLabel;
    private Label? _deployTitleLabel;
    private Label? _deployDescLabel;
    private Label? _attackTitleLabel;
    private Label? _attackDescLabel;

    public static async Task<ChoiceType?> ShowSelection()
    {
        var screen = new FlakTrackChoiceScreen();
        screen.BuildUi();
        screen.UpdateUiText();
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
    }

    public static async Task<ChoiceType?> ShowSelection(string title, string deployTitle, string deployDesc, string attackTitle, string attackDesc)
    {
        var screen = new FlakTrackChoiceScreen();
        screen._title = title;
        screen._deployTitle = deployTitle;
        screen._deployDesc = deployDesc;
        screen._attackTitle = attackTitle;
        screen._attackDesc = attackDesc;
        screen.BuildUi();
        screen.UpdateUiText();
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
    }

    public static async Task<ChoiceType?> ShowSelectionWithSync(Player player)
    {
        return await ShowSelectionWithSync(player, null, null, null, null, null);
    }

    public static async Task<ChoiceType?> ShowSelectionWithSync(Player player, string title, string deployTitle, string deployDesc, string attackTitle, string attackDesc)
    {
        ChoiceType? selectedChoice = null;
        
        object? runManager = GetRunManager();
        if (runManager == null)
        {
            selectedChoice = string.IsNullOrEmpty(title) 
                ? await ShowSelection() 
                : await ShowSelection(title, deployTitle, deployDesc, attackTitle, attackDesc);
            return selectedChoice;
        }

        if (!IsMultiplayerGame(runManager))
        {
            selectedChoice = string.IsNullOrEmpty(title) 
                ? await ShowSelection() 
                : await ShowSelection(title, deployTitle, deployDesc, attackTitle, attackDesc);
            return selectedChoice;
        }

        object? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
        if (synchronizer == null)
        {
            selectedChoice = string.IsNullOrEmpty(title) 
                ? await ShowSelection() 
                : await ShowSelection(title, deployTitle, deployDesc, attackTitle, attackDesc);
            return selectedChoice;
        }

        uint choiceId = ReserveChoiceId(synchronizer, player);
        
        if (IsLocalPlayer(runManager, player))
        {
            selectedChoice = string.IsNullOrEmpty(title) 
                ? await ShowSelection() 
                : await ShowSelection(title, deployTitle, deployDesc, attackTitle, attackDesc);
            SyncChoice(synchronizer, player, choiceId, selectedChoice);
            return selectedChoice;
        }

        selectedChoice = await WaitForRemoteChoice(synchronizer, player, choiceId);
        return selectedChoice;
    }

    private static object? GetRunManager()
    {
        try
        {
            var runManagerType = Type.GetType("MegaCrit.Sts2.Core.Runs.RunManager, MegaCrit.Sts2.Core");
            if (runManagerType == null) return null;
            var instanceProp = runManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProp == null) return null;
            return instanceProp.GetValue(null);
        }
        catch { return null; }
    }

    private static bool IsMultiplayerGame(object runManager)
    {
        try
        {
            var netServiceProp = runManager.GetType().GetProperty("NetService");
            if (netServiceProp == null) return false;
            var netService = netServiceProp.GetValue(runManager);
            if (netService == null) return false;
            var typeProp = netService.GetType().GetProperty("Type");
            if (typeProp == null) return false;
            var netType = typeProp.GetValue(netService);
            if (netType == null) return false;
            string typeName = netType.ToString();
            return typeName == "Host" || typeName == "Client";
        }
        catch { return false; }
    }

    private static async Task<object?> WaitForPlayerChoiceSynchronizerAsync(object runManager)
    {
        try
        {
            for (int i = 0; i < 60; i++)
            {
                var synchronizerProp = runManager.GetType().GetProperty("PlayerChoiceSynchronizer");
                if (synchronizerProp != null)
                {
                    var synchronizer = synchronizerProp.GetValue(runManager);
                    if (synchronizer != null) return synchronizer;
                }
                await Task.Yield();
            }
            var finalProp = runManager.GetType().GetProperty("PlayerChoiceSynchronizer");
            if (finalProp != null) return finalProp.GetValue(runManager);
        }
        catch { }
        return null;
    }

    private static bool IsLocalPlayer(object runManager, Player player)
    {
        try
        {
            var netServiceProp = runManager.GetType().GetProperty("NetService");
            if (netServiceProp == null) return true;
            var netService = netServiceProp.GetValue(runManager);
            if (netService == null) return true;
            var serviceNetIdProp = netService.GetType().GetProperty("NetId");
            if (serviceNetIdProp == null) return true;
            ulong serviceNetId = (ulong)serviceNetIdProp.GetValue(netService);
            return player.NetId != 0UL && player.NetId == serviceNetId;
        }
        catch { return true; }
    }

    private static uint ReserveChoiceId(object synchronizer, Player player)
    {
        try
        {
            var reserveMethod = synchronizer.GetType().GetMethod("ReserveChoiceId");
            if (reserveMethod != null)
                return (uint)reserveMethod.Invoke(synchronizer, new object[] { player });
        }
        catch { }
        return uint.MaxValue;
    }

    private static void SyncChoice(object synchronizer, Player player, uint choiceId, ChoiceType? selectedChoice)
    {
        try
        {
            var choiceResult = new MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult();
            var choiceTypeField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_choiceType", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            var payloadField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_payload", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (choiceTypeField != null)
                choiceTypeField.SetValue(choiceResult, "RedAlert2ModFlakTrackChoice");
            if (payloadField != null)
                payloadField.SetValue(choiceResult, selectedChoice.HasValue ? ((int)selectedChoice.Value).ToString() : "-1");
            
            var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
            if (syncMethod != null)
                syncMethod.Invoke(synchronizer, new object[] { player, choiceId, choiceResult });
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[FlakTrackSync] 同步选择失败: {ex}");
        }
    }

    private static async Task<ChoiceType?> WaitForRemoteChoice(object synchronizer, Player player, uint choiceId)
    {
        try
        {
            TaskCompletionSource<ChoiceType?> tcs = new();
            EventInfo? eventInfo = synchronizer.GetType().GetEvent("PlayerChoiceReceived");
            
            if (eventInfo != null)
            {
                var handlerInstance = new FlakTrackChoiceHandler(player.NetId, choiceId, tcs);
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, handlerInstance, "OnReceived");
                eventInfo.AddEventHandler(synchronizer, handler);
                
                try
                {
                    Task waitTask = tcs.Task;
                    Task timeout = Task.Delay(30000);
                    if (await Task.WhenAny(waitTask, timeout) != waitTask)
                        return null;
                    return await tcs.Task;
                }
                finally
                {
                    eventInfo.RemoveEventHandler(synchronizer, handler);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[FlakTrackSync] 等待远程选择失败: {ex}");
        }
        return null;
    }

    private class FlakTrackChoiceHandler
    {
        private readonly ulong _expectedPlayerNetId;
        private readonly uint _expectedChoiceId;
        private readonly TaskCompletionSource<ChoiceType?> _tcs;

        public FlakTrackChoiceHandler(ulong expectedPlayerNetId, uint expectedChoiceId, TaskCompletionSource<ChoiceType?> tcs)
        {
            _expectedPlayerNetId = expectedPlayerNetId;
            _expectedChoiceId = expectedChoiceId;
            _tcs = tcs;
        }

        public void OnReceived(object receivedPlayer, uint receivedChoiceId, NetPlayerChoiceResult result)
        {
            if (receivedPlayer is not Player p) return;
            if (p.NetId != _expectedPlayerNetId) return;
            if (receivedChoiceId != _expectedChoiceId) return;

            var choiceResult = MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult.FromNetData(
                p, p.RunState, result);
            
            var payloadField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_payload",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var payload = payloadField?.GetValue(choiceResult) as string;
            
            if (int.TryParse(payload, out int selectedChoice) && selectedChoice >= 0 && selectedChoice < Enum.GetValues(typeof(ChoiceType)).Length)
                _tcs.TrySetResult((ChoiceType)selectedChoice);
            else
                _tcs.TrySetResult(null);
        }
    }

    private void BuildUi()
    {
        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0.02f, 0.025f, 0.035f, 0.8f),
            MouseFilter = MouseFilterEnum.Stop
        };
        backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        CenterContainer center = new() { Name = "Center" };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        PanelContainer panel = new()
        {
            Name = "ContentPanel",
            CustomMinimumSize = new Vector2(800f, 380f)
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        center.AddChild(panel);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 30);
        margin.AddThemeConstantOverride("margin_right", 30);
        margin.AddThemeConstantOverride("margin_top", 30);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        VBoxContainer root = new() { Alignment = BoxContainer.AlignmentMode.Center };
        root.AddThemeConstantOverride("separation", 20);
        margin.AddChild(root);

        _titleLabel = new Label()
        {
            Text = _title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 26);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(_titleLabel);

        HBoxContainer choicesRow = new()
        {
            Name = "ChoicesRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        choicesRow.AddThemeConstantOverride("separation", 30);
        root.AddChild(choicesRow);

        choicesRow.AddChild(CreateChoiceButton(ChoiceType.Deploy, out _deployTitleLabel, out _deployDescLabel));
        choicesRow.AddChild(CreateChoiceButton(ChoiceType.Attack, out _attackTitleLabel, out _attackDescLabel));
    }

    private void UpdateUiText()
    {
        if (_titleLabel != null) _titleLabel.Text = _title;
        if (_deployTitleLabel != null) _deployTitleLabel.Text = _deployTitle;
        if (_deployDescLabel != null) _deployDescLabel.Text = _deployDesc;
        if (_attackTitleLabel != null) _attackTitleLabel.Text = _attackTitle;
        if (_attackDescLabel != null) _attackDescLabel.Text = _attackDesc;
    }

    private Button CreateChoiceButton(ChoiceType type, out Label titleLabel, out Label descLabel)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{type}",
            CustomMinimumSize = new Vector2(300f, 220f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(new Color(0.12f, 0.18f, 0.28f, 0.9f)));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(new Color(0.18f, 0.26f, 0.4f, 0.95f)));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(new Color(0.1f, 0.14f, 0.22f, 0.98f)));

        MarginContainer contentMargin = new();
        contentMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        contentMargin.AddThemeConstantOverride("margin_left", 15);
        contentMargin.AddThemeConstantOverride("margin_right", 15);
        contentMargin.AddThemeConstantOverride("margin_top", 15);
        contentMargin.AddThemeConstantOverride("margin_bottom", 15);
        button.AddChild(contentMargin);

        VBoxContainer content = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        content.AddThemeConstantOverride("separation", 8);
        contentMargin.AddChild(content);

        titleLabel = new Label()
        {
            Text = type == ChoiceType.Deploy ? _deployTitle : _attackTitle,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        content.AddChild(titleLabel);

        descLabel = new Label()
        {
            Text = type == ChoiceType.Deploy ? _deployDesc : _attackDesc,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        content.AddChild(descLabel);

        button.Pressed += () => OnChoiceSelected(type);

        return button;
    }

    private void OnChoiceSelected(ChoiceType type)
    {
        if (_choiceLocked)
            return;

        _choiceLocked = true;
        _completionSource.SetResult(type);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = FactionHelper.GetFactionBorderColor();
        return style;
    }

    private StyleBoxFlat CreateCardStyle(Color bgColor)
    {
        StyleBoxFlat style = new();
        style.BgColor = bgColor;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = FactionHelper.GetFactionBorderColor();
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        return style;
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }
}