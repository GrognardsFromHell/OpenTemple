using System;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenTemple.Core.IO.Images
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class JpegCompressor : IDisposable
    {
        private IntPtr _handle = Jpeg_CreateEncoder();

        public unsafe byte[] Compress(
            ReadOnlySpan<byte> pixelData,
            int width,
            int stride,
            int height,
            JpegPixelFormat pixelFormat,
            int quality
        )
        {
            var bufferSize = Jpeg_GetEncoderBufferSize(width, height);
            var buffer = new byte[bufferSize];
            bool result;

            fixed (byte* pixelDataPtr = pixelData, bufferPtr = buffer)
            {
                result = Jpeg_Encode(
                    _handle,
                    pixelDataPtr,
                    width,
                    stride,
                    height,
                    pixelFormat,
                    bufferPtr,
                    ref bufferSize,
                    quality);
            }

            if (!result)
            {
                throw new Exception("Failed to encode JPEG image.");
            }

            // Resize the buffer to it's actual size
            Array.Resize(ref buffer, (int) bufferSize);
            return buffer;
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                Jpeg_Destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }

        [DllImport("OpenTemple.Native")]
        private static extern IntPtr Jpeg_CreateEncoder();

        [DllImport("OpenTemple.Native")]
        private static extern uint Jpeg_GetEncoderBufferSize(int width, int height);

        [DllImport("OpenTemple.Native")]
        private static extern unsafe bool Jpeg_Encode(
            IntPtr encoder,
            byte* pixelData,
            int width,
            int pitch,
            int height,
            JpegPixelFormat pixelFormat,
            byte* outputBuffer,
            ref uint outputBufferSize,
            int quality
        );

        [DllImport("OpenTemple.Native")]
        private static extern void Jpeg_Destroy(IntPtr handle);
    }
}