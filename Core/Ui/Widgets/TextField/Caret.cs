using System;
using System.Net.Http;
using System.Text;
using SDL2;

namespace OpenTemple.Core.Ui.Widgets.TextField;

public class Caret
{
    private readonly StringBuilder _buffer;

    /// <summary>
    /// Triggered when the string builder given to the caret is modified by the caret.
    /// </summary>
    public event Action? OnChangeText;
    /// <summary>
    /// Triggered when the caret position or selection anchor changes.
    /// </summary>
    public event Action? OnChangeCaret;

    public Caret(StringBuilder buffer)
    {
        _buffer = buffer;
    }

    public int Position { get; private set; }

    public int SelectionAnchor { get; private set; }

    public void Set(int position) => Set(position, position);

    public void Set(int position, bool keepSelection)
    {
        Set(position, keepSelection ? SelectionAnchor : position);
    }

    public void Set(int position, int selectionAnchor)
    {
        Position = ClampPosition(position);
        SelectionAnchor = ClampPosition(selectionAnchor);
        OnChangeCaret?.Invoke();
    }

    private int ClampPosition(int position) => Math.Clamp(position, 0, _buffer.Length);

    public int SelectionStartIndex => Math.Min(Position, SelectionAnchor);
    public int SelectionEndIndex => Math.Max(Position, SelectionAnchor);
    public int SelectionLength => SelectionEndIndex - SelectionStartIndex;

    public string SelectedText => _buffer.ToString(SelectionStartIndex, SelectionLength);

    public void Replace(ReadOnlySpan<char> text)
    {
        _buffer.Remove(SelectionStartIndex, SelectionLength);
        _buffer.Insert(SelectionStartIndex, text);
        OnChangeText?.Invoke();
        Set(SelectionStartIndex + text.Length);
    }

    public void DeleteBackwards(CaretMove unit)
    {
        if (Position == SelectionAnchor)
        {
            SelectionAnchor = GetBackwardsMovePosition(unit);
        }

        DeleteSelection();
    }

    public void DeleteForward(CaretMove unit)
    {
        if (Position == SelectionAnchor)
        {
            SelectionAnchor = GetForwardMovePosition(unit);
        }

        DeleteSelection();
    }

    public void DeleteSelection()
    {
        _buffer.Remove(SelectionStartIndex, SelectionLength);
        OnChangeText?.Invoke();
        Set(SelectionStartIndex);
    }

    public void MoveBackwards(CaretMove unit, bool keepSelection)
    {
        Set(GetBackwardsMovePosition(unit), keepSelection);
    }

    public void MoveForward(CaretMove unit, bool keepSelection)
    {
        Set(GetForwardMovePosition(unit), keepSelection);
    }

    private int GetBackwardsMovePosition(CaretMove unit)
    {
        return ClampPosition(unit switch
        {
            CaretMove.Character => Position - 1,
            CaretMove.Word => FindBeginningOfPreviousWord(Position),
            CaretMove.All => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        });
    }

    private int GetForwardMovePosition(CaretMove unit)
    {
        return ClampPosition(unit switch
        {
            CaretMove.Character => Position + 1,
            CaretMove.Word => FindBeginningOfNextWord(Position),
            CaretMove.All => _buffer.Length,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        });
    }

    private int FindBeginningOfNextWord(int start)
    {
        var current = start;

        // Skip whitespace
        while (current < _buffer.Length && char.IsWhiteSpace(_buffer[current]))
            current++;

        // Skip non-whitespace (if we're in the middle or at the beginning of a word
        while (current < _buffer.Length && !char.IsWhiteSpace(_buffer[current]))
            current++;

        // Skip trailing whitespace
        while (current < _buffer.Length && char.IsWhiteSpace(_buffer[current]))
            current++;

        return current;
    }

    private int FindBeginningOfPreviousWord(int start)
    {
        // Skip trailing whitespace
        var current = start;
        while (current > 0 && char.IsWhiteSpace(_buffer[current - 1]))
            current--;

        // Now skip any non-whitespace
        while (current > 0 && !char.IsWhiteSpace(_buffer[current - 1]))
            current--;

        return current;
    }

    public void SelectAll()
    {
        Set(_buffer.Length, 0);
    }
}

public enum CaretMove
{
    Character,
    Word,
    All
}