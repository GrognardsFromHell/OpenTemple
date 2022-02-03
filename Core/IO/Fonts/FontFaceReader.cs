using System.Drawing;
using System.IO;

namespace OpenTemple.Core.IO.Fonts;

public static class FontFaceReader
{
    public static FontFace Read(byte[] data)
    {
        var reader = new BinaryReader(new MemoryStream(data));
        return Read(reader);
    }

    public static FontFace Read(BinaryReader reader)
    {
        var result = new FontFace();
        result.BaseLine = reader.ReadInt32();
        var glyphCount = reader.ReadInt32();
        result.FontArtCount = reader.ReadInt32();
        result.LargestHeight = reader.ReadInt32();
        result.Size = reader.ReadInt32();
        result.AntiAliased = reader.ReadInt32() == 1;
        result.Name = reader.ReadPrefixedString();

        var glyphs = new FontFaceGlyph[glyphCount];
        for (var i = 0; i < glyphCount; ++i)
        {
            ReadGlyph(reader, out glyphs[i]);
        }

        result.Glyphs = glyphs;

        return result;
    }

    private static void ReadGlyph(BinaryReader reader, out FontFaceGlyph glyph)
    {
        glyph = new FontFaceGlyph
        {
            Rectangle = new Rectangle
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32()
            },
            FontArtIndex = reader.ReadInt32(),
            WidthLine = reader.ReadInt32(),
            WidthLineXOffset = reader.ReadInt32(),
            BaseLineYOffset = reader.ReadInt32()
        };
    }
}