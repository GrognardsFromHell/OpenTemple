using System;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using static SDL2.SDL;

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

    private readonly WidgetText _currentInputLabel;

    [TempleDllLocation(0x10bec960)]
    private string _currentInput;

    [TempleDllLocation(0x10bec7c4)]
    private Action<string, bool> _callback;

    [TempleDllLocation(0x10bec768)]
    private int _caretPosition;

    public TextEntryUi()
    {
        var doc = WidgetDoc.Load("ui/text_entry_ui.json");

        _dialog = doc.GetRootContainer();
        Globals.UiManager.Root.OnKeyUp += e =>
        {
            if (_dialog.Visible && HandleKeyUp(e))
            {
                e.StopImmediatePropagation();
            }
        };
        _dialog.OnTextInput += HandleTextInput;

        _okButton = doc.GetButton("okButton");
        _okButton.AddClickListener(Confirm);
        _cancelButton = doc.GetButton("cancelButton");
        _cancelButton.AddClickListener(Cancel);
        _titleLabel = doc.GetTextContent("titleLabel");
        _currentInputLabel = doc.GetTextContent("currentInputLabel");
    }

    private void HandleTextInput(TextInputEvent e)
    {
        var newText = _currentInput.Insert(_caretPosition, e.Text);
        _caretPosition += e.Text.Length;
        UpdateInput(newText);
    }

    private bool HandleKeyUp(KeyboardEvent e)
    {
        switch (e.VirtualKey)
        {
            case SDL_Keycode.SDLK_RETURN:
                Confirm();
                return true;
            case SDL_Keycode.SDLK_ESCAPE:
                Cancel();
                return true;
            default:
                return false;
        }
    }

    [TempleDllLocation(0x1014e670)]
    [TempleDllLocation(0x1014e8a0)]
    public void ShowTextEntry(UiCreateNamePacket crNamePkt)
    {
        UpdateInput(crNamePkt.InitialValue ?? "");
        _titleLabel.Text = crNamePkt.DialogTitle ?? "";
        _okButton.Text = crNamePkt.OkButtonLabel ?? "";
        _cancelButton.Text = crNamePkt.CancelButtonLabel ?? "";
        _callback = crNamePkt.Callback;

        Globals.UiManager.AddWindow(_dialog);

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

    [TempleDllLocation(0x1014e900)]
    private void UpdateInput(string text)
    {
        _currentInput = text;
        _caretPosition = Math.Clamp(_caretPosition, 0, _currentInput.Length);

        // Insert the caret
        var displayedText = text.Insert(_caretPosition, "|");
        _currentInputLabel.Text = displayedText;

        // This is _incredibly_ bad, but it's what vanilla ToEE did :-(
        while (_currentInputLabel.GetPreferredSize().Width >= _currentInputLabel.FixedWidth)
        {
            displayedText = displayedText.Substring(1);
            _currentInputLabel.Text = displayedText;
        }
    }

    public void Confirm()
    {
        _callback?.Invoke(_currentInput, true);
        Reset();
    }

    public void Cancel()
    {
        _callback?.Invoke(_currentInput, false);
        Reset();
    }

    private void Reset()
    {
        Globals.UiManager.RemoveWindow(_dialog);
        _currentInput = "";
        _callback = null;
        _caretPosition = 0;
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