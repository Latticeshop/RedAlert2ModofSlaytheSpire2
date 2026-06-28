using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace RedAlert2ModCode.UI;

/// <summary>
/// 超时空传送牌堆选择界面
/// </summary>
public sealed partial class ChronoWarpScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private readonly string _prompt;
    private bool _choiceLocked;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    public enum PileChoice
    {
        Draw = 0,
        Hand = 1,
        Discard = 2
    }

    private ChronoWarpScreen(string prompt)
    {
        _prompt = prompt;
        Name = nameof(ChronoWarpScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<int?> ShowPileSelection(string prompt)
    {
        var screen = new ChronoWarpScreen(prompt);
        NOverlayStack.Instance?.Push(screen);
        return await screen._completionSource.Task;
    }

    public static async Task<int?> ShowPileSelectionWithSync(string prompt, Player player)
    {
        int? selectedPile = null;
        
        object? runManager = GetRunManager();
        if (runManager == null)
        {
            selectedPile = await ShowPileSelection(prompt);
            return selectedPile;
        }

        if (!IsMultiplayerGame(runManager))
        {
            selectedPile = await ShowPileSelection(prompt);
            return selectedPile;
        }

        object? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
        if (synchronizer == null)
        {
            selectedPile = await ShowPileSelection(prompt);
            return selectedPile;
        }

        uint choiceId = ReserveChoiceId(synchronizer, player);
        
        if (IsLocalPlayer(runManager, player))
        {
            selectedPile = await ShowPileSelection(prompt);
            SyncChoice(synchronizer, player, choiceId, selectedPile);
            return selectedPile;
        }

        selectedPile = await WaitForRemoteChoice(synchronizer, player, choiceId);
        return selectedPile;
    }

    private static object? GetRunManager()
    {
        try
        {
            var runManagerType = Type.GetType("MegaCrit.Sts2.Core.Runs.RunManager, MegaCrit.Sts2.Core");
            if (runManagerType == null) return null;
            var instanceProp = runManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
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

    private static void SyncChoice(object synchronizer, Player player, uint choiceId, int? selectedPile)
    {
        try
        {
            var choiceResult = new MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult();
            var choiceTypeField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_choiceType", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var payloadField = typeof(MegaCrit.Sts2.Core.GameActions.PlayerChoiceResult).GetField("_payload", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (choiceTypeField != null)
                choiceTypeField.SetValue(choiceResult, "RedAlert2ModChronoWarpSelection");
            if (payloadField != null)
                payloadField.SetValue(choiceResult, selectedPile.HasValue ? selectedPile.Value.ToString() : "-1");
            
            var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
            if (syncMethod != null)
                syncMethod.Invoke(synchronizer, new object[] { player, choiceId, choiceResult });
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ChronoWarpSync] 同步选择失败: {ex}");
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
                var handlerInstance = new ChronoWarpChoiceHandler(player.NetId, choiceId, tcs);
                var handler = System.Delegate.CreateDelegate(eventInfo.EventHandlerType, handlerInstance, "OnReceived");
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
            GD.PrintErr($"[ChronoWarpSync] 等待远程选择失败: {ex}");
        }
        return null;
    }

    private class ChronoWarpChoiceHandler
    {
        private readonly ulong _expectedPlayerNetId;
        private readonly uint _expectedChoiceId;
        private readonly TaskCompletionSource<int?> _tcs;

        public ChronoWarpChoiceHandler(ulong expectedPlayerNetId, uint expectedChoiceId, TaskCompletionSource<int?> tcs)
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
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var payload = payloadField?.GetValue(choiceResult) as string;
            
            if (int.TryParse(payload, out int selectedPile))
                _tcs.TrySetResult(selectedPile);
            else
                _tcs.TrySetResult(null);
        }
    }

    private void BuildUi()
    {
        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0.02f, 0.025f, 0.035f, 0.85f),
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
            CustomMinimumSize = new Vector2(800f, 350f)
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

        Label title = new()
        {
            Text = _prompt,
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
        root.AddChild(title);

        HBoxContainer choicesRow = new()
        {
            Name = "ChoicesRow",
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        choicesRow.AddThemeConstantOverride("separation", 20);
        root.AddChild(choicesRow);

        choicesRow.AddChild(CreatePileButton((int)PileChoice.Draw, "摸牌堆"));
        choicesRow.AddChild(CreatePileButton((int)PileChoice.Hand, "手牌"));
        choicesRow.AddChild(CreatePileButton((int)PileChoice.Discard, "弃牌堆"));
    }

    private Button CreatePileButton(int pileChoice, string label)
    {
        Button button = new()
        {
            Name = $"PileButton_{pileChoice}",
            Text = label,
            CustomMinimumSize = new Vector2(200f, 80f),
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
        };

        button.AddThemeStyleboxOverride("normal", CreateButtonStyle(new Color(0.15f, 0.22f, 0.35f)));
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(new Color(0.2f, 0.3f, 0.45f)));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(new Color(0.1f, 0.18f, 0.28f)));
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1f));
        button.Pressed += () => OnPileSelected(pileChoice);

        return button;
    }

    private void OnPileSelected(int pileChoice)
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(pileChoice);
        NOverlayStack.Instance?.Remove(this);
    }

    private StyleBoxFlat CreatePanelStyle()
    {
        StyleBoxFlat style = new();
        style.BgColor = new Color(0.08f, 0.1f, 0.14f, 0.92f);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        return style;
    }

    private StyleBoxFlat CreateButtonStyle(Color bgColor)
    {
        StyleBoxFlat style = new();
        style.BgColor = bgColor;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        return style;
    }

    public void AfterOverlayOpened() { Visible = true; }
    public void AfterOverlayClosed() { QueueFree(); }
    public void AfterOverlayShown() { Visible = true; }
    public void AfterOverlayHidden() { Visible = false; }

    public override void _ExitTree()
    {
        _completionSource.TrySetResult(null);
        base._ExitTree();
    }
}
