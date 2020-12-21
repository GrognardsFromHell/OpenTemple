using System;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;
using Vortice.DirectWrite;

namespace OpenTemple.Core.Ui.Widgets
{
    public interface ITextInputElement
    {
        public void InputText(string text);
    }

    public class WidgetTextInput : WidgetBase, ITextInputElement
    {
        private readonly TextBlock _textBlock = Tig.RenderingDevice.GetTextEngine().CreateTextBlock();

        private readonly StringBuilder _value = new();

        private bool _dirty = true;

        private int _caret;

        private TextStyle _textStyle;

        private TextRange _selection;

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
            AddKeyboardEventListener(SystemEventType.KeyDown, e => HandleShortcut(e.ActiveModifiers, e.Key));

            _textStyle = new TextStyle()
            {
                foreground = Brush.Default,
                fontFace = "OFL Sorts Mill Goudy"
            };
            _textStyle.disableLigatures = true;
            _textBlock.DefaultStyle = _textStyle;
        }

        public string Value
        {
            get => _value.ToString();
            set
            {
                _dirty = true;
                _value.Clear();
                _value.Append(value);
            }
        }

        public override void Render()
        {
            base.Render();

            if (Visible)
            {
                if (_dirty)
                {
                    _dirty = false;
                    _textBlock.SetText(_value.ToString());
                }

                if (IsFocused)
                {
                    _textBlock.ShowCaret(_caret);
                }
                else
                {
                    _textBlock.HideCaret();
                }
                _textBlock.SetSelection(0, _caret);

                var contentRect = GetContentArea();
                _textBlock.Render(contentRect.X, contentRect.Y);
            }
        }

        public void InputText(string text)
        {
            _value.Insert(_caret, text);
            _caret += text.Length;
            _dirty = true;
        }

        private void HandleShortcut(KeyboardModifier modifiers, KeyboardKey key)
        {
            var selecting = (modifiers & KeyboardModifier.Shift) != 0;
            if (selecting)
            {
                modifiers &= ~KeyboardModifier.Shift;
            }

            if (key == KeyboardKey.ArrowLeft)
            {
                _caret--;
                ClampCaret();
            }
            else if (key == KeyboardKey.ArrowRight)
            {
                _caret++;
                ClampCaret();
            }
            else if (key == KeyboardKey.End)
            {
                _caret = _value.Length;
            }
            else if (key == KeyboardKey.Home)
            {
                _caret = 0;
            }
        }

        private void ClampCaret()
        {
            _caret = Math.Clamp(_caret, 0, _value.Length);
        }
    }
}