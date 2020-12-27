using System;
using System.Text;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.TextInput
{
    /// <summary>
    /// Class supporting single- and multi-line
    /// plain text input controls.
    /// </summary>
    public class TextInputHelper
    {

        [Flags]
        private enum DirtyFlag
        {
            Text = 1,
            Caret = 2,
            Selection = 4,
            All = Text | Caret | Selection
        }

        public bool IsMultiLine { get; }

        /// <summary>
        /// The selection position. If nothing is selected, this equals the caret position.
        /// This may be before or after the caret, depending on the direction of the selection.
        /// </summary>
        private int _selectionPos;

        private readonly Element _host;

        private readonly StringBuilder _value = new();

        public event Action OnChange;

        public TextInputHelper(Element host, bool multiLine)
        {
            _host = host;
            IsMultiLine = multiLine;
        }

        public string Value
        {
            get => _value.ToString();
            set => SetRangeText(value, 0, _value.Length, SelectionMode.End);
        }

        private int _caret;

        public int Caret
        {
            get => _caret;
            private set => _caret = Math.Clamp(value, 0, _value.Length);
        }

        public (int, int) SelectionRange
        {
            get
            {
                var start = Math.Min(Caret, _selectionPos);
                var length = Math.Abs(Caret - _selectionPos);
                return (start, length);
            }
        }

        public void MoveCaret(int position, bool selecting)
        {
            if (selecting)
            {
                Caret = position;
            }
            else
            {
                _selectionPos = position;
                Caret = position;
            }

            OnChange?.Invoke();
        }

        public void DeleteCharacter(SelectionDirection direction)
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
                        _host.OwnerDocument.Host?.Clipboard.SetText(SelectedText);
                        SetRangeText("");
                    }

                    break;
                case EditCommand.Copy:
                    if (SelectedText.Length > 0)
                    {
                        _host.OwnerDocument.Host?.Clipboard.SetText(SelectedText);
                    }

                    break;
                case EditCommand.Paste:
                    if (_host.OwnerDocument.Host != null)
                    {
                        if (_host.OwnerDocument.Host.Clipboard.TryGetText(out var text))
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
                    OnChange?.Invoke();
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
                _host.OwnerDocument?.Host.Defer(() =>
                {
                    var evt = new UiEvent(SystemEventType.Select, new UiEventInit()
                    {
                        Bubbles = true
                    });
                    _host.Dispatch(evt);
                });
            }

            OnChange?.Invoke();
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

            OnChange?.Invoke();
        }

    }
}