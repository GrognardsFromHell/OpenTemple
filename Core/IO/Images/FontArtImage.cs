using System;
using System.Diagnostics;

namespace OpenTemple.Core.IO.Images;

internal static class FontArtImage
{
    public static DecodedImage Decode(ReadOnlySpan<byte> data)
    {
        // 256x256 image with 8bit alpha
        Trace.Assert(data.Length == 256 * 256);

        DecodedImage result;
        result.info.width = 256;
        result.info.height = 256;
        result.info.format = ImageFileFormat.FNTART;
        result.info.hasAlpha = true;
        result.data = new byte[256 * 256 * 4];

        int offset = 0;
        var dest = result.data;
        foreach (var alpha in data)
        {
            dest[offset++] = alpha;
            dest[offset++] = alpha;
            dest[offset++] = alpha;
            dest[offset++] = alpha;
        }

        return result;
    }
}