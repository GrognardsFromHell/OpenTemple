using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SpicyTemple.Core.IO.Images
{
    [SuppressUnmanagedCodeSecurity]
    internal static class StbNative
    {
        public static unsafe bool GetBitmapInfo(
            ReadOnlySpan<byte> imageData, out int width, out int height, out bool hasAlpha)
        {
            fixed (byte* imageDataPtr = imageData)
            {
                var imageDataSize = (uint) imageData.Length;
                return Stb_BmpInfo(imageDataPtr, imageDataSize, out width, out height, out hasAlpha);
            }
        }

        public static unsafe byte[] DecodeBitmap(ReadOnlySpan<byte> imageData)
        {
            byte* pixelData;
            uint pixelDataSize;
            fixed (byte* imageDataPtr = imageData)
            {
                var imageDataSize = (uint) imageData.Length;
                pixelData = Stb_BmpDecode(imageDataPtr, imageDataSize, out _, out _,
                    out _, out pixelDataSize);
            }

            try
            {
                var result = new byte[pixelDataSize];
                Marshal.Copy((IntPtr) pixelData, result, 0, (int) pixelDataSize);
                return result;
            }
            finally
            {
                Stb_BmpFree(pixelData);
            }
        }

        [DllImport("SpicyTemple.Native")]
        private static extern unsafe bool Stb_BmpInfo(
            byte* imageData,
            uint imageDataSize,
            out int width,
            out int height,
            out bool hasAlpha);

        [DllImport("SpicyTemple.Native")]
        private static extern unsafe byte* Stb_BmpDecode(
            byte* imageData,
            uint imageDataSize,
            out int width,
            out int height,
            out bool hasAlpha,
            out uint pixelDataSize);

        [DllImport("SpicyTemple.Native")]
        private static extern unsafe void Stb_BmpFree(byte* data);
    }
}