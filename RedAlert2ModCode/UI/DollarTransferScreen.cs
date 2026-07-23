#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

public sealed partial class DollarTransferScreen : Control, IOverlayScreen
{
    private readonly TaskCompletionSource<int?> _completionSource = new();
    private bool _choiceLocked;
    private Player _sender;
    private List<Player> _targets = new();
    private int _selectedAmount = 1000;
    private int _maxAmount = 0;
    private LineEdit? _amountInput;
    private Label? _errorLabel;

    public NetScreenType ScreenType => NetScreenType.Rewards;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => null;

    private DollarTransferScreen(Player sender)
    {
        _sender = sender;
        Name = nameof(DollarTransferScreen);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = Control.FocusModeEnum.All;
        BuildUi();
    }

    public static async Task<int?> ShowTransferScreen(Player sender)
    {
        DollarTransferManager.ResetTransferLock();

        var screen = new DollarTransferScreen(sender);
        NOverlayStack.Instance?.Push(screen);

        if (!MultiplayerSyncHelper.IsLocalPlayer(sender))
        {
            screen.Close();
            return null;
        }

        return await screen._completionSource.Task;
    }

    public void Close()
    {
        if (_choiceLocked) return;
        _choiceLocked = true;
        _completionSource.TrySetResult(null);
        NOverlayStack.Instance?.Remove(this);
        QueueFree();
    }

    private void BuildUi()
    {
        _targets = DollarTransferManager.GetValidTargets(_sender).ToList();
        _maxAmount = DollarTransferManager.GetSenderBalance(_sender);

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
            CustomMinimumSize = new Vector2(700f, 450f)
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
            Text = new LocString("card_keywords", "ui.dollar_transfer.title").GetRawText(),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 26);
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 0.4f));
        root.AddChild(title);

        Label balanceLabel = new()
        {
            Text = $"{new LocString("card_keywords", "ui.dollar_transfer.balance").GetRawText()}: {_maxAmount}",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        balanceLabel.AddThemeFontSizeOverride("font_size", 18);
        balanceLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        root.AddChild(balanceLabel);

        VBoxContainer amountSection = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        amountSection.AddThemeConstantOverride("separation", 10);
        root.AddChild(amountSection);

        Label amountLabel = new()
        {
            Text = $"{new LocString("card_keywords", "ui.dollar_transfer.amount").GetRawText()}:",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        amountLabel.AddThemeFontSizeOverride("font_size", 16);
        amountLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        amountSection.AddChild(amountLabel);

        HBoxContainer amountRow = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        amountRow.AddThemeConstantOverride("separation", 10);
        amountSection.AddChild(amountRow);

        Button minusBtn = new()
        {
            Text = "-",
            CustomMinimumSize = new Vector2(40f, 40f),
            FocusMode = Control.FocusModeEnum.All
        };
        minusBtn.AddThemeFontSizeOverride("font_size", 24);
        minusBtn.Pressed += () => AdjustAmount(-1000);
        amountRow.AddChild(minusBtn);

        _amountInput = new LineEdit()
        {
            Text = _selectedAmount.ToString(),
            CustomMinimumSize = new Vector2(150f, 40f),
            FocusMode = Control.FocusModeEnum.All
        };
        _amountInput.AddThemeConstantOverride("horizontal_alignment", (int)HorizontalAlignment.Center);
        _amountInput.AddThemeFontSizeOverride("font_size", 20);
        _amountInput.TextChanged += (text) => OnAmountInputChanged(text);
        amountRow.AddChild(_amountInput);

        Button plusBtn = new()
        {
            Text = "+",
            CustomMinimumSize = new Vector2(40f, 40f),
            FocusMode = Control.FocusModeEnum.All
        };
        plusBtn.AddThemeFontSizeOverride("font_size", 24);
        plusBtn.Pressed += () => AdjustAmount(1000);
        amountRow.AddChild(plusBtn);

        VBoxContainer targetSection = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        targetSection.AddThemeConstantOverride("separation", 10);
        root.AddChild(targetSection);

        Label targetLabel = new()
        {
            Text = $"{new LocString("card_keywords", "ui.dollar_transfer.recipient").GetRawText()}:",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        targetLabel.AddThemeFontSizeOverride("font_size", 16);
        targetLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.75f, 0.85f));
        targetSection.AddChild(targetLabel);

        HBoxContainer targetRow = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        targetRow.AddThemeConstantOverride("separation", 20);
        targetSection.AddChild(targetRow);

        if (_targets.Count == 0)
        {
            Label noTarget = new()
            {
                Text = new LocString("card_keywords", "ui.dollar_transfer.no_target").GetRawText(),
                HorizontalAlignment = HorizontalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            noTarget.AddThemeFontSizeOverride("font_size", 16);
            noTarget.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 0.4f));
            targetRow.AddChild(noTarget);
        }
        else
        {
            for (int i = 0; i < _targets.Count; i++)
            {
                int index = i;
                Player target = _targets[i];

                string playerName = PlatformUtil.GetPlayerNameRaw(RunManager.Instance.NetService.Platform, target.NetId);
                Button targetBtn = new()
                {
                    Text = playerName ?? target.Character?.GetType().Name ?? "Unknown",
                    CustomMinimumSize = new Vector2(150f, 50f),
                    FocusMode = Control.FocusModeEnum.All,
                    MouseDefaultCursorShape = Control.CursorShape.PointingHand
                };
                targetBtn.AddThemeFontSizeOverride("font_size", 18);
                targetBtn.Pressed += () => OnTargetSelected(index);
                targetRow.AddChild(targetBtn);
            }
        }

        _errorLabel = new Label()
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Visible = false
        };
        _errorLabel.AddThemeFontSizeOverride("font_size", 16);
        _errorLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f));
        root.AddChild(_errorLabel);

        Button cancelBtn = new()
        {
            Text = new LocString("card_keywords", "ui.dollar_transfer.cancel").GetRawText(),
            CustomMinimumSize = new Vector2(120f, 45f),
            FocusMode = Control.FocusModeEnum.All,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand
        };
        cancelBtn.AddThemeFontSizeOverride("font_size", 16);
        cancelBtn.Pressed += Close;
        root.AddChild(cancelBtn);
    }

    private void AdjustAmount(int delta)
    {
        int newAmount = _selectedAmount + delta;
        SetAmount(newAmount);
    }

    private void SetAmount(int amount)
    {
        _selectedAmount = Math.Max(1000, Math.Min(amount, _maxAmount));
        _selectedAmount = _selectedAmount - (_selectedAmount % 1000);

        if (_amountInput != null)
        {
            _amountInput.Text = _selectedAmount.ToString();
        }
    }

    private void OnAmountInputChanged(string text)
    {
        if (int.TryParse(text, out int amount))
        {
            _selectedAmount = Math.Max(0, Math.Min(amount, _maxAmount));
        }
    }

    private void OnTargetSelected(int targetIndex)
    {
        if (_choiceLocked) return;
        if (targetIndex < 0 || targetIndex >= _targets.Count) return;
        if (_selectedAmount <= 0) return;

        Player receiver = _targets[targetIndex];
        bool success = DollarTransferManager.ExecuteTransfer(_sender, receiver, _selectedAmount);

        if (success)
        {
            _choiceLocked = true;
            GD.Print($"[DollarTransfer] 转账请求已发送: {_sender.Character?.GetType().Name} -> {receiver.Character?.GetType().Name}, {_selectedAmount}");
            _completionSource.SetResult(targetIndex);
            NOverlayStack.Instance?.Remove(this);
            QueueFree();
        }
        else
        {
            ShowError(new LocString("card_keywords", "ui.dollar_transfer.failed").GetRawText());
            GD.Print($"[DollarTransfer] 转账请求失败: {_sender.Character?.GetType().Name} -> {receiver.Character?.GetType().Name}, {_selectedAmount}");
        }
    }

    private void ShowError(string message)
    {
        if (_errorLabel != null)
        {
            _errorLabel.Text = message;
            _errorLabel.Visible = true;
        }
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
        style.BorderColor = new Color(0.3f, 0.6f, 0.3f);
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