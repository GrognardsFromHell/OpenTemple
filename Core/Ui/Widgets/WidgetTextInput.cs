using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace OpenTemple.Core.Ui.Widgets
{

    public enum SelectionDirection
    {
        Forward,
        Backward
    }

    public enum SelectionMode
    {
        /// <summary>
        /// Make the newly inserted text the selected text.
        /// </summary>
        Select,
        /// <summary>
        /// Put the caret at the  start of the inserted text.
        /// </summary>
        Start,
        /// <summary>
        /// Put the caret at the end of the inserted text.
        /// </summary>
        End,
        Preserve
    }

    /// <summary>
    /// Roughly modeled after https://html.spec.whatwg.org/#textFieldSelection
    /// </summary>
    public interface ITextInputElement
    {
        int SelectionStart { get; set; }

        int SelectionEnd { get; set; }

        SelectionDirection SelectionDirection { get; set; }

        void SetSelectionRange(int start, int end, SelectionDirection direction = default);

        void SetRangeText(string replacement);

        void SetRangeText(string replacement, int start, int end);

        void SetRangeText(string replacement, int start, int end, SelectMode selectionMode);
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

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
            AddEventListener<KeyboardEvent>(SystemEventType.KeyDown, HandleShortcut);
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

        private void HandleShortcut(KeyboardEvent evt)
        {
            if (EditCommandHandler.TryGetEditCommand(evt, out var command))
            {
                ExecuteCommand(command);
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

        public bool ExecuteCommand(EditCommand command)
        {
            switch (command)
            {
                case EditCommand.DeleteNextCharacter:

                    break;
                case EditCommand.DeletePreviousCharacter:
                    break;
                case EditCommand.DeleteNextWord:
                    break;
                case EditCommand.DeletePreviousWord:
                    break;
                case EditCommand.MoveForwardByCharacter:
                    MoveCaret(Caret + 1, false);
                    break;
                case EditCommand.MoveBackwardsByCharacter:
                    MoveCaret(Caret - 1, false);
                    break;
                case EditCommand.MoveSelectionForwardByCharacter:
                    MoveCaret(Caret + 1, true);
                    break;
                case EditCommand.MoveSelectionBackwardsByCharacter:
                    MoveCaret(Caret - 1, true);
                    break;
                case EditCommand.MoveToEndOfLine:
                    MoveCaret(_value.Length, false);
                    break;
                case EditCommand.MoveToStartOfLine:
                    MoveCaret(0, false);
                    break;
                case EditCommand.MoveSelectionToEndOfLine:
                    MoveCaret(_value.Length, true);
                    break;
                case EditCommand.MoveSelectionToStartOfLine:
                    MoveCaret(0, true);
                    break;
                case EditCommand.SelectAll:
                    _selectionPos = 0;
                    MoveCaret(_value.Length, true);
                    break;
                case EditCommand.Cut:
                    if (SelectedText.Length > 0)
                    {
                        OwnerDocument.Host?.Clipboard.SetText(SelectedText);
                        SelectedText = "";
                    }

                    break;
                case EditCommand.Copy:
                    if (SelectedText.Length > 0)
                    {
                        OwnerDocument.Host?.Clipboard.SetText(SelectedText);
                    }

                    break;
                case EditCommand.Paste:
                    if (OwnerDocument.Host != null)
                    {
                        if (OwnerDocument.Host.Clipboard.TryGetText(out var text))
                        {
                            SelectedText = text;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }

            return true;
        }

        public enum EditCommand
        {
            DeleteNextCharacter,
            DeletePreviousCharacter,
            DeleteNextWord,
            DeletePreviousWord,
            MoveForwardByCharacter,
            MoveBackwardsByCharacter,
            MoveSelectionForwardByCharacter,
            MoveSelectionBackwardsByCharacter,
            MoveToEndOfLine,
            MoveToStartOfLine,
            MoveSelectionToEndOfLine,
            MoveSelectionToStartOfLine,
            SelectAll,
            Copy,
            Paste,
            Cut
        }

        public static class EditCommandHandler
        {
            public static bool TryGetEditCommand(KeyboardEvent evt, out EditCommand command)
            {
                var key = evt.Key;
                var modifiers = evt.ActiveModifiers;
                var selecting = (modifiers & KeyboardModifier.Shift) != 0;
                if (selecting)
                {
                    modifiers &= ~KeyboardModifier.Shift;
                }

                if (key == KeyboardKey.ArrowLeft)
                {
                    command = selecting
                        ? EditCommand.MoveSelectionBackwardsByCharacter
                        : EditCommand.MoveBackwardsByCharacter;
                }
                else if (key == KeyboardKey.ArrowRight)
                {
                    command = selecting
                        ? EditCommand.MoveSelectionForwardByCharacter
                        : EditCommand.MoveForwardByCharacter;
                }
                else if (key == KeyboardKey.End)
                {
                    command = selecting
                        ? EditCommand.MoveSelectionToEndOfLine
                        : EditCommand.MoveToEndOfLine;
                }
                else if (key == KeyboardKey.Home)
                {
                    command = selecting
                        ? EditCommand.MoveSelectionToStartOfLine
                        : EditCommand.MoveToStartOfLine;
                }
                else if (key == KeyboardKey.Backspace && modifiers == default)
                {
                    command = EditCommand.DeletePreviousCharacter;
                }
                else if (key == KeyboardKey.Backspace && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.DeletePreviousWord;
                }
                else if (key == KeyboardKey.Delete && modifiers == default)
                {
                    command = EditCommand.DeleteNextCharacter;
                }
                else if (key == KeyboardKey.Delete && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.DeleteNextWord;
                }
                else if (key == KeyboardKey.A && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.SelectAll;
                }
                else if (key == KeyboardKey.V && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.Paste;
                }
                else if (key == KeyboardKey.C && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.Copy;
                }
                else if (key == KeyboardKey.X && modifiers == KeyboardModifier.Control)
                {
                    command = EditCommand.Cut;
                }
                else
                {
                    command = default;
                    return false;
                }

                return true;
            }
        }

        public SelectionDirection SelectionDirection
        {
            get
            {
                return (_caret - _selectionPos) switch
                {
                    >= 0 => SelectionDirection.Forward,
                    < 0 => SelectionDirection.Backward
                };
            }
            set
            {
                // Swap the caret/selection-point if the requested direction is different
                if (value == SelectionDirection.Forward && _caret < _selectionPos
                || value == SelectionDirection.Backward && _caret > _selectionPos)
                {
                    var tmp = _selectionPos;
                    _selectionPos = _caret;
                    _caret = tmp;
                }
            }
        }

        public int SelectionStart
        {
            get => Math.Min(_selectionPos, _caret);
            set
            {
                var end = Math.Max(SelectionEnd, value);
                SetSelectionRange(value, end, SelectionDirection);
            }
        }

        public int SelectionEnd
        {
            get => Math.Max(_selectionPos, _caret);
            set => SetSelectionRange(SelectionStart, value, SelectionDirection);
        }

        public void SetSelectionRange(int start, int end, SelectionDirection direction = SelectionDirection.Forward)
        {
            throw new NotImplementedException();
        }

        public void SetRangeText(string replacement)
        {
            throw new NotImplementedException();
        }

        public void SetRangeText(string replacement, int start, int end)
        {
            throw new NotImplementedException();
        }

        public void SetRangeText(string replacement, int start, int end, SelectMode selectionMode)
        {
            throw new NotImplementedException();
        }
    }
}