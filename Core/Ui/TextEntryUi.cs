using System;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class TextEntryUi
    {
        [TempleDllLocation(0x1014e8e0)]
        public bool IsVisible => _dialog.IsInTree();

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

            _dialog = doc.TakeRootContainer();
            _dialog.SetKeyStateChangeHandler(HandleShortcut);
            _dialog.OnHandleMessage += HandleMessage;

            _okButton = doc.GetButton("okButton");
            _okButton.SetClickHandler(Confirm);
            _cancelButton = doc.GetButton("cancelButton");
            _cancelButton.SetClickHandler(Cancel);
            _titleLabel = doc.GetTextContent("titleLabel");
            _currentInputLabel = doc.GetTextContent("currentInputLabel");
        }

        private bool HandleShortcut(MessageKeyStateChangeArgs arg)
        {
            if (arg.down)
            {
                // We handle these on key-down because we are interested in key-repeats
                switch (arg.key)
                {
                    case DIK.DIK_LEFT:
                    case DIK.DIK_NUMPAD4:
                        if (--_caretPosition < 0)
                        {
                            _caretPosition = 0;
                        }

                        UpdateInput(_currentInput);
                        break;
                    case DIK.DIK_RIGHT:
                    case DIK.DIK_NUMPAD6:
                        if (++_caretPosition > _currentInput.Length)
                        {
                            _caretPosition = _currentInput.Length;
                        }
                        UpdateInput(_currentInput);
                        break;
                    case DIK.DIK_HOME:
                        _caretPosition = 0;
                        UpdateInput(_currentInput);
                        break;
                    case DIK.DIK_END:
                        _caretPosition = _currentInput.Length;
                        UpdateInput(_currentInput);
                        break;
                    case DIK.DIK_DELETE:
                    case DIK.DIK_DECIMAL:
                        if (_caretPosition < _currentInput.Length)
                        {
                            UpdateInput(_currentInput.Remove(_caretPosition, 1));
                        }

                        break;
                    case DIK.DIK_BACKSPACE:
                        if (_caretPosition > 0)
                        {
                            --_caretPosition;
                            UpdateInput(_currentInput.Remove(_caretPosition, 1));
                        }

                        break;
                }
            }
            else
            {
                switch (arg.key)
                {
                    case DIK.DIK_RETURN:
                        Confirm();
                        break;
                    case DIK.DIK_ESCAPE:
                        Cancel();
                        break;
                }
            }

            return true;
        }

        private bool HandleMessage(Message arg)
        {
            if (arg.type == MessageType.CHAR)
            {
                if (!char.IsControl(arg.CharArgs.Character))
                {
                    var newText = _currentInput.Insert(_caretPosition++, arg.CharArgs.Character.ToString());
                    UpdateInput(newText);
                }

                return true;
            }

            return false;
        }

        [TempleDllLocation(0x1014e670)]
        [TempleDllLocation(0x1014e8a0)]
        public void ShowTextEntry(UiCreateNamePacket crNamePkt)
        {
            _dialog.SetPos(crNamePkt.DialogX, crNamePkt.DialogY);

            UpdateInput(crNamePkt.InitialValue ?? "");
            _titleLabel.SetText(crNamePkt.DialogTitle ?? "");
            _okButton.SetText(crNamePkt.OkButtonLabel ?? "");
            _cancelButton.SetText(crNamePkt.CancelButtonLabel ?? "");
            _callback = crNamePkt.Callback;

            if (crNamePkt.DialogX != 0 || crNamePkt.DialogY != 0)
            {
                _dialog.X = crNamePkt.DialogX;
                _dialog.Y = crNamePkt.DialogY;
            }
            else
            {
                _dialog.CenterOnScreen();
            }

            Globals.UiManager.RootElement.Append(_dialog);
            _dialog.BringToFront();
        }

        [TempleDllLocation(0x1014e900)]
        private void UpdateInput(string text)
        {
            _currentInput = text;
            _caretPosition = Math.Clamp(_caretPosition, 0, _currentInput.Length);

            // Insert the caret
            var displayedText = text.Insert(_caretPosition, "|");
            _currentInputLabel.SetText(displayedText);

            // This is _incredibly_ bad, but it's what vanilla ToEE did :-(
            while (_currentInputLabel.GetPreferredSize().Width >= _currentInputLabel.FixedWidth)
            {
                displayedText = displayedText.Substring(1);
                _currentInputLabel.SetText(displayedText);
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
            _dialog.Remove();
            _currentInput = "";
            _callback = null;
            _caretPosition = 0;
        }
    }

    public class UiCreateNamePacket
    {
        public float DialogX;
        public float DialogY;
        public string OkButtonLabel;
        public string CancelButtonLabel;
        public string DialogTitle;
        public string InitialValue;
        public Action<string, bool> Callback;
    }

}