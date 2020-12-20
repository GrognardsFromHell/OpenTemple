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
        public string text;
        public TextStyle defaultStyle;
        private List<ConstrainedTextStyle> formats;

        public List<ConstrainedTextStyle> Formats
        {
            get
            {
                if (formats == null)
                {
                    formats = new List<ConstrainedTextStyle>();
                }

                return formats;
            }
        }

        public void AddFormat(TextStyle style, int startChar, int length)
        {
            if (formats == null)
            {
                formats = new List<ConstrainedTextStyle>();
            }

            formats.Add(new ConstrainedTextStyle
            {
                style = style,
                startChar = startChar,
                length = length
            });
        }
    }
}