using System;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Ui.Widgets.TextField;

namespace OpenTemple.Core.Ui;

public class TextEntryUi
{
    [TempleDllLocation(0x1014e8e0)]
    public bool IsVisible => _dialog.IsInTree;

    [TempleDllLocation(0x10bec3a0)]
    private readonly WidgetContainer _dialog;

    [TempleDllLocation(0x10bec7e8)]
    private readonly WidgetButton _okButton;

    [TempleDllLocation(0x10bec8a4)]
    private readonly WidgetButton _cancelButton;

    private readonly WidgetText _titleLabel;

    private readonly TextFieldWidget _textField;

    [TempleDllLocation(0x10bec7c4)]
    private Action<string, bool>? _callback;

    public TextEntryUi()
    {
        var doc = WidgetDoc.Load("ui/text_entry_ui.json");

        _dialog = doc.GetRootContainer();

        _okButton = doc.GetButton("okButton");
        _okButton.AddClickListener(Confirm);
        _cancelButton = doc.GetButton("cancelButton");
        _cancelButton.AddClickListener(Cancel);
        _titleLabel = doc.GetTextContent("titleLabel");
        _textField = doc.GetTextField("input");

        // Hotkeys for confirm / cancel, even if the text box is focused
        _dialog.AddActionHotkey(UiHotkeys.Confirm, Confirm);
        _dialog.AddActionHotkey(UiHotkeys.Cancel, Cancel);
        _textField.AddActionHotkey(UiHotkeys.Confirm, Confirm, () => _textField.HasFocus);
        _textField.AddActionHotkey(UiHotkeys.Cancel, Cancel, () => _textField.HasFocus);
    }

    [TempleDllLocation(0x1014e670)]
    [TempleDllLocation(0x1014e8a0)]
    public void ShowTextEntry(UiCreateNamePacket crNamePkt)
    {
        _textField.Text = crNamePkt.InitialValue ?? "";
        _titleLabel.Text = crNamePkt.DialogTitle ?? "";
        _okButton.Text = crNamePkt.OkButtonLabel ?? "";
        _cancelButton.Text = crNamePkt.CancelButtonLabel ?? "";
        _callback = crNamePkt.Callback;

        Globals.UiManager.AddWindow(_dialog);
        _textField.Focus();

        if (crNamePkt.DialogX != 0 || crNamePkt.DialogY != 0)
        {
            _dialog.X = crNamePkt.DialogX;
            _dialog.Y = crNamePkt.DialogY;
        }
        else
        {
            _dialog.CenterInParent();
        }

        _dialog.BringToFront();
    }

    public void Confirm()
    {
        _callback?.Invoke(_textField.Text, true);
        Reset();
    }

    public void Cancel()
    {
        _callback?.Invoke(_textField.Text, false);
        Reset();
    }

    private void Reset()
    {
        Globals.UiManager.RemoveWindow(_dialog);
        _textField.Text = "";
        _callback = null;
    }
}

public class UiCreateNamePacket
{
    public int DialogX;
    public int DialogY;
    public string? OkButtonLabel;
    public string? CancelButtonLabel;
    public string? DialogTitle;
    public string? InitialValue;
    public Action<string, bool> Callback;
}