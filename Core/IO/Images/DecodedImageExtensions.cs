using System;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.IO.Images
{
    public static class DecodedImageExtensions
    {
        public static PackedLinearColorA ReadPackedPixel(this DecodedImage image, int x, int y)
        {
            ReadOnlySpan<uint> intData = MemoryMarshal.Cast<byte, uint>(image.data);
            var pixelValue = intData[y * image.info.width + x];
            return new PackedLinearColorA(pixelValue);
        }
    }
}