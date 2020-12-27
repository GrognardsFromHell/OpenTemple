using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using AlphaMode = Vortice.Direct2D1.AlphaMode;
using FactoryType = Vortice.DirectWrite.FactoryType;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTemple.Core.GFX.TextRendering
{
    public class FontFile
    {
        public string Name { get; }
        public byte[] Data { get; }

        public FontFile(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }
    }

    public class TextEngine : IDisposable
    {
        private static readonly TextRange FullTextRange = new() {StartPosition = 0, Length = -1};

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private ID3D11Device device3d;

        /*
            Direct2D resources
        */

        private ID2D1Device device;

        private ID2D1Factory1 factory;

        internal ID2D1DeviceContext context;

        private ID2D1Bitmap1 target;

        /*
            DirectWrite resources
        */
        internal IDWriteFactory dWriteFactory;

        /*
            Custom font handling
        */
        private readonly FontLoader _fontLoader;

        private readonly List<FontFile> _fonts = new();

        private IDWriteFontCollection _fontCollection;

        // DirectWrite caches font collections internally.
        // If we reload, we need to generate a new key
        private int _fontCollKey;

        private unsafe void LoadFontCollection()
        {
            Logger.Info("Reloading font collection...");

            Span<int> fontCollKey = stackalloc int[1]
            {
                _fontCollKey++
            };
            fixed (void* fontCollKeyPtr = fontCollKey)
            {
                _fontCollection = dWriteFactory.CreateCustomFontCollection(
                    _fontLoader,
                    new IntPtr(fontCollKeyPtr),
                    sizeof(int)
                );
            }

            // Enumerate all loaded fonts to the logger
            var count = _fontCollection.FontFamilyCount;
            Logger.Debug("Loaded {0} font families:", count);
            for (var i = 0; i < count; i++)
            {
                using var family = _fontCollection.GetFontFamily(i);

                using var familyNames = family.FamilyNames;

                var familyNamesList = new StringBuilder();
                for (var j = 0; j < familyNames.Count; j++)
                {
                    if (familyNamesList.Length > 0)
                    {
                        familyNamesList.Append(", ");
                    }

                    familyNamesList.Append(familyNames.GetString(j));
                }

                Logger.Info(" Loaded Font Family: {0}", familyNamesList);
            }
        }

        // Text format cache
        private readonly Dictionary<TextStyle, IDWriteTextFormat> _textFormats =
            new(new TextStyleEqualityComparer());

        private IDWriteTextFormat _defaultTextFormat;

        /// <summary>
        /// The default text format that is to be used if no other properties are specified.
        /// This is more of a fallback format.
        /// </summary>
        public IDWriteTextFormat DefaultTextFormat
        {
            get
            {
                if (_defaultTextFormat != null)
                {
                    return _defaultTextFormat;
                }

                if (_fontCollection == null)
                {
                    LoadFontCollection();
                }

                if (_fontCollection.FontFamilyCount == 0)
                {
                    throw new Exception("No fonts are loaded. Cannot create default text style.");
                }

                using var defaultFamily = _fontCollection.GetFontFamily(0);
                _defaultTextFormat = dWriteFactory.CreateTextFormat(
                    defaultFamily.FamilyNames.GetString(0),
                    _fontCollection,
                    FontWeight.Regular,
                    FontStyle.Normal,
                    FontStretch.Normal,
                    12,
                    ""
                );
                return _defaultTextFormat;
            }
        }

        internal IDWriteTextFormat GetTextFormat(TextStyle textStyle)
        {
            if (_textFormats.TryGetValue(textStyle, out var existingFormat))
            {
                return existingFormat;
            }

            var fontWeight = GetFontWeight(textStyle);
            var fontStyle = GetFontStyle(textStyle);

            // Lazily build the font collection
            if (_fontCollection == null)
            {
                LoadFontCollection();
            }

            // Lazily create the text format
            var textFormat = dWriteFactory.CreateTextFormat(
                textStyle.fontFace,
                _fontCollection,
                fontWeight,
                fontStyle,
                FontStretch.Normal,
                textStyle.pointSize,
                ""
            );

            if (textStyle.tabStopWidth > 0)
            {
                textFormat.IncrementalTabStop = textStyle.tabStopWidth;
            }

            textFormat.WordWrapping = WordWrapping.Wrap;

            switch (textStyle.align)
            {
                case TextAlign.Left:
                    textFormat.TextAlignment = TextAlignment.Leading;
                    break;
                case TextAlign.Center:
                    textFormat.TextAlignment = TextAlignment.Center;
                    break;
                case TextAlign.Right:
                    textFormat.TextAlignment = TextAlignment.Trailing;
                    break;
                case TextAlign.Justified:
                    textFormat.TextAlignment = TextAlignment.Justified;
                    break;
            }

            switch (textStyle.paragraphAlign)
            {
                case ParagraphAlign.Near:
                    textFormat.ParagraphAlignment = ParagraphAlignment.Near;
                    break;
                case ParagraphAlign.Far:
                    textFormat.ParagraphAlignment = ParagraphAlignment.Far;
                    break;
                case ParagraphAlign.Center:
                    textFormat.ParagraphAlignment = ParagraphAlignment.Center;
                    break;
            }

            if (textStyle.uniformLineHeight)
            {
                textFormat.SetLineSpacing(
                    LineSpacingMethod.Uniform,
                    textStyle.lineHeight,
                    textStyle.baseLine
                );
            }

            _textFormats[textStyle] = textFormat;
            return textFormat;
        }

        // Brush cache
        private Dictionary<Brush, ID2D1Brush> brushes = new(new BrushEqualityComparer());

        private static Color4 ToD2dColor(PackedLinearColorA color)
        {
            return new(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            );
        }

        internal ID2D1Brush GetBrush(in Brush brush)
        {
            if (brushes.TryGetValue(brush, out var existingBrush))
            {
                return existingBrush;
            }

            ID2D1Brush simpleBrush;

            if (!brush.gradient)
            {
                var colorValue = ToD2dColor(brush.primaryColor);
                simpleBrush = context.CreateSolidColorBrush(colorValue);
            }
            else
            {
                var gradientStops = new[]
                {
                    new GradientStop
                    {
                        Color = ToD2dColor(brush.primaryColor),
                        Position = 0
                    },
                    new GradientStop
                    {
                        Color = ToD2dColor(brush.secondaryColor),
                        Position = 1
                    }
                };

                // Create the gradient stops
                using var gradientStopColl = context.CreateGradientStopCollection(gradientStops);

                // Configure the gradient to go top.down
                var gradientProps = new LinearGradientBrushProperties
                {
                    StartPoint = new Vector2(0, 0),
                    EndPoint = new Vector2(0, 1)
                };

                simpleBrush = context.CreateLinearGradientBrush(
                    gradientProps,
                    gradientStopColl
                );
            }

            brushes[brush] = simpleBrush;
            return simpleBrush;
        }


        // Formatted strings
        private IDWriteTextLayout GetTextLayout(int width, int height, TextStyle style, string text)
        {
            // The maximum width/height of the box may or may not be specified
            float widthF, heightF;
            if (width > 0)
            {
                widthF = width;
            }
            else
            {
                widthF = float.MaxValue;
            }

            if (height > 0)
            {
                heightF = height;
            }
            else
            {
                heightF = float.MaxValue;
            }

            var textFormat = GetTextFormat(style);

            var textLayout = dWriteFactory.CreateTextLayout(
                text,
                textFormat,
                widthF,
                heightF
            );

            if (style.disableLigatures)
            {
                textLayout.SetTypography(_noLigaturesTypography, FullTextRange);
            }

            if (style.trim)
            {
                // Ellipsis for the end of the str
                using var trimmingSign = dWriteFactory.CreateEllipsisTrimmingSign(textFormat);

                var trimming = new Trimming {Granularity = TrimmingGranularity.Character};
                textLayout.SetTrimming(trimming, trimmingSign);
            }

            return textLayout;
        }

        private IDWriteTextLayout GetTextLayout(float width, float height, in FormattedText formatted,
            bool skipDrawingEffects = false)
        {
            // The maximum width/height of the box may or may not be specified
            float widthF, heightF;
            if (width > 0)
            {
                widthF = width;
            }
            else
            {
                widthF = float.MaxValue;
            }

            if (height > 0)
            {
                heightF = height;
            }
            else
            {
                heightF = float.MaxValue;
            }

            var textFormat = GetTextFormat(formatted.DefaultStyle);
            var text = formatted.Text;

            var textLayout = dWriteFactory.CreateTextLayout(
                text,
                textFormat,
                widthF,
                heightF
            );

            if (formatted.DefaultStyle.trim)
            {
                // Ellipsis for the end of the str
                using var trimmingSign = dWriteFactory.CreateEllipsisTrimmingSign(textFormat);

                var trimming = new Trimming {Granularity = TrimmingGranularity.Character};
                textLayout.SetTrimming(trimming, trimmingSign);
            }

            foreach (var range in formatted.Formats)
            {
                var textRange = new TextRange {StartPosition = range.startChar, Length = range.length};

                if (range.style.bold != formatted.DefaultStyle.bold)
                {
                    textLayout.SetFontWeight(GetFontWeight(range.style), textRange);
                }

                if (range.style.italic != formatted.DefaultStyle.italic)
                {
                    textLayout.SetFontStyle(GetFontStyle(range.style), textRange);
                }

                // This will collide with drop shadow drawing for example
                if (!skipDrawingEffects)
                {
                    var rangeBrush = GetBrush(range.style.foreground);
                    textLayout.SetDrawingEffect(rangeBrush, textRange);
                }
            }

            return textLayout;
        }

        // Clipping
        private bool enableClipRect = false;
        private RawRectF clipRect;

        private static readonly Matrix3x2 IdentityMatrix = new(
            1, 0,
            0, 1,
            0, 0
        );

        internal readonly IDWriteTypography _noLigaturesTypography;

        internal void BeginDraw()
        {
            context.BeginDraw();

            if (enableClipRect)
            {
                context.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);
            }
        }

        internal void EndDraw()
        {
            if (enableClipRect)
            {
                context.PopAxisAlignedClip();
            }

            context.EndDraw();
        }

        public TextEngine(ID3D11Device device3d, bool debugDevice)
        {
            this.device3d = device3d;

            // Create the D2D factory
            FactoryOptions factoryOptions;
            if (debugDevice)
            {
                factoryOptions.DebugLevel = DebugLevel.Information;
                Logger.Info("Creating Direct2D Factory (debug=true).");
            }
            else
            {
                factoryOptions.DebugLevel = DebugLevel.None;
                Logger.Info("Creating Direct2D Factory (debug=false).");
            }

            var err = D2D1.D2D1CreateFactory(Vortice.Direct2D1.FactoryType.SingleThreaded, factoryOptions, out factory);
            if (!err.Success)
            {
                throw new GfxException("Failed to initialize D2D1 device: " + err);
            }

            using var dxgiDevice = device3d.QueryInterface<IDXGIDevice>();

            // Create a D2D device on top of the DXGI device
            device = factory.CreateDevice(dxgiDevice);

            // Get Direct2D device's corresponding device context object.
            context = device.CreateDeviceContext(DeviceContextOptions.None);

            // DirectWrite factory
            err = DWrite.DWriteCreateFactory(FactoryType.Shared, out dWriteFactory);
            if (!err.Success)
            {
                throw new GfxException("Failed to initialize DirectWrite factory: " + err);
            }

            // Create our custom font handling ObjectHandles.
            _fontLoader = new FontLoader(dWriteFactory, _fonts);
            dWriteFactory.RegisterFontFileLoader(_fontLoader);
            dWriteFactory.RegisterFontCollectionLoader(_fontLoader);

            context.TextAntialiasMode = Vortice.Direct2D1.TextAntialiasMode.Grayscale;

            // Create some commonly used resources
            _noLigaturesTypography = dWriteFactory.CreateTypography();
            _noLigaturesTypography.AddFontFeature(new FontFeature()
            {
                NameTag = FontFeatureTag.StandardLigatures,
                Parameter = 0
            });
        }

        public void SetScissorRect(float x, float y, float width, float height)
        {
            enableClipRect = true;
            clipRect = new RawRectF(
                x,
                y,
                x + width,
                y + height
            );
        }

        public void ResetScissorRect()
        {
            enableClipRect = false;
        }

        private static FontStyle GetFontStyle(TextStyle textStyle)
        {
            return textStyle.italic ? FontStyle.Italic : FontStyle.Normal;
        }

        private static FontWeight GetFontWeight(TextStyle textStyle)
        {
            return textStyle.bold ? FontWeight.Bold : FontWeight.Normal;
        }

        public TextBlock CreateTextBlock() => new(this);

        public void SetRenderTarget(ID3D11Texture2D renderTarget)
        {
            target?.Dispose();

            if (renderTarget == null)
            {
                context.SetTarget(null);
                return;
            }

            // Get the underlying DXGI surface
            using var dxgiSurface = renderTarget.QueryInterface<IDXGISurface>();

            // Create a D2D RT bitmap for it
            var bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.Unknown, AlphaMode.Ignore),
                96.0f,
                96.0f,
                BitmapOptions.Target | BitmapOptions.CannotDraw
            );

            target = context.CreateBitmapFromDxgiSurface(dxgiSurface, bitmapProperties);

            context.SetTarget(target);
        }

        public void RenderText(RectangleF rect, FormattedText formattedStr)
        {
            BeginDraw();

            float x = rect.X;
            float y = rect.Y;

            using var textLayout = GetTextLayout(rect.Width, rect.Height, formattedStr);

            var metrics = textLayout.Metrics;

            // Draw the drop shadow first as a simple +1, +1 shift
            if (formattedStr.DefaultStyle.dropShadow)
            {
                var shadowLayout = GetTextLayout(rect.Width, rect.Height, formattedStr, true);

                var shadowBrush = GetBrush(formattedStr.DefaultStyle.dropShadowBrush);
                context.DrawTextLayout(
                    new PointF(x + 1, y + 1),
                    shadowLayout,
                    shadowBrush
                );
            }

            // Get applicable brush
            var brush = GetBrush(formattedStr.DefaultStyle.foreground);

            // This is really unpleasant, but DirectWrite doesn't really use a well designed brush
            // coordinate system for drawing text apparently...
            using var gradientBrush = brush.QueryInterfaceOrNull<ID2D1LinearGradientBrush>();
            if (gradientBrush != null)
            {
                gradientBrush.StartPoint = new PointF(0, y + metrics.Top);
                gradientBrush.EndPoint = new PointF(0, y + metrics.Top + metrics.Height);
            }

            context.DrawTextLayout(
                new PointF(x, y),
                textLayout,
                brush
            );

            EndDraw();
        }

        public void RenderTextRotated(RectangleF rect, float angle, Vector2 center, FormattedText formattedStr)
        {
            var transform2d = Matrix3x2.CreateRotation(angle, new Vector2(center.X, center.Y));
            context.Transform = transform2d;

            RenderText(rect, formattedStr);

            context.Transform = IdentityMatrix;
        }

        public void RenderText(Rectangle rect, TextStyle style, string text)
        {
            BeginDraw();

            // Draw the drop shadow first as a simple +1, +1 shift
            if (style.dropShadow)
            {
                var shadowLayout = GetTextLayout(rect.Width, rect.Height, style, text);

                var shadowBrush = GetBrush(style.dropShadowBrush);
                context.DrawTextLayout(
                    new PointF((float) rect.X + 1, (float) rect.Y + 1),
                    shadowLayout,
                    shadowBrush
                );
            }

            var textLayout = GetTextLayout(rect.Width, rect.Height, style, text);

            var metrics = textLayout.Metrics;

            // Get applicable brush
            var brush = GetBrush(style.foreground);

            // This is really unpleasant, but DirectWrite doesn't really use a well designed brush
            // coordinate system for drawing text apparently...
            using var gradientBrush = brush.QueryInterfaceOrNull<ID2D1LinearGradientBrush>();
            if (gradientBrush != null)
            {
                gradientBrush.StartPoint = new PointF(0, rect.Y + metrics.Top);
                gradientBrush.EndPoint = new PointF(0, rect.Y + metrics.Top + metrics.Height);
            }

            context.DrawTextLayout(
                new PointF(rect.X + metrics.Left, rect.Y + metrics.Top),
                textLayout,
                brush
            );

            EndDraw();
        }

        public TextMetrics MeasureText(FormattedText formattedStr, int maxWidth = 0, int maxHeight = 0)
        {
            using var textLayout = GetTextLayout(maxWidth, maxHeight, formattedStr);
            MeasureText(textLayout, out var metrics);
            return metrics;
        }

        public TextMetrics MeasureText(TextStyle style, ReadOnlySpan<char> text, int maxWidth = 0, int maxHeight = 0)
        {
            var textStr = new string(text); // TODO: Sadly SharpDX does not support Span's
            using var textLayout = GetTextLayout(maxWidth, maxHeight, style, textStr);
            MeasureText(textLayout, out var metrics);
            return metrics;
        }

        private void MeasureText(IDWriteTextLayout textLayout, out TextMetrics metrics)
        {
            var lineMetrics = textLayout.GetLineMetrics();

            var dWriteMetrics = textLayout.Metrics;
            metrics.width = (int) MathF.Ceiling(dWriteMetrics.WidthIncludingTrailingWhitespace);
            metrics.height = (int) MathF.Ceiling(dWriteMetrics.Height);
            metrics.lineHeight = (int) MathF.Round(lineMetrics[0].Height);
            metrics.lines = lineMetrics.Length;
        }

        public void AddFont(string filename)
        {
            var fontData = Tig.FS.ReadBinaryFile(filename);

            _fonts.Add(new FontFile(filename, fontData));
            _fontCollection?.Dispose(); // Has to be rebuilt...
            _textFormats.Clear();
        }

        /**
         * Checks if this text engine can provide the given font family. If false, it means it would
         * use a fallback font.
         */
        public bool HasFontFamily(string name)
        {
            if (_fontCollection == null)
            {
                LoadFontCollection();
            }

            return _fontCollection.FindFamilyName(name, out _);
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        public void Dispose()
        {
            dWriteFactory.UnregisterFontFileLoader(_fontLoader);
            dWriteFactory.UnregisterFontCollectionLoader(_fontLoader);

            device3d?.Dispose();
            device?.Dispose();
            factory?.Dispose();
            context?.Dispose();
            target?.Dispose();
            dWriteFactory?.Dispose();
            _fontLoader?.Dispose();
        }
    }
}