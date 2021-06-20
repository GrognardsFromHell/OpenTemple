using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenTemple.Core.IO.Images
{
    internal enum TgaColorMapType : byte
    {
        TrueColor = 0,
        Indexed = 1
    };

    internal enum TgaDataType : byte
    {
        NoData = 0,
        UncompressedColorMapped = 1,
        UncompressedRgb = 2,
        UncompressedMono = 3,
        RleColorMapped = 9,
        RleRgb = 10,
        CompressedMono = 11,
        CompressedColorMapped = 32,
        CompressedColorMapped2 = 33
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TgaHeader
    {
        public byte imageIdLength;
        public TgaColorMapType colorMapType;
        public TgaDataType dataType;
        public ushort colorMapOffset;
        public ushort colorMapOrigin;
        public byte colorMapDepth;
        public short xOrigin;
        public short yOrigin;
        public ushort width;
        public ushort height;
        public byte bpp;
        public byte imageDescriptor;
    };

    internal static class TargaImage
    {
        private static readonly int HeaderSize = Marshal.SizeOf<TgaHeader>();

        public static bool DetectTga(ReadOnlySpan<byte> data, out ImageFileInfo info)
        {
            info = new ImageFileInfo();

            if (data.Length < HeaderSize)
            {
                return false;
            }

            var header = MemoryMarshal.Read<TgaHeader>(data);

            if (header.colorMapType != TgaColorMapType.TrueColor)
            {
                return false; // We don't supported index TGA
            }

            if (header.dataType != TgaDataType.UncompressedRgb)
            {
                return false; // We only support uncompressed RGB
            }

            if (header.bpp != 24 && header.bpp != 32)
            {
                return false; // We only support 24 or 32 bit TGAs
            }

            info.width = header.width;
            info.height = header.height;
            info.hasAlpha = (header.bpp == 32);
            info.format = ImageFileFormat.TGA;
            return true;
        }

        public static unsafe byte[] Decode(ReadOnlySpan<byte> data)
        {
            
		if (data.Length < HeaderSize) {
			throw new Exception("Not enough data for TGA header");
		}

		var header = MemoryMarshal.Read<TgaHeader>(data);

		if (header.colorMapType != TgaColorMapType.TrueColor) {
			throw new Exception("Only true color TGA images are supported.");
		}

		if (header.dataType != TgaDataType.UncompressedRgb) {
			throw new Exception("Only uncompressed RGB TGA images are supported.");
		}

		if (header.bpp != 24 && header.bpp != 32) {
			throw new Exception("Only uncompressed RGB 24-bpp or 32-bpp TGA images are supported.");
		}

		var result = new byte[header.width * header.height * 4];

		fixed (byte* dataPtr = data, resultPtr = result)
		{
			// Points to the start of the TGA image data
			var srcStart = dataPtr + HeaderSize + header.imageIdLength;
			var srcSize = data.Length - sizeof(TgaHeader) - header.imageIdLength;
			var dest = resultPtr;

			if (header.bpp == 24)
			{
				var srcPitch = header.width * 3;
				Trace.Assert((int) srcSize >= header.height * srcPitch);
				for (int y = 0; y < header.height; ++y)
				{
					var src = srcStart + (header.height - y - 1) * srcPitch;
					for (int x = 0; x < header.width; ++x)
					{
						*dest++ = *src++;
						*dest++ = *src++;
						*dest++ = *src++;
						*dest++ = 0xFF; // Fixed alpha
					}
				}
			}
			else
			{
				var srcPitch = header.width * 4;
				Trace.Assert(srcSize >= header.height * srcPitch);
				for (int y = 0; y < header.height; ++y)
				{
					var src = srcStart + (header.height - y - 1) * srcPitch;
					for (int x = 0; x < header.width; ++x)
					{
						*dest++ = *src++;
						*dest++ = *src++;
						*dest++ = *src++;
						*dest++ = *src++;
						// Fix-Up broken TGAs that use purple as the fill-color for transparency,
						// when using non-perfect integer scaling and linear filtering, it will
						// start sampling texels from that area too.
						if (dest[-1] == 0)
						{
							dest[-2] = 0;
							dest[-2] = 0;
							dest[-4] = 0;
						}
					}
				}
			}
		}

		return result;
        }
    }
}