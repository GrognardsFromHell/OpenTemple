using System;
using System.Diagnostics;
using System.Text;

namespace SpicyTemple.Core.IO.MesFiles
{
    /// <summary>
    /// Implements a lexer to tokenize a .mes file based on a Span.
    /// </summary>
    internal ref struct MesLexer
    {
        private readonly StringBuilder _tokenBuilder;

        private readonly string _filename;

        private ReadOnlySpan<byte> _currentData;

        public int Line { get; set; }

        public MesLexer(string filename, ReadOnlySpan<byte> data) : this()
        {
            _filename = filename;
            _currentData = data;
            _tokenBuilder = new StringBuilder(100);
            Line = 1;
        }

        public bool ReadNextToken(out string token)
        {
            _tokenBuilder.Clear();

            // Seek token start
            var foundStart = false;
            for (var ch = GetCh(); ch != -1; ch = GetCh())
            {
                if (ch == '}')
                {
                    Debug.Print("Closing brace before opening brace @ {0}:{1}", _filename, Line);
                }
                else if (ch == '{')
                {
                    foundStart = true;
                    break;
                }
            }

            if (!foundStart)
            {
                token = null;
                return false;
            }

            for (var ch = GetCh(); ch != -1; ch = GetCh())
            {
                if (ch == '}')
                {
                    token = _tokenBuilder.ToString();
                    return true;
                }

                _tokenBuilder.Append((char) ch);

                if (ch == '{')
                {
                    Debug.Print("Found opening brace before closing brace @ {0}:{1}", _filename, Line);
                }

                if (_tokenBuilder.Length >= 1999)
                {
                    Debug.Print("Line exceeds 2000 char limit @ {0}:{1}", _filename, Line);
                }
            }

            Debug.Print("Found EOF before reaching closing brace @ {0}:{1}", _filename, Line);
            token = null;
            return false; // Incomplete token
        }

        private int GetCh()
        {
            if (_currentData.Length > 0)
            {
                var ch = (char)_currentData[0];
                _currentData = _currentData.Slice(1);

                if (ch == '\n')
                {
                    Line++;
                }

                return ch;
            }

            return -1;
        }

    }
}