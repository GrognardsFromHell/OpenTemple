using System;

namespace OpenTemple.Core.IO.TabFiles;

internal ref struct LineReader {

    public LineReader(ReadOnlySpan<byte> content)
    {
        _content = content;
        _curLine = Span<byte>.Empty;
        _pos = 0;
    }

    public bool NextLine() {

        // Skip all \r\n
        while (!IsAtEnd && IsAtLineEnd) {
            _pos++;
        }

        if (IsAtEnd) {
            return false;
        }

        var start = _pos;
        var count = 0;
        while (!IsAtEnd && !IsAtLineEnd) {
            _pos++;
            count++;
        }

        _curLine = _content.Slice(start, count);
        return true;
    }

    public ReadOnlySpan<byte> GetLine() => _curLine;

    private ReadOnlySpan<byte> _curLine;
    private readonly ReadOnlySpan<byte> _content;
    private int _pos;

    public bool IsAtEnd => _pos >= _content.Length;

    public bool IsAtLineEnd {
        get
        {
            var ch = _content[_pos];
            return ch == '\n' || ch == '\r';
        }
    }
}