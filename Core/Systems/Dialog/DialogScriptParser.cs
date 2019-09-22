using System;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.Systems.Dialog
{
    public ref struct DialogScriptParser
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly string _path;
        private readonly ReadOnlySpan<char> _content;
        private int _pos;
        private int _currentLine;

        public DialogScriptParser(string path, ReadOnlySpan<char> content)
        {
            _path = path;
            _content = content;
            _currentLine = 1;
            _pos = 0;
        }

        public bool GetSingleLine(out DialogLine line, out int fileLine) {
            // parse buf for bracket stuff until a line is complete
            line = default;

            ReadOnlySpan<char> fieldContent;
            if (!GetBracketContent(out fieldContent, out fileLine))
            {
                return false;
            }

            line = new DialogLine();
            if (!int.TryParse(fieldContent, out line.key))
            {
                Logger.Warn("Invalid dialog line key '{0}' @ {1}:{2}", new string(fieldContent), _path, fileLine);
                return false;
            }

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing text for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }
            line.txt = new string(fieldContent);

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing gender for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }
            line.genderField = new string(fieldContent);

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing minimum intelligence for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }

            if (!fieldContent.IsEmpty && !int.TryParse(fieldContent, out line.minIq))
            {
                Logger.Warn("Invalid minimum intelligence '{0}' for dialog line '{1}' @ {2}:{3}. Must be blank (for an NPC) or non-zero (for a PC).",
                    new string(fieldContent), line.key, _path, fileLine);
                return false;
            }

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing prerequisite check for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }
            line.testField = new string(fieldContent);

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing response for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }

            if (!fieldContent.IsEmpty && !int.TryParse(fieldContent, out line.answerLineId))
            {
                Logger.Warn("Invalid response '{0}' for dialog line '{1}' @ {2}:{3}.",
                    new string(fieldContent), line.key, _path, fileLine);
                return false;
            }

            if (!GetBracketContent(out fieldContent, out _))
            {
                Logger.Warn("Missing side-effect script for dialog line {0} @ {1}:{2}", line.key, _path, fileLine);
                return false;
            }
            if (!fieldContent.IsEmpty)
            {
                line.effectField = new string(fieldContent);
            }

            // check non-blank gender field for NPC lines
            if (line.IsNpcLine)
            {
                // if it's blank, check that the txt field isn't also blank (e.g. when using the picker stuff)
                if (line.genderField.Length == 0 && line.txt.Length == 0)
                {
                    Logger.Warn("Missing NPC response line for females: {0} (dialog line {1} of {2})",
                        fileLine, line.key, _path);
                }
            }

            return true;
        }

        private bool SeekToNextOpeningBrace()
        {
            while (_pos < _content.Length)
            {
                if (_content[_pos] == '{')
                {
                    return true;
                }

                if (_content[_pos] == '\n')
                {
                    _currentLine++;
                }

                if (_content[_pos] == '}')
                {
                    Logger.Warn("Possible closing brace without opening brace at {0}:{1}", _path, _currentLine);
                }
                _pos++;
            }

            return false;
        }

        private bool SeekToNextClosingBrace()
        {
            while (_pos < _content.Length)
            {
                if (_content[_pos] == '}')
                {
                    return true;
                }

                if (_content[_pos] == '\n')
                {
                    _currentLine++;
                }
                _pos++;
            }

            return false;
        }

        private bool GetBracketContent(out ReadOnlySpan<char> content, out int fileLine)
        {
            if (!SeekToNextOpeningBrace())
            {
                // no new brackets found
                fileLine = 0;
                content = ReadOnlySpan<char>.Empty;
                return false;
            }

            fileLine = _currentLine;
            var startOfContent = ++_pos;

            if (!SeekToNextClosingBrace())
            {
                Logger.Warn("Possibly unclosed brace @ {0}:{1}", _path, fileLine);
                content = ReadOnlySpan<char>.Empty;
                return false;
            }

            // _pos is now ON the closing brace
            content = _content.Slice(startOfContent, _pos - startOfContent);
            _pos++; // Skip the closing brace

            content = content.Trim();
            return true;
        }
    }
}