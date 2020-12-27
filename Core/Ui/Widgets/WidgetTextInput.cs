using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.TextInput;

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

        void SetRangeText(string replacement, SelectionMode selectionMode = SelectionMode.Preserve);

        void SetRangeText(string replacement, int start, int end, SelectionMode selectionMode = SelectionMode.Preserve);
    }

    public partial class WidgetTextInput : WidgetBase, ITextInputElement
    {
        private readonly TextBlock _textBlock = Tig.RenderingDevice.GetTextEngine().CreateTextBlock();

        private readonly StringBuilder _value = new();

        [Flags]
        private enum DirtyFlag
        {
            Text = 1,
            Caret = 2,
            Selection = 4,
            All = Text | Caret | Selection
        }

        private DirtyFlag _dirty = DirtyFlag.All;

        private int _caret;

        private int Caret
        {
            get => _caret;
            set
            {
                _caret = Math.Clamp(value, 0, _value.Length);
                _dirty |= DirtyFlag.Caret;
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
            set => SetRangeText(value, 0, _value.Length, SelectionMode.End);
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
            AddEventListener<MouseEvent>(SystemEventType.MouseMove, HandleMouseMove);
            AddEventListener<MouseEvent>(SystemEventType.MouseUp, HandleMouseUp);
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

            if (_dirty != default)
            {
                if ((_dirty & DirtyFlag.Text) != 0)
                {
                    _textBlock.SetText(_value.ToString());
                }

                if ((_dirty & DirtyFlag.Selection) != 0)
                {
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

                if ((_dirty & DirtyFlag.Caret) != 0)
                {
                    EnsureCaretVisibility();
                }

                _dirty = default;
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

        private enum DragMode
        {
            None,
            MovingCaret,
            Selecting
        }

        private DragMode _dragMode;

        private void HandleMouseDown(MouseEvent evt)
        {
            if (evt.Button == 0)
            {
                _dragMode = evt.ShiftKey ? DragMode.Selecting : DragMode.MovingCaret;
                SetPointerCapture();

                HandleMouseMove(evt);
            }
        }

        private void HandleMouseMove(MouseEvent evt)
        {
            if (_dragMode == DragMode.None || !HasPointerCapture())
            {
                return;
            }

            var rect = GetContentArea();
            var x = evt.ClientX - rect.X + _horizontalScroll;
            var y = evt.ClientY - rect.Y;
            _textBlock.HitTest(x, y, out var position);
            MoveCaret(position, _dragMode == DragMode.Selecting);
        }

        private void HandleMouseUp(MouseEvent evt)
        {
            _dragMode = DragMode.None;
            if (evt.Button == 0)
            {
                ReleasePointerCapture();
            }
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

            _dirty |= DirtyFlag.Selection;
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

        private void DeleteCharacter(SelectionDirection direction)
        {
            if (_caret == _selectionPos)
            {
                if (direction == SelectionDirection.Backward)
                {
                    SetRangeText("", _caret - 1, _caret, SelectionMode.End);
                }
                else
                {
                    SetRangeText("", _caret, _caret + 1, SelectionMode.End);
                }
            }
            else
            {
                SetRangeText(""); // Delete selection
            }
        }

        private int GetStartOfPreviousWord()
        {
            var cur = _caret - 1;
            while (cur >= 0 && char.IsWhiteSpace(_value[cur]))
            {
                cur--;
            }

            while (cur >= 0 && !char.IsWhiteSpace(_value[cur]))
            {
                cur--;
            }

            return Math.Max(0, cur);
        }

        private int GetEndOfNextWord()
        {
            var cur = _caret;
            while (cur < _value.Length && char.IsWhiteSpace(_value[cur]))
            {
                cur++;
            }

            while (cur < _value.Length && !char.IsWhiteSpace(_value[cur]))
            {
                cur++;
            }

            return cur;
        }

        private void DeleteWord(SelectionDirection direction)
        {
            if (_caret == _selectionPos)
            {
                if (direction == SelectionDirection.Backward)
                {
                    SetRangeText("", GetStartOfPreviousWord(), _caret, SelectionMode.End);
                }
                else
                {
                    SetRangeText("", _caret, GetEndOfNextWord(), SelectionMode.End);
                }
            }
            else
            {
                SetRangeText(""); // Delete selection
            }
        }

        public bool ExecuteCommand(EditCommand command)
        {
            switch (command)
            {
                case EditCommand.DeleteNextCharacter:
                    DeleteCharacter(SelectionDirection.Forward);
                    break;
                case EditCommand.DeletePreviousCharacter:
                    DeleteCharacter(SelectionDirection.Backward);
                    break;
                case EditCommand.DeleteNextWord:
                    DeleteWord(SelectionDirection.Forward);
                    break;
                case EditCommand.DeletePreviousWord:
                    DeleteWord(SelectionDirection.Backward);
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
                case EditCommand.MoveForwardByWord:
                    MoveCaret(GetEndOfNextWord(), false);
                    break;
                case EditCommand.MoveBackwardsByWord:
                    MoveCaret(GetStartOfPreviousWord(), false);
                    break;
                case EditCommand.MoveSelectionForwardByWord:
                    MoveCaret(GetEndOfNextWord(), true);
                    break;
                case EditCommand.MoveSelectionBackwardsByWord:
                    MoveCaret(GetStartOfPreviousWord(), true);
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
                    if (_selectionPos != Caret)
                    {
                        OwnerDocument.Host?.Clipboard.SetText(SelectedText);
                        SetRangeText("");
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
                            SetRangeText(text, SelectionMode.End);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }

            return true;
        }

        public string SelectedText
        {
            get
            {
                var (start, length) = SelectionRange;
                return _value.ToString(start, length);
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
            if (start > end)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            start = Math.Clamp(start, 0, _value.Length);
            end = Math.Clamp(end, 0, _value.Length);

            var beforeCaret = Caret;
            var beforeSelection = _selectionPos;

            if (direction == SelectionDirection.Forward)
            {
                Caret = end;
                _selectionPos = start;
            }
            else
            {
                Caret = start;
                _selectionPos = end;
            }

            if (beforeCaret != _caret || beforeSelection != _selectionPos)
            {
                OwnerDocument?.Host.Defer(() =>
                {
                    var evt = new UiEvent(SystemEventType.Select, new UiEventInit()
                    {
                        Bubbles = true
                    });
                    Dispatch(evt);
                });
            }
        }

        public void SetRangeText(string replacement, SelectionMode selectionMode = SelectionMode.Preserve)
        {
            SetRangeText(replacement, SelectionStart, SelectionEnd, selectionMode);
        }

        public void SetRangeText(string replacement, int start, int end,
            SelectionMode selectionMode = SelectionMode.Preserve)
        {
            if (start > end)
            {
                throw new IndexOutOfRangeException();
            }

            start = Math.Clamp(start, 0, _value.Length);
            end = Math.Clamp(end, 0, _value.Length);

            var selStart = SelectionStart;
            var selEnd = SelectionEnd;

            if (start < end)
            {
                _value.Remove(start, end - start);
            }

            _value.Insert(start, replacement);
            _dirty = DirtyFlag.All;

            var newEnd = start + replacement.Length;

            switch (selectionMode)
            {
                case SelectionMode.Select:
                    SetSelectionRange(start, newEnd);
                    break;
                case SelectionMode.Start:
                    SetSelectionRange(start, start);
                    break;
                case SelectionMode.End:
                    SetSelectionRange(newEnd, newEnd);
                    break;
                case SelectionMode.Preserve:
                {
                    var oldLength = end - start;
                    var delta = replacement.Length - oldLength;

                    if (selStart > end)
                    {
                        // The selection was after the removed/inserted text. Just move it according to the delta
                        selStart += delta;
                    }
                    else if (selStart > start)
                    {
                        // The selection start intersects the inserted text, extend it to the new start
                        selStart = start;
                    }

                    if (selEnd > end)
                    {
                        // The selection was after the removed/inserted text. Just move it according to the delta
                        selEnd += delta;
                    }
                    else if (selEnd > start)
                    {
                        selEnd = start + replacement.Length;
                    }

                    SetSelectionRange(selStart, selEnd, SelectionDirection);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null);
            }
        }
    }
}