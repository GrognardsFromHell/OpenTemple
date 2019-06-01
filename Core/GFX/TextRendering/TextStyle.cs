namespace SpicyTemple.Core.GFX.TextRendering
{
    public enum TextAlign : byte
    {
        Left,
        Center,
        Right,
        Justified
    };

    public enum ParagraphAlign : byte
    {
        Near,
        Far,
        Center
    };

    public struct Brush
    {
        public bool gradient;
        public PackedLinearColorA primaryColor;
        public PackedLinearColorA secondaryColor;

        public Brush(PackedLinearColorA fillColor)
        {
            gradient = false;
            primaryColor = fillColor;
            secondaryColor = fillColor;
        }

        public static Brush Default => new Brush
        {
            primaryColor = PackedLinearColorA.White,
            secondaryColor = PackedLinearColorA.White
        };
    };

    public sealed class TextStyle
    {
        public string id;
        public string fontFace;
        public float pointSize = 12;
        public bool bold = false;
        public bool italic = false;
        public TextAlign align = TextAlign.Left;
        public ParagraphAlign paragraphAlign = ParagraphAlign.Near;
        public Brush foreground = Brush.Default;
        public bool uniformLineHeight = false;
        public float lineHeight = 0;
        public float baseLine = 0;
        public bool dropShadow = false;
        public Brush dropShadowBrush = Brush.Default;
        public bool trim = false;
        public float tabStopWidth = 0;

        public int legacyLeading;
        public int legacyKerning;
        public int legacyTracking;

        public TextStyle Copy()
        {
            return (TextStyle) MemberwiseClone();
        }
    }
}