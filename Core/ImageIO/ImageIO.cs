using System;

namespace SpicyTemple.Core.ImageIO
{
    public struct DecodedImage
    {
        public byte[] data;
        public ImageFileInfo info;
    }

    /// <summary>
    /// Specifies the pixel format of the uncompressed data
    /// when encoding or decoding a JPEG image.
    /// </summary>
    enum JpegPixelFormat
    {
        RGB,
        BGR,
        RGBX,
        BGRX,
        XBGR,
        XRGB
    }

    public static class ImageIO
    {
        /// <summary>
        /// Tries to detect the image format of the given data by
        /// inspecting the header only.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ImageFileInfo DetectImageFormat(ReadOnlySpan<byte> data);

        public static bool DetectTga(ReadOnlySpan<byte> data, out ImageFileInfo info)
        {
        }

        public static byte[] DecodeTga(ReadOnlySpan<byte> data)
        {
        }

        public static byte[] EncodeJpeg(ReadOnlySpan<byte> imageData,
            JpegPixelFormat imageDataFormat,
            int width,
            int height,
            int quality,
            int pitch);

        public static DecodedImage DecodeFontArt(ReadOnlySpan<byte> data);

        public static DecodedImage DecodeImage(ReadOnlySpan<byte> data);

        public static DecodedImage DecodeCombinedImage(string filename, ReadOnlySpan<byte> data);

        public static IntPtr LoadImageToCursor(ReadOnlySpan<byte> data, uint hotspotX, uint hotspotY);

    }
}