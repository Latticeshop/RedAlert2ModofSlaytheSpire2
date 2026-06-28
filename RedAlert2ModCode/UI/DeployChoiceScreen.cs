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

public sealed partial class DeployChoiceScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private bool _choiceLocked;
    private FactionType _faction = FactionType.Allied;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private DeployChoiceScreen(FactionType faction = FactionType.Allied)
    {
        _faction = faction;
        Name = nameof(DeployChoiceScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
    }

    private string _title = "选择行动";
    private List<ChoiceOption> _options = new();

    private Label? _titleLabel;

    public class ChoiceOption
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconPath { get; set; }
    }

    public static async Task<int?> ShowSelection(string title, List<ChoiceOption> options, FactionType faction = FactionType.Allied)
    {
        var screen = new DeployChoiceScreen(faction);
        screen._title = title;
        screen._options = options;
        screen.BuildUi();
        screen.UpdateUiText();
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
    }

    public static async Task<int?> ShowSelectionWithSync(Player player, string title, List<ChoiceOption> options, FactionType faction = FactionType.Allied)
    {
        int? selectedChoice = null;
        
        object? runManager = GetRunManager();
        if (runManager == null)
        {
            selectedChoice = await ShowSelection(title, options, faction);
            return selectedChoice;
        }

        if (!IsMultiplayerGame(runManager))
        {
            selectedChoice = await ShowSelection(title, options, faction);
            return selectedChoice;
        }

        object? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
        if (synchronizer == null)
        {
            selectedChoice = await ShowSelection(title, options, faction);
            return selectedChoice;
        }

        uint choiceId = ReserveChoiceId(synchronizer, player);
        
        if (IsLocalPlayer(runManager, player))
        {
            selectedChoice = await ShowSelection(title, options, faction);
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

    private static void SyncChoice(object synchronizer, Player player, uint choiceId, int? selectedChoice)
    {
        try
        {
            var choiceResult = new MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult();
            var choiceTypeField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_choiceType", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            var payloadField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_payload", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (choiceTypeField != null)
                choiceTypeField.SetValue(choiceResult, "RedAlert2ModDeployChoice");
            if (payloadField != null)
                payloadField.SetValue(choiceResult, selectedChoice.HasValue ? selectedChoice.Value.ToString() : "-1");
            
            var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
            if (syncMethod != null)
                syncMethod.Invoke(synchronizer, new object[] { player, choiceId, choiceResult });
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DeployChoiceSync] 同步选择失败: {ex}");
        }
    }

    private static async Task<int?> WaitForRemoteChoice(object synchronizer, Player player, uint choiceId)
    {
        try
        {
            TaskCompletionSource<int?> tcs = new();
            EventInfo? eventInfo = synchronizer.GetType().GetEvent("PlayerChoiceReceived");
            
            if (eventInfo != null)
            {
                var handlerInstance = new DeployChoiceHandler(player.NetId, choiceId, tcs);
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
            GD.PrintErr($"[DeployChoiceSync] 等待远程选择失败: {ex}");
        }
        return null;
    }

    private class DeployChoiceHandler
    {
        private readonly ulong _expectedPlayerNetId;
        private readonly uint _expectedChoiceId;
        private readonly TaskCompletionSource<int?> _tcs;

        public DeployChoiceHandler(ulong expectedPlayerNetId, uint expectedChoiceId, TaskCompletionSource<int?> tcs)
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
            
            if (int.TryParse(payload, out int selectedChoice) && selectedChoice >= 0)
                _tcs.TrySetResult(selectedChoice);
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
        _titleLabel.AddThemeColorOverride("font_color", GetPrimaryColor());
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

        for (int i = 0; i < _options.Count; i++)
        {
            choicesRow.AddChild(CreateChoiceButton(i, _options[i]));
        }
    }

    private void UpdateUiText()
    {
        if (_titleLabel != null) _titleLabel.Text = _title;
    }

    private Button CreateChoiceButton(int index, ChoiceOption option)
    {
        Button button = new()
        {
            Name = $"ChoiceButton_{index}",
            CustomMinimumSize = new Vector2(300f, 220f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateCardStyle(GetButtonColor()));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(GetButtonHoverColor()));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(GetSecondaryColor()));

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

        Label titleLabel = new Label()
        {
            Text = option.Title,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", GetPrimaryColor());
        content.AddChild(titleLabel);

        Label descLabel = new Label()
        {
            Text = option.Description,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        content.AddChild(descLabel);

        button.Pressed += () => OnChoiceSelected(index);

        return button;
    }

    private Color GetPrimaryColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.9f, 0.4f, 0.4f),
            FactionType.Yuri => new Color(0.8f, 0.4f, 1f),
            _ => new Color(0.4f, 0.6f, 0.9f)
        };
    }

    private Color GetSecondaryColor()
    {
        return new Color(0.08f, 0.1f, 0.14f, 0.92f);
    }

    private Color GetBorderColor()
    {
        return _faction switch
        {
            FactionType.Soviet => new Color(0.9f, 0.4f, 0.4f),
            FactionType.Yuri => new Color(0.8f, 0.4f, 1f),
            _ => new Color(0.4f, 0.6f, 0.9f)
        };
    }

    private Color GetButtonColor()
    {
        return new Color(0.1f, 0.15f, 0.2f, 0.8f);
    }

    private Color GetButtonHoverColor()
    {
        return new Color(0.15f, 0.2f, 0.28f, 0.9f);
    }

    private void OnChoiceSelected(int index)
    {
        if (_choiceLocked)
            return;

        _choiceLocked = true;
        _completionSource.SetResult(index);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = GetSecondaryColor();
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = GetBorderColor();
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
        style.BorderColor = GetBorderColor();
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
