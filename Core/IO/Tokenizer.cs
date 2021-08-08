using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenTemple.Core.IO
{
    /// <summary>
    /// String tokenizer that works like the ToEE vanilla one.
    /// </summary>
    public ref struct Tokenizer
    {
        private ReadOnlySpan<char> _remainingInput;
        private StringBuilder _tokenTextBuilder;
        private ReadOnlySpan<char> _tokenText;
        private TokenType _tokenType;
        private double _tokenFloat;
        private int _tokenInt;

        private ReadOnlySpan<char> _line; // Buffer for current line
        private int _linePos; // Current position within mLine
        private int _lineNo;

        public Tokenizer(ReadOnlySpan<char> input)
        {
            _remainingInput = input;
            _tokenTextBuilder = new StringBuilder();
            _tokenText = ReadOnlySpan<char>.Empty;
            _tokenType = TokenType.Unknown;
            _tokenFloat = 0;
            _tokenInt = 0;

            IsEnableEscapes = true;

            _line = null;
            _linePos = 0;
            _lineNo = 0;
        }

        public bool NextToken()
        {
            // This skips comments / spaces after a token has been read
            if (LineHasMoreChars())
            {
                SkipSpaceAndControl();
                SkipComment();
            }

            // This loop finds the next line
            // If we ran out of line, seek to the next one
            while (!LineHasMoreChars())
            {
                if (!GetLine())
                {
                    return false;
                }
            }

            // Record the position where the current token started
            var startOfToken = _linePos;

            if (ReadNumber())
            {
                return true;
            }
            else if (ReadQuotedString())
            {
                return true;
            }
            else if (ReadIdentifier())
            {
                return true;
            }

            _linePos = startOfToken;
            var ch = TakeChar();

            if (char.IsPunctuation(ch))
            {
                _tokenText = ch.ToString();
                _tokenType = TokenType.Unknown;
                return true;
            }

            throw new TokenizerException($"Unrecognized character: {ch}");
        }

        public bool IsQuotedString => _tokenType == TokenType.QuotedString;

        public bool IsNumber => _tokenType == TokenType.Number;

        public bool IsIdentifier => _tokenType == TokenType.Identifier;

        public bool IsNamedIdentifier(string identifier)
        {
            // In debug mode ensure that the identifier
            // being passed in is lowercase
            Debug.Assert(identifier.ToLowerInvariant() == identifier);

            if (_tokenType != TokenType.Identifier)
            {
                return false;
            }

            return _tokenText.SequenceEqual(identifier);
        }

        public ReadOnlySpan<char> TokenText => _tokenText;

        public bool IsEnableEscapes { get; set; }

        public int TokenInt => _tokenInt;

        public float TokenFloat => (float)_tokenFloat;

        private enum TokenType
        {
            Number,
            QuotedString,
            Identifier,
            Unknown
        };

        private bool GetLine()
        {
            _linePos = 0;
            _line = null;

            // Search for the next newline
            int i;
            for (i = 0; i < _remainingInput.Length; i++)
            {
                var ch = _remainingInput[i];
                if (ch == '\r' || ch == '\n')
                {
                    _line = _remainingInput.Slice(0, i);
                    // Skip a potential \n following a \r
                    if (ch == '\r' && i + 1 < _remainingInput.Length && _remainingInput[i + 1] == '\n')
                    {
                        i++;
                    }

                    _remainingInput = _remainingInput.Slice(i + 1);
                    break;
                }
            }

            // If no further newline was found, just use the entire remaining input
            if (_line.IsEmpty)
            {
                _line = _remainingInput;
                _remainingInput = null;
            }

            if (!_line.IsEmpty)
            {
                // Spaces at beginning and end of the line
                // are ignored. This is more lenient than
                // vanilla ToEE, but it is convenient
                // trim(mLine);
                _lineNo++;
                SkipSpaceAndControl();
                SkipComment();
                return true;
            }

            return false;
        }

        private bool LineHasMoreChars()
        {
            return _line != null && _linePos < _line.Length;
        }

        private bool ReadNumber()
        {
            Trace.Assert(LineHasMoreChars());

            var startOfToken = _linePos;

            var firstChar = PeekChar();

            // Handle digits
            if (firstChar != '+' && firstChar != '-' && !char.IsDigit(firstChar))
            {
                return false;
            }

            SkipChar(); // Consumes the character we peeked

            _tokenText = null;
            _tokenTextBuilder.Clear();
            _tokenTextBuilder.Append(firstChar);

            var foundDecimalMarks = false;
            var foundDigits = char.IsDigit(firstChar);

            while (LineHasMoreChars())
            {
                var nextChar = TakeChar();

                if (char.IsDigit(nextChar))
                {
                    foundDigits = true;
                }
                else if (nextChar == '.' && !foundDecimalMarks)
                {
                    foundDecimalMarks = true;
                }
                else
                {
                    UngetChar(); // Read something that doesn't belong to the number
                    break;
                }

                _tokenTextBuilder.Append(nextChar);
            }

            _tokenText = _tokenTextBuilder.ToString();

            // Seems to be a number...
            if (!foundDigits)
            {
                // While we started with + or -, we didn't actually read a number
                _linePos = startOfToken;
                return false;
            }

            double.TryParse(_tokenText, NumberStyles.Any, CultureInfo.InvariantCulture, out _tokenFloat);
            int.TryParse(_tokenText, NumberStyles.Integer, CultureInfo.InvariantCulture, out _tokenInt);
            _tokenType = TokenType.Number;
            return true;
        }

        private bool ReadQuotedString()
        {
            var startedOnLine = _lineNo;
            var startedPos = _line;
            var firstChar = PeekChar();

            if (firstChar != '"' && firstChar != '\'')
            {
                return false; // Not a quoted string
            }

            TakeChar(); // We dont actually store the quote

            _tokenTextBuilder.Clear();

            var lastLineEndingEscaped = false;

            while (true)
            {
                // If the quoted string is not terminated yet, it actually eats lines
                while (!LineHasMoreChars())
                {
                    if (!GetLine())
                    {
                        // TODO Nice error
                        throw new TokenizerException("Unterminated string literal");
                    }

                    if (!lastLineEndingEscaped)
                    {
                        _tokenTextBuilder.Append('\n');
                    }
                    else
                    {
                        // The backslash is worth only a single line ending
                        lastLineEndingEscaped = false;
                    }
                }

                var ch = TakeChar();
                if (ch == firstChar)
                {
                    break;
                }

                // Handle escape sequences if enabled
                if (IsEnableEscapes && ch == '\\')
                {
                    // It's possible for a quoted string to span multiple lines by
                    // escaping the end of the line
                    if (!LineHasMoreChars())
                    {
                        lastLineEndingEscaped = true;
                        continue;
                    }

                    // Escape sequences always consume the next char
                    var nextChar = TakeChar();
                    switch (char.ToLowerInvariant(nextChar))
                    {
                        case 'n':
                        case 'r':
                            ch = '\n';
                            break;
                        case 't':
                            ch = '\t';
                            break;
                        /*
                        ToEE also supports hexadecimal escape sequences (a la \xF2), but
                        the implementation seems broken...
                        On top of that it doesn't seem to be used anywhere...
                    */
                        default:
                            // ... This kinda means any path with backslashes in MDF files gets mangled...
                            ch = nextChar;
                            break;
                    }
                }

                _tokenTextBuilder.Append(ch);
            }

            _tokenText = _tokenTextBuilder.ToString();
            _tokenType = TokenType.QuotedString;
            return true;
        }

        private bool ReadIdentifier()
        {
            var startedPos = _line;
            var firstChar = PeekChar();

            // Identifiers start with an alpha char or underscore
            if (firstChar != '_' && !char.IsLetter(firstChar))
            {
                return false;
            }

            _tokenTextBuilder.Clear();
            while (LineHasMoreChars())
            {
                var ch = TakeChar();
                if (char.IsDigit(ch) || char.IsLetter(ch) || ch == '_')
                {
                    // Note that identifiers are always lowercased automatically
                    _tokenTextBuilder.Append(char.ToLowerInvariant(ch));
                }
                else
                {
                    UngetChar();
                    break;
                }
            }

            _tokenText = _tokenTextBuilder.ToString();
            _tokenType = TokenType.Identifier;
            return true;
        }

        // Character based reading on the input source
        private char PeekChar()
        {
            Trace.Assert(LineHasMoreChars());
            return _line[_linePos];
        }

        private void SkipChar()
        {
            Trace.Assert(LineHasMoreChars());
            _linePos++;
        }

        private char TakeChar()
        {
            Trace.Assert(LineHasMoreChars());
            return _line[_linePos++];
        }

        private void UngetChar()
        {
            _linePos--;
            Trace.Assert(_linePos >= 0);
        }

        // Seeks past any control or space characters at current line pos
        private void SkipSpaceAndControl()
        {
            // Skip control characters and spaces
            while (LineHasMoreChars())
            {
                var ch = _line[_linePos];
                if (!char.IsWhiteSpace(ch) && !char.IsControl(ch))
                {
                    return;
                }

                _linePos++; // Skip space & control
            }
        }

        // Skips past comment at current line pos to end of line
        private void SkipComment()
        {
            if (_linePos < _line.Length
                && _line[_linePos] == '#')
            {
                _linePos = _line.Length;
            }
            else if (_linePos + 1 < _line.Length
                     && _line[_linePos] == '/'
                     && _line[_linePos + 1] == '/')
            {
                _linePos = _line.Length;
            }
        }
    };

    public class TokenizerException : Exception
    {
        public TokenizerException(string message) : base(message)
        {
        }
    }
}