using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Imaging;
using Avalonia.Platform;
using Avalonia.Visuals.Media.Imaging;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.IO.TroikaArchives;

namespace OpenTemple.Core.TigSubsystems
{
    public class DelegatingRenderPlatform : IPlatformRenderInterface, IOpenGlAwarePlatformRenderInterface
    {
        private readonly IPlatformRenderInterface _impl;

        private readonly IOpenGlAwarePlatformRenderInterface _openGlImpl;

        public DelegatingRenderPlatform(IPlatformRenderInterface impl)
        {
            _impl = impl;
            if (impl is IOpenGlAwarePlatformRenderInterface openGlImpl)
            {
                _openGlImpl = openGlImpl;
            }
        }

        public IOpenGlBitmapImpl CreateOpenGlBitmap(PixelSize size, Vector dpi)
        {
            return _openGlImpl.CreateOpenGlBitmap(size, dpi);
        }

        public IFormattedTextImpl CreateFormattedText(string text, Typeface typeface, double fontSize,
            TextAlignment textAlignment,
            TextWrapping wrapping, Size constraint, IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            return _impl.CreateFormattedText(text, typeface, fontSize, textAlignment, wrapping, constraint, spans);
        }

        public IGeometryImpl CreateEllipseGeometry(Rect rect)
        {
            return _impl.CreateEllipseGeometry(rect);
        }

        public IGeometryImpl CreateLineGeometry(Point p1, Point p2)
        {
            return _impl.CreateLineGeometry(p1, p2);
        }

        public IGeometryImpl CreateRectangleGeometry(Rect rect)
        {
            return _impl.CreateRectangleGeometry(rect);
        }

        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return _impl.CreateStreamGeometry();
        }

        public IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            return _impl.CreateRenderTarget(surfaces);
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            return _impl.CreateRenderTargetBitmap(size, dpi);
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat format,
            AlphaFormat alphaFormat)
        {
            return _impl.CreateWriteableBitmap(size, dpi, format, alphaFormat);
        }

        public IBitmapImpl LoadBitmap(string fileName)
        {
            return LoadBitmap(ImageIO.DecodeImage(Tig.FS, fileName));
        }

        public IBitmapImpl LoadBitmap(Stream stream)
        {
            if (stream is VfsStream vfsStream)
            {
                try
                {
                    // Combined image files are split into chunks
                    if (vfsStream.Path.EndsWith(".img") && vfsStream.Length == 4)
                    {
                        return LoadBitmap(ImageIO.DecodeCombinedImage(vfsStream.FileSystem, vfsStream.Path,
                            vfsStream.GetBuffer().Span));
                    }

                    return LoadBitmap(ImageIO.DecodeImage(vfsStream.GetBuffer().Span));
                }
                catch (ImageIOException)
                {
                }
            }

            return null;
        }

        private unsafe IBitmapImpl LoadBitmap(DecodedImage image)
        {
            fixed (void* data = image.data)
            {
                return LoadBitmap(PixelFormat.Bgra8888, image.info.hasAlpha ? AlphaFormat.Unpremul : AlphaFormat.Opaque,
                    (IntPtr) data, new PixelSize(image.info.width, image.info.height), new Vector(96, 96),
                    image.info.width * 4);
            }
        }

        public IBitmapImpl LoadBitmapToWidth(Stream stream, int width,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return _impl.LoadBitmapToWidth(stream, width, interpolationMode);
        }

        public IBitmapImpl LoadBitmapToHeight(Stream stream, int height,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return _impl.LoadBitmapToHeight(stream, height, interpolationMode);
        }

        public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return _impl.ResizeBitmap(bitmapImpl, destinationSize, interpolationMode);
        }

        public IBitmapImpl LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size,
            Vector dpi,
            int stride)
        {
            return _impl.LoadBitmap(format, alphaFormat, data, size, dpi, stride);
        }

        public IGlyphRunImpl CreateGlyphRun(GlyphRun glyphRun, out double width)
        {
            return _impl.CreateGlyphRun(glyphRun, out width);
        }

        public bool SupportsIndividualRoundRects => _impl.SupportsIndividualRoundRects;

        public AlphaFormat DefaultAlphaFormat => _impl.DefaultAlphaFormat;

        public PixelFormat DefaultPixelFormat => _impl.DefaultPixelFormat;
    }
}