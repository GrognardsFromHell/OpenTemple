using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SpicyTemple.Core.IO.Images
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class JpegDecompressor : IDisposable
    {
        private IntPtr _handle = Jpeg_CreateDecompressor();

        public unsafe bool ReadHeader(ReadOnlySpan<byte> imageData, out int width, out int height)
        {
            fixed (byte* imageDataPtr = imageData)
            {
                var imageDataSize = (uint) imageData.Length;

                return Jpeg_ReadHeader(
                    _handle,
                    imageDataPtr,
                    imageDataSize,
                    out width,
                    out height
                );
            }
        }

        public unsafe bool Read(ReadOnlySpan<byte> imageData,
            Span<byte> pixelData,
            int width,
            int stride,
            int height,
            JpegPixelFormat pixelFormat
        )
        {
            fixed (byte* imageDataPtr = imageData, pixelDataPtr = pixelData)
            {
                var imageDataSize = (uint) imageData.Length;
                return Jpeg_Decode(
                    _handle,
                    imageDataPtr,
                    imageDataSize,
                    pixelDataPtr,
                    width,
                    stride,
                    height,
                    pixelFormat,
                    0);
            }
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                Jpeg_Destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }

        [DllImport("SpicyTemple.Native")]
        private static extern IntPtr Jpeg_CreateDecompressor();

        [DllImport("SpicyTemple.Native")]
        private static extern unsafe bool Jpeg_ReadHeader(
            IntPtr decoder,
            byte* imageData,
            uint imageDataSize,
            out int width,
            out int height
        );

        [DllImport("SpicyTemple.Native")]
        private static extern unsafe bool Jpeg_Decode(
            IntPtr decoder,
            byte* imageData,
            uint imageDataSize,
            byte* pixelData,
            int width,
            int stride,
            int height,
            JpegPixelFormat pixelFormat,
            int flags
        );

        [DllImport("SpicyTemple.Native")]
        private static extern void Jpeg_Destroy(IntPtr handle);
    }
}