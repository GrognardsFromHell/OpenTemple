using System;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;
using Vortice.DirectWrite;
using Vortice.Mathematics;

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

        private int Caret
        {
            get => _caret;
            set
            {
                _caret = Math.Clamp(value, 0, _value.Length);
                EnsureCaretVisibility();
            }
        }

        private TextStyle _textStyle;

        /// <summary>
        /// The selection position. If nothing is selected, this equals the caret position.
        /// This may be before or after the caret, depending on the direction of the selection.
        /// </summary>
        private int _selectionPos;

        /// <summary>
        /// Horizontal scrolling is applied if the text is larger than the area of the input field,
        /// and the caret is to the right of that visible area.
        /// </summary>
        private float _horizontalScroll;

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
            AddEventListener<KeyboardEvent>(SystemEventType.KeyDown, e => HandleShortcut(e.ActiveModifiers, e.Key));
            AddEventListener<MouseEvent>(SystemEventType.MouseDown, HandleMouseDown);
            // Reset horizontal scrolling on blur
            AddEventListener(SystemEventType.Blur, evt => _horizontalScroll = 0);

            _textStyle = new TextStyle()
            {
                foreground = Brush.Default,
                fontFace = "OFL Sorts Mill Goudy"
            };
            _textStyle.disableLigatures = true;
            _textBlock.DefaultStyle = _textStyle;

            SetSize(100, 20);
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

        private (int, int) SelectionRange
        {
            get
            {
                var start = Math.Min(Caret, _selectionPos);
                var length = Math.Abs(Caret - _selectionPos);
                return (start, length);
            }
        }

        public override void Render()
        {
            base.Render();

            if (!Visible)
            {
                return;
            }

            if (_dirty)
            {
                _dirty = false;
                _textBlock.SetText(_value.ToString());
                if (_selectionPos == _caret)
                {
                    _textBlock.ClearSelection();
                }
                else
                {
                    var (start, length) = SelectionRange;
                    _textBlock.SetSelection(start, length);
                }
            }

            var contentRect = GetContentArea();
            var outline = contentRect;
            outline.Inflate(1, 1);
            Tig.ShapeRenderer2d.DrawRectangleOutline(outline, new PackedLinearColorA(127, 127, 127, 255));

            if (IsFocused)
            {
                _textBlock.ShowCaret(_caret);
            }
            else
            {
                _textBlock.HideCaret();
            }

            var clipRect = new RectangleF(contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height);
            _textBlock.Render(MathF.Round(clipRect.X - _horizontalScroll), clipRect.Y, clipRect);
        }

        public void InputText(string text)
        {
            _value.Insert(Caret, text);
            _textBlock.SetText(_value.ToString());
            MoveCaret(Caret + text.Length, false);
            _dirty = true;
        }

        private void HandleMouseDown(MouseEvent evt)
        {
            var rect = GetContentArea();
            var x = evt.ClientX - rect.X;
            var y = evt.ClientY - rect.Y;

            _textBlock.HitTest(x, y, out var position);
            MoveCaret(position, evt.ShiftKey);
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
                MoveCaret(Caret - 1, selecting);
            }
            else if (key == KeyboardKey.ArrowRight)
            {
                MoveCaret(Caret + 1, selecting);
            }
            else if (key == KeyboardKey.End)
            {
                MoveCaret(_value.Length, selecting);
            }
            else if (key == KeyboardKey.Home)
            {
                MoveCaret(0, selecting);
            }
            else if (key == KeyboardKey.A && modifiers == KeyboardModifier.Control)
            {
                _selectionPos = 0;
                MoveCaret(_value.Length, true);
            }
        }

        private void MoveCaret(int position, bool selecting)
        {
            if (selecting)
            {
                Caret = position;
            }
            else
            {
                Caret = position;
                _selectionPos = Caret;
            }

            _dirty = true;
        }

        private void EnsureCaretVisibility()
        {
            if (!IsFocused)
            {
                _horizontalScroll = 0;
                return;
            }

            var caretRect = _textBlock.GetCaretRectangle(0, 0, _caret);
            // Caret distance from the left or right edge of the input
            var rightDistance = caretRect.Right - _horizontalScroll - Width;
            var leftDistance = -(caretRect.Left - _horizontalScroll);
            if (rightDistance >= 0 && leftDistance <= 0)
            {
                // Caret is to the right
                _horizontalScroll += rightDistance + Width / 3.0f;
            }
            else if (leftDistance > 0 && rightDistance <= 0)
            {
                // Caret is to the left
                _horizontalScroll -= leftDistance + Width / 3.0f;
            }

            // Ensure the scrolling is capped
            var textWidth = MathF.Ceiling(_textBlock.BoundingBox.Width + caretRect.Width);
            var overhang = Math.Max(0, textWidth - Width);
            _horizontalScroll = Math.Clamp(_horizontalScroll, 0, overhang);

        }
    }
}