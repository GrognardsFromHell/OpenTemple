using System.Drawing;

namespace SpicyTemple.Core.IO.Fonts
{
    public class FontFace
    {

        /// <summary>
        /// Height of the original font face in pixels i think.
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Basename of the font face. Used to load fntart files.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Height of the tallest glyph in pixels.
        /// </summary>
        public int LargestHeight { get; set; }

        /// <summary>
        /// The number of fntart files associated with this font.
        /// </summary>
        public int FontArtCount { get; set; }

        /// <summary>
        /// The glyphs included in this font.
        /// </summary>
        public FontFaceGlyph[] Glyphs { get; set; }

        /// <summary>
        /// Was the font rendered with anti-aliasing?
        /// </summary>
        public bool AntiAliased { get; set; }

        public int BaseLine { get; set; }

    }

    public struct FontFaceGlyph
    {
        public Rectangle Rectangle { get; set; }
        /// <summary>
        /// Which texture (index) this glyph is on.
        /// </summary>
        public int FontArtIndex { get; set; }
        public int WidthLine { get; set; }
        public int WidthLineXOffset { get; set; }
        public int BaseLineYOffset { get; set; }
    }

}
