using System;

namespace OpenTemple.Core.IO.Images
{
    internal static class CursorLoader
    {
        public static IntPtr LoadCursor(ReadOnlySpan<byte> data, int hotspotX, int hotspotY)
        {
            var imageInfo = ImageIO.DecodeImage(data);
            return Interop.CursorLoader.Win32_LoadImageToCursor(
                imageInfo.data,
                imageInfo.info.width,
                imageInfo.info.height,
                hotspotX,
                hotspotY
            );
        }
    }
}