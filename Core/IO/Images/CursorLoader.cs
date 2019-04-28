using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SpicyTemple.Core.IO.Images
{
    internal static class CursorLoader
    {
        public static IntPtr LoadCursor(ReadOnlySpan<byte> data, int hotspotX, int hotspotY)
        {
            var imageInfo = IO.Images.ImageIO.DecodeImage(data);
            return Win32_LoadImageToCursor(
                imageInfo.data,
                imageInfo.info.width,
                imageInfo.info.height,
                hotspotX,
                hotspotY
            );
        }

        [DllImport("SpicyTemple.Native.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr Win32_LoadImageToCursor(
            [In] byte[] pixelData,
            int width,
            int height,
            int hotspotX,
            int hotspotY
        );
    }
}