using System;
using OpenTemple.Core.Platform;
using static SDL2.SDL;

namespace OpenTemple.Core.IO.Images;

internal static class CursorLoader
{
    /// <summary>
    /// Decode and load an image into a SDL cursor.
    /// </summary>
    public static IntPtr LoadCursor(ReadOnlySpan<byte> data, int hotspotX, int hotspotY)
    {
        var image = ImageIO.DecodeImage(data);

        return LoadCursor(image, hotspotX, hotspotY);
    }

    /// <summary>
    /// Load an image into a SDL cursor.
    /// </summary>
    public static unsafe IntPtr LoadCursor(DecodedImage image, int hotspotX, int hotspotY)
    {
        var info = image.info;

        fixed (byte* pixelData = image.data)
        {
            // Keep in mind SDL surfaces do not actually copy the data,
            // they reference the pinned pointer
            var cursorSurface = SDL_CreateRGBSurfaceWithFormatFrom(
                new IntPtr(pixelData),
                info.width,
                info.height,
                info.hasAlpha ? 32 : 24,
                info.width * 4,
                info.hasAlpha
                    ? SDL_PIXELFORMAT_ARGB8888
                    : SDL_PIXELFORMAT_RGB888
            );

            if (cursorSurface == IntPtr.Zero)
            {
                throw new SDLException("Failed to create cursor surface.");
            }

            try
            {
                var cursor = SDL_CreateColorCursor(cursorSurface, hotspotX, hotspotY);
                if (cursor == IntPtr.Zero)
                {
                    throw new SDLException("Failed to create cursor");
                }

                return cursor;
            }
            finally
            {
                SDL_FreeSurface(cursorSurface);
            }
        }
    }
}