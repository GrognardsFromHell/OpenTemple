using OpenTemple.Core.IO.Images;
using SkiaSharp;

namespace OpenTemple.Core.TigSubsystems
{
    public static class DecodedImageExtensions
    {
        public static SKImage ToSkImage(this DecodedImage image)
        {
            var info = image.info;
            var alphaType = info.hasAlpha ? SKAlphaType.Unpremul : SKAlphaType.Opaque;
            var colorType = SKColorType.Bgra8888;
            var imageInfo = new SKImageInfo(info.width, info.height, colorType, alphaType);

            return SKImage.FromPixelCopy(imageInfo, image.data, info.width * 4);
        }

        public static SKImage ToSkImage(this DecodedAlphaMask image)
        {
            var alphaType = SKAlphaType.Premul;
            var colorType = SKColorType.Alpha8;
            var imageInfo = new SKImageInfo(image.Width, image.Height, colorType, alphaType);

            return SKImage.FromPixelCopy(imageInfo, image.Data, image.Width);
        }
    }
}