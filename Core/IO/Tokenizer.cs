using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace SpicyTemple.Core.IO
{

/*
	String tokenizer that works like the ToEE vanilla one.
*/
    public class Tokenizer {

        public Tokenizer(string input)
        {
            mIn = new StringReader(input);
        }

        public bool NextToken()
        {
            // This skips comments / spaces after a token has been read
            if (LineHasMoreChars()) {
                SkipSpaceAndControl();
                SkipComment();
            }

            // This loop finds the next line
            // If we ran out of line, seek to the next one
            while (!LineHasMoreChars()) {
                if (!GetLine()) {
                    return false;
                }
            }

            // Record the position where the current token started
            var startOfToken = mLinePos;

            if (ReadNumber()) {
                return true;
            } else if (ReadQuotedString()) {
                return true;
            } else if (ReadIdentifier()) {
                return true;
            }

            mLinePos = startOfToken;
            var ch = TakeChar();

            if (char.IsPunctuation(ch)) {
                mTokenText = ch.ToString();
                mTokenType = TokenType.Unknown;
                return true;
            }

            throw new TokenizerException($"Unrecognized character: {ch}");

        }

        public bool IsQuotedString => mTokenType == TokenType.QuotedString;

        public bool IsNumber => mTokenType == TokenType.Number;

        public bool IsIdentifier => mTokenType == TokenType.Identifier;

        public bool IsNamedIdentifier(string identifier)
        {
            // In debug mode ensure that the identifier
            // being passed in is lowercase
            Debug.Assert(identifier.ToLowerInvariant() == identifier);

            if (mTokenType != TokenType.Identifier) {
                return false;
            }

            return mTokenText == identifier;
        }

        public string TokenText => mTokenText;

        public bool IsEnableEscapes { get; set; } = true;

        public int TokenInt => mTokenInt;

        public float TokenFloat => (float) mTokenFloat;

        private enum TokenType {
            Number,
            QuotedString,
            Identifier,
            Unknown
        };

        private StringReader mIn;
        private string mTokenText;
        private StringBuilder tokenTextBuilder = new StringBuilder();
        private TokenType mTokenType = TokenType.Unknown;
        private double mTokenFloat = 0;
        private int mTokenInt = 0;

        private string mLine; // Buffer for current line
        private int mLinePos; // Current position within mLine
        private int mLineNo = 0;

        private bool GetLine()
        {
            mLinePos = 0;
            mLine = mIn.ReadLine();
            if (mLine != null) {
                // Spaces at beginning and end of the line
                // are ignored. This is more lenient than
                // vanilla ToEE, but it is convenient
                // trim(mLine);
                mLineNo++;
                SkipSpaceAndControl();
                SkipComment();
                return true;
            }
            return false;
        }

        private bool LineHasMoreChars()
        {
            return mLine != null && mLinePos < mLine.Length;
        }

        private bool ReadNumber()
        {
            Trace.Assert(LineHasMoreChars());

            var startOfToken = mLinePos;

            var firstChar = PeekChar();

            // Handle digits
            if (firstChar != '+' && firstChar != '-' && !char.IsDigit(firstChar)) {
                return false;
            }

            SkipChar(); // Consumes the character we peeked

            tokenTextBuilder.Clear();
            tokenTextBuilder.Append(firstChar);

            var foundDecimalMarks = false;
            var foundDigits = char.IsDigit(firstChar);

            while (LineHasMoreChars()) {
                var nextChar = TakeChar();

                if (char.IsDigit(nextChar)) {
                    foundDigits = true;
                } else if (nextChar == '.' && !foundDecimalMarks) {
                    foundDecimalMarks = true;
                } else {
                    UngetChar(); // Read something that doesn't belong to the number
                    break;
                }

                tokenTextBuilder.Append(nextChar);
            }

            mTokenText = tokenTextBuilder.ToString();

            // Seems to be a number...
            if (!foundDigits) {
                // While we started with + or -, we didn't actually read a number
                mLinePos = startOfToken;
                return false;
            }

            double.TryParse(mTokenText, NumberStyles.Any, CultureInfo.InvariantCulture, out mTokenFloat);
            int.TryParse(mTokenText, NumberStyles.Integer, CultureInfo.InvariantCulture, out mTokenInt);
            mTokenType = TokenType.Number;
            return true;
        }

        private bool ReadQuotedString()
        {

            var startedOnLine = mLineNo;
            var startedPos = mLine;
            var firstChar = PeekChar();

            if (firstChar != '"' && firstChar != '\'') {
                return false; // Not a quoted string
            }

            TakeChar(); // We dont actually store the quote

            tokenTextBuilder.Clear();

            var lastLineEndingEscaped = false;

            while (true) {

                // If the quoted string is not terminated yet, it actually eats lines
                while (!LineHasMoreChars()) {
                    if (!GetLine()) {
                        // TODO Nice error
                        throw new TokenizerException("Unterminated string literal");
                    }
                    if (!lastLineEndingEscaped) {
                        tokenTextBuilder.Append('\n');
                    } else {
                        // The backslash is worth only a single line ending
                        lastLineEndingEscaped = false;
                    }
                }

                var ch = TakeChar();
                if (ch == firstChar) {
                    break;
                }

                // Handle escape sequences if enabled
                if (IsEnableEscapes && ch == '\\') {
                    // It's possible for a quoted string to span multiple lines by
                    // escaping the end of the line
                    if (!LineHasMoreChars()) {
                        lastLineEndingEscaped = true;
                        continue;
                    }

                    // Escape sequences always consume the next char
                    var nextChar = TakeChar();
                    switch (char.ToLowerInvariant(nextChar)) {
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

                tokenTextBuilder.Append(ch);
            }

            mTokenText = tokenTextBuilder.ToString();
            mTokenType = TokenType.QuotedString;
            return true;
        }

        private bool ReadIdentifier()
        {
            var startedPos = mLine;
            var firstChar = PeekChar();

            // Identifiers start with an alpha char or underscore
            if (firstChar != '_' && !char.IsLetter(firstChar)) {
                return false;
            }

            tokenTextBuilder.Clear();
            while (LineHasMoreChars()) {
                var ch = TakeChar();
                if (char.IsDigit(ch) || char.IsLetter(ch) || ch == '_') {
                    // Note that identifiers are always lowercased automatically
                    tokenTextBuilder.Append(char.ToLowerInvariant(ch));
                } else {
                    UngetChar();
                    break;
                }
            }

            mTokenText = tokenTextBuilder.ToString();
            mTokenType = TokenType.Identifier;
            return true;
        }

        // Character based reading on the input source
        private char PeekChar()
        {
            Trace.Assert(LineHasMoreChars());
            return mLine[mLinePos];
        }

        private void SkipChar()
        {
            Trace.Assert(LineHasMoreChars());
            mLinePos++;
        }

        private char TakeChar()
        {
            Trace.Assert(LineHasMoreChars());
            return mLine[mLinePos++];
        }

        private void UngetChar()
        {
            mLinePos--;
            Trace.Assert(mLinePos >= 0);
        }

        // Seeks past any control or space characters at current line pos
        private void SkipSpaceAndControl()
        {
            // Skip control characters and spaces
            while (LineHasMoreChars()) {
                var ch = mLine[mLinePos];
                if (!char.IsWhiteSpace(ch) && !char.IsControl(ch)) {
                    return;
                }
                mLinePos++; // Skip space & control
            }
        }

        // Skips past comment at current line pos to end of line
        private void SkipComment()
        {
            if (mLinePos < mLine.Length
                && mLine[mLinePos] == '#') {
                mLinePos = mLine.Length;

            } else if (mLinePos + 1 < mLine.Length
                       && mLine[mLinePos] == '/'
                       && mLine[mLinePos + 1] == '/') {
                mLinePos = mLine.Length;
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