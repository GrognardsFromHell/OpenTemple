using System;
using System.Runtime.InteropServices;
using System.Security;
using OpenTemple.Interop;

namespace OpenTemple.Core.IO.Images
{
    internal static class CursorLoader
    {
        public static NativeCursor LoadCursor(ReadOnlySpan<byte> data, int hotspotX, int hotspotY)
        {
            var imageInfo = IO.Images.ImageIO.DecodeImage(data);
            return new NativeCursor(
                imageInfo.data,
                imageInfo.info.width,
                imageInfo.info.height,
                hotspotX,
                hotspotY
            );
        }
    }

    public sealed class NativeCursor : IDisposable
    {
        public IntPtr Handle { get; private set; }

        public NativeCursor(
            byte[] pixelData,
            int width,
            int height,
            int hotspotX,
            int hotspotY)
        {
            Handle = cursor_create(pixelData, width, height, hotspotX, hotspotY);
        }

        public void Dispose()
        {
            cursor_delete(Handle);
            Handle = IntPtr.Zero;
        }


        [DllImport(OpenTempleLib.Path)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr cursor_create(
            [In]
            byte[] pixelData,
            int width,
            int height,
            int hotspotX,
            int hotspotY
        );

        [DllImport(OpenTempleLib.Path)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr cursor_delete(IntPtr handle);
    }
}