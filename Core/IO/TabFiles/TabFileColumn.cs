using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SpicyTemple.Core.IO.TabFiles
{
    public ref struct TabFileColumn
    {
        public int ColumnIndex { get; }

        public int LineNumber { get; }

        public int ValueStart { get; }

        public bool IsEmpty => _value.IsEmpty;

        public static implicit operator bool(TabFileColumn column) => !column.IsEmpty;

        public static implicit operator ReadOnlySpan<byte>(TabFileColumn column) => column._value;

        public string AsString() => Encoding.Default.GetString(_value);

        public bool EqualsIgnoreCase(ReadOnlySpan<char> text)
        {
            if (text.Length != _value.Length)
            {
                return false;
            }

            for (var i = 0; i < text.Length; i++)
            {
                // NOTE: We assume equivalent encoding here (single byte encoding, essentially)
                if (char.ToUpperInvariant(text[i]) != char.ToUpperInvariant((char) _value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetFloat(out float value)
        {
            // Hack for untrimmed crap
            ReadOnlySpan<byte> trimmedVal = _value;
            while (!trimmedVal.IsEmpty && trimmedVal[0] == ' ')
            {
                trimmedVal = trimmedVal.Slice(1);
            }

            return Utf8Parser.TryParse(trimmedVal, out value, out _);
        }

        public bool TryGetInt(out int value)
        {
            return Utf8Parser.TryParse(_value, out value, out _);
        }

        public int GetInt()
        {
            if (!TryGetInt(out var value))
            {
                throw new Exception($"Failed to parse int from '{AsString()}' {Location}");
            }

            return value;
        }

        public float GetFloat()
        {
            if (!TryGetFloat(out var value))
            {
                throw new Exception($"Failed to parse float from '{AsString()}' {Location}");
            }

            return value;
        }

        public string Location => $"@{LineNumber}:{ValueStart}";

        public bool TryGetEnum<T>(Dictionary<string, T> mapping, out T value) where T : Enum
        {
            // Decode on the stack
            Span<char> decodedText = stackalloc char[Encoding.Default.GetMaxCharCount(_value.Length)];

            foreach (var (text, mappedValue) in mapping)
            {
                var textSpan = text.AsSpan();
                if (textSpan.Equals(decodedText, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = mappedValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        internal TabFileColumn(int lineNumber, int columnIndex, int valueStart, ReadOnlySpan<byte> value)
        {
            _value = value;
            LineNumber = lineNumber;
            ColumnIndex = columnIndex;
            ValueStart = valueStart;
        }

        private readonly ReadOnlySpan<byte> _value;
    }
}