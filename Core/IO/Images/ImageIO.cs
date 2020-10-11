using System;
using System.Diagnostics;
using System.IO;
using OpenTemple.Interop;

namespace OpenTemple.Core.IO.Images
{
    public struct DecodedImage
    {
        public byte[] data;
        public ImageFileInfo info;
    }

    public static class ImageIO
    {
        /// <summary>
        /// Tries to detect the image format of the given data by
        /// inspecting the header only.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ImageFileInfo DetectImageFormat(ReadOnlySpan<byte> data)
        {
            ImageFileInfo info;

            if (StbNative.GetPngInfo(data, out info.width, out info.height, out info.hasAlpha))
            {
                info.format = ImageFileFormat.PNG;
                return info;
            }

            if (StbNative.GetBitmapInfo(data, out info.width, out info.height, out info.hasAlpha))
            {
                info.format = ImageFileFormat.BMP;
                return info;
            }

            using var jpegDecompressor = new JpegDecompressor();
            if (jpegDecompressor.ReadHeader(data, out info.width, out info.height))
            {
                info.hasAlpha = false;
                info.format = ImageFileFormat.JPEG;
                return info;
            }

            if (TargaImage.DetectTga(data, out info))
            {
                return info;
            }

            // Not a very good heuristic
            if (data.Length == 256 * 256)
            {
                info.width = 256;
                info.height = 256;
                info.hasAlpha = true;
                info.format = ImageFileFormat.FNTART;
                return info;
            }

            return info;
        }

        public static byte[] DecodeTga(ReadOnlySpan<byte> data)
        {
            return TargaImage.Decode(data);
        }

        public static byte[] EncodeJpeg(ReadOnlySpan<byte> pixelData,
            JpegPixelFormat pixelFormat,
            int width,
            int height,
            int quality,
            int pitch)
        {
            using var compressor = new JpegCompressor();
            return compressor.Compress(
                pixelData,
                width,
                pitch,
                height,
                pixelFormat,
                quality
            );
        }

        public static byte[] DecodeJpeg(ReadOnlySpan<byte> data)
        {
            using var decompressor = new JpegDecompressor();

            if (!decompressor.ReadHeader(data, out var width, out var height))
            {
                throw new ImageIOException("Failed to read JPEG header.");
            }

            var pixelData = new byte[width * height * 4];
            if (!decompressor.Read(data, pixelData, width, width * 4, height, JpegPixelFormat.BGRX))
            {
                throw new ImageIOException("Unable to decompress jpeg image.");
            }

            return pixelData;
        }

        public static DecodedImage DecodeFontArt(ReadOnlySpan<byte> data)
        {
            return FontArtImage.Decode(data);
        }

        public static DecodedImage DecodeImage(ReadOnlySpan<byte> data)
        {
            DecodedImage result = new DecodedImage();
            result.info = DetectImageFormat(data);

            switch (result.info.format)
            {
                case ImageFileFormat.BMP:
                    result.data = StbNative.DecodeBitmap(data);
                    break;
                case ImageFileFormat.PNG:
                    result.data = StbNative.DecodePng(data);
                    break;
                case ImageFileFormat.JPEG:
                    result.data = DecodeJpeg(data);
                    break;
                case ImageFileFormat.TGA:
                    result.data = DecodeTga(data);
                    break;
                case ImageFileFormat.FNTART:
                    return DecodeFontArt(data);
                default:
                case ImageFileFormat.Unknown:
                    throw new ImageIOException("Unrecognized image format.");
            }

            return result;
        }

        public static DecodedImage DecodeImage(IFileSystem fs, string filename)
        {
            using var memory = fs.ReadFile(filename);
            return DecodeImage(memory.Memory.Span);
        }

        static string BuildTgaFilenamePattern(string imgFilename)
        {
            // Build a pattern for the actual tga filenames
            string filenamePattern = imgFilename;
            var dotPos = filenamePattern.LastIndexOf('.');
            if (dotPos != -1)
            {
                filenamePattern = filenamePattern.Substring(0, dotPos);
            }

            filenamePattern += "_{0}_{1}.tga";
            return filenamePattern;
        }

        public static DecodedImage DecodeCombinedImage(IFileSystem fs, string filename, ReadOnlySpan<byte> data)
        {
            var reader = new BinaryReader(new MemoryStream(data.ToArray()));

            DecodedImage result;
            result.info.format = ImageFileFormat.IMG;
            result.info.hasAlpha = true; // Depends on the TGAs, actually
            result.info.width = reader.ReadUInt16();
            result.info.height = reader.ReadUInt16();

            var filenamePattern = BuildTgaFilenamePattern(filename);

            result.data = new byte[result.info.width * result.info.height * 4];
            var stride = result.info.width * 4;

            int yTile = 0;
            for (int yCur = 0; yCur < result.info.height; yCur += 256, yTile++)
            {
                int xTile = 0;
                for (int xCur = 0; xCur < result.info.width; xCur += 256, xTile++)
                {
                    var tileFilename = string.Format(filenamePattern, xTile, yTile);
                    var tileData = DecodeImage(fs.ReadBinaryFile(tileFilename));

                    // Must fit into remaining image
                    Trace.Assert(tileData.info.width <= result.info.width - xCur);
                    Trace.Assert(tileData.info.height <= result.info.height - yCur);

                    // Have to copy row by row
                    var srcStride = tileData.info.width * 4;
                    for (var row = 0; row < tileData.info.height; row++)
                    {
                        ReadOnlySpan<byte> srcRow = tileData.data.AsSpan(row * srcStride, srcStride);

                        // Convert from Y-up to Y-down axis
                        var targetRowIdx = result.info.height - yCur - tileData.info.height + row;

                        var dest = result.data.AsSpan(targetRowIdx * stride + xCur * 4, srcStride);
                        srcRow.CopyTo(dest);
                    }
                }
            }

            return result;
        }

        public static IntPtr LoadImageToCursor(ReadOnlySpan<byte> data, int hotspotX, int hotspotY)
        {
            return CursorLoader.LoadCursor(data, hotspotX, hotspotY);
        }
    }
}