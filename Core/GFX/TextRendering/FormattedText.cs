using System.Collections.Generic;

namespace OpenTemple.Core.GFX.TextRendering
{
    public struct ConstrainedTextStyle
    {
        public TextStyle style;
        public int startChar;
        public int length;

        public ConstrainedTextStyle(TextStyle style) : this()
        {
            this.style = style;
        }
    }

    public struct FormattedText
    {
        public string Text { get; set; }
        public TextStyle DefaultStyle { get; set; }
        private List<ConstrainedTextStyle> _formats;

        public List<ConstrainedTextStyle> Formats
        {
            get
            {
                if (_formats == null)
                {
                    _formats = new List<ConstrainedTextStyle>();
                }

                return _formats;
            }
        }

        public void AddFormat(TextStyle style, int startChar, int length)
        {
            if (_formats == null)
            {
                _formats = new List<ConstrainedTextStyle>();
            }

            _formats.Add(new ConstrainedTextStyle
            {
                style = style,
                startChar = startChar,
                length = length
            });
        }
    }
}