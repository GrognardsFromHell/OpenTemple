using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using D3D11Device = SharpDX.Direct3D11.Device;
using D2D1Device = SharpDX.Direct2D1.Device;
using D2D1Factory1 = SharpDX.Direct2D1.Factory1;
using D2D1DeviceContext = SharpDX.Direct2D1.DeviceContext;
using D2D1Bitmap1 = SharpDX.Direct2D1.Bitmap1;
using DXGIDevice = SharpDX.DXGI.Device;
using DWriteFactory = SharpDX.DirectWrite.Factory1;
using DWriteFontCollection = SharpDX.DirectWrite.FontCollection;
using DWriteTextFormat = SharpDX.DirectWrite.TextFormat;
using D2D1Brush = SharpDX.Direct2D1.Brush;
using DWriteTextLayout = SharpDX.DirectWrite.TextLayout;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using DWriteInlineObject = SharpDX.DirectWrite.InlineObject;
using DXGISurface = SharpDX.DXGI.Surface;
using DataStream = SharpDX.DataStream;
using DataPointer = SharpDX.DataPointer;
using FontStyle = SharpDX.DirectWrite.FontStyle;
using Matrix3x2 = SharpDX.Matrix3x2;

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
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly DirectXDevices _devices;

        /*
            Direct2D resources
        */

        internal D2D1Device device => null;

        internal D2D1DeviceContext context;

        private D2D1Bitmap1 target;

        /*
            DirectWrite resources
        */
        internal DWriteFactory dWriteFactory =>_devices.DirectWriteFactory;

        /*
            Custom font handling
        */
        private FontLoader fontLoader;

        private List<FontFile> fonts = new List<FontFile>();

        private DWriteFontCollection fontCollection;

        // DirectWrite caches font collections internally.
        // If we reload, we need to generate a new key
        private readonly DataStream _fontCollKeyStream = new DataStream(sizeof(int), true, true);
        private int _fontCollKey = 0;

        private void LoadFontCollection()
        {
            Logger.Info("Reloading font collection...");
            _fontCollKeyStream.Position = 0;
            _fontCollKeyStream.Write(_fontCollKey++);
            fontCollection = new FontCollection(
                dWriteFactory,
                fontLoader,
                new DataPointer(_fontCollKeyStream.DataPointer, (int) _fontCollKeyStream.Length)
            );

            // Enumerate all loaded fonts to the logger
            var count = fontCollection.FontFamilyCount;
            for (var i = 0; i < count; i++)
            {
                using var family = fontCollection.GetFontFamily(i);

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
        private readonly Dictionary<TextStyle, DWriteTextFormat> textFormats =
            new Dictionary<TextStyle, TextFormat>(new TextStyleEqualityComparer());

        private DWriteTextFormat GetTextFormat(TextStyle textStyle)
        {
            if (textFormats.TryGetValue(textStyle, out var existingFormat))
            {
                return existingFormat;
            }

            var fontWeight = GetFontWeight(textStyle);
            var fontStyle = GetFontStyle(textStyle);

            // Lazily build the font collection
            if (fontCollection == null) {
                LoadFontCollection();
            }

            // Lazily create the text format
            var textFormat = new DWriteTextFormat(
                dWriteFactory,
                textStyle.fontFace,
                fontCollection,
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

            switch (textStyle.align) {
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

            switch (textStyle.paragraphAlign) {
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

            if (textStyle.uniformLineHeight) {
                textFormat.SetLineSpacing(
                    LineSpacingMethod.Uniform,
                    textStyle.lineHeight,
                    textStyle.baseLine
                    );
            }

            textFormats[textStyle] = textFormat;
            return textFormat;
        }

        // Brush cache
        private Dictionary<Brush, D2D1Brush> brushes = new Dictionary<Brush, D2D1Brush>(new BrushEqualityComparer());

        private static RawColor4 ToD2dColor(PackedLinearColorA color)
        {
            return new RawColor4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            );
        }

        private D2D1Brush GetBrush(in Brush brush)
        {
            if (brushes.TryGetValue(brush, out var existingBrush))
            {
                return existingBrush;
            }

            D2D1Brush simpleBrush;

            if (!brush.gradient) {
                var colorValue = ToD2dColor(brush.primaryColor);
                simpleBrush = new SolidColorBrush(context, colorValue);
            } else {
                var gradientStops = new []
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
                using var gradientStopColl = new GradientStopCollection(
                    context,
                    gradientStops
                );

                // Configure the gradient to go top.down
                var gradientProps = new LinearGradientBrushProperties
                {
                    StartPoint = new RawVector2(0, 0),
                    EndPoint = new RawVector2(0, 1)
                };

                simpleBrush = new LinearGradientBrush(
                    context,
                    gradientProps,
                    gradientStopColl
                );
            }

            brushes[brush] = simpleBrush;
            return simpleBrush;
        }


        // Formatted strings
        private DWriteTextLayout GetTextLayout(int width, int height, TextStyle style, string text)
        {

            // The maximum width/height of the box may or may not be specified
            float widthF, heightF;
            if (width > 0) {
                widthF = width;
            } else
            {
                widthF = float.MaxValue;
            }
            if (height > 0) {
                heightF = height;
            }
            else
            {
                heightF = float.MaxValue;
            }

            var textFormat = GetTextFormat(style);

            var textLayout = new DWriteTextLayout(
                dWriteFactory,
                text,
                textFormat,
                widthF,
                heightF
            );

            if (style.trim) {
                // Ellipsis for the end of the str
                using var trimmingSign = new EllipsisTrimming(dWriteFactory, textFormat);

                var trimming = new Trimming {Granularity = TrimmingGranularity.Character};
                textLayout.SetTrimming(trimming, trimmingSign);
            }

            return textLayout;

        }

        private DWriteTextLayout GetTextLayout(int width, int height, in FormattedText formatted,
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

            var textFormat = GetTextFormat(formatted.defaultStyle);
            var text = formatted.text;

            var textLayout = new DWriteTextLayout(
                dWriteFactory,
                text,
                textFormat,
                widthF,
                heightF
            );

            if (formatted.defaultStyle.trim) {
                // Ellipsis for the end of the str
                using var trimmingSign = new EllipsisTrimming(dWriteFactory, textFormat);

                var trimming = new Trimming {Granularity = TrimmingGranularity.Character};
                textLayout.SetTrimming(trimming, trimmingSign);
            }

            foreach (var range in formatted.Formats)
            {
                var textRange = new TextRange(range.startChar, range.length);

                if (range.style.bold != formatted.defaultStyle.bold)
                {
                    textLayout.SetFontWeight(GetFontWeight(range.style), textRange);
                }

                if (range.style.italic != formatted.defaultStyle.italic)
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
        private RawRectangleF clipRect;

        private void BeginDraw()
        {
            context.BeginDraw();

            if (enableClipRect) {
                context.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);
            }
        }

        private void EndDraw()
        {
            if (enableClipRect) {
                context.PopAxisAlignedClip();
            }

            context.EndDraw();
        }

        public TextEngine(DirectXDevices devices)
        {
            _devices = devices;

            // Get Direct2D device's corresponding device context object.
            // context = new D2D1DeviceContext(device, DeviceContextOptions.None);

            // Create our custom font handling ObjectHandles.
            fontLoader = new FontLoader(dWriteFactory, fonts);
            dWriteFactory.RegisterFontCollectionLoader(fontLoader);
            dWriteFactory.RegisterFontFileLoader(fontLoader);

            // context.TextAntialiasMode = TextAntialiasMode.Grayscale;
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
            enableClipRect = true;
            clipRect = new RawRectangleF(
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


        private static FontStyle GetFontStyle(TextStyle textStyle) {
            return textStyle.italic ? FontStyle.Italic : FontStyle.Normal;
        }

        private static FontWeight GetFontWeight(TextStyle textStyle) {
            return textStyle.bold ? FontWeight.Bold : FontWeight.Normal;
        }

        public void SetRenderTarget(Texture2D renderTarget)
        {
            return;
            target?.Dispose();

            if (renderTarget == null)
            {
                context.Target = null;
                return;
            }

            // Get the underlying DXGI surface
            using var dxgiSurface = renderTarget.QueryInterface<DXGISurface>();

            // Create a D2D RT bitmap for it
            var bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.Unknown, AlphaMode.Ignore),
                96.0f,
                96.0f,
                BitmapOptions.Target | BitmapOptions.CannotDraw
            );

            // target = new D2D1Bitmap1(context, dxgiSurface, bitmapProperties);

            // context.Target = target;
        }

        public void RenderText(Rectangle rect, FormattedText formattedStr)
        {

            BeginDraw();

            float x = (float) rect.X;
            float y = (float) rect.Y;

            using var textLayout = GetTextLayout(rect.Width, rect.Height, formattedStr);

            var metrics = textLayout.Metrics;

            // Draw the drop shadow first as a simple +1, +1 shift
            if (formattedStr.defaultStyle.dropShadow) {
                var shadowLayout = GetTextLayout(rect.Width, rect.Height, formattedStr, true);

                var shadowBrush = GetBrush(formattedStr.defaultStyle.dropShadowBrush);
                context.DrawTextLayout(
                    new RawVector2( x + 1, y + 1 ),
                    shadowLayout,
                    shadowBrush
                );
            }

            // Get applicable brush
            var brush = GetBrush(formattedStr.defaultStyle.foreground);

            // This is really unpleasant, but DirectWrite doesn't really use a well designed brush
            // coordinate system for drawing text apparently...
            using var gradientBrush = brush.QueryInterfaceOrNull<LinearGradientBrush>();
            if (gradientBrush != null)
            {
                gradientBrush.StartPoint = new RawVector2(0, y + metrics.Top);
                gradientBrush.EndPoint = new RawVector2(0, y + metrics.Top + metrics.Height);
            }

            context.DrawTextLayout(
                new RawVector2(x, y),
                textLayout,
                brush
            );

            EndDraw();
        }

        private static readonly RawMatrix3x2 IdentityMatrix = new RawMatrix3x2(
            1, 0,
            0, 1,
            0, 0
        );

        public void RenderTextRotated(Rectangle rect, float angle, Vector2 center, FormattedText formattedStr)
        {
            var transform2d = Matrix3x2.Rotation(angle, new RawVector2(center.X, center.Y));
            context.Transform = transform2d;

            RenderText(rect, formattedStr);

            context.Transform = IdentityMatrix;
        }

        public void RenderText(Rectangle rect, TextStyle style, string text)
        {

            BeginDraw();

            // Draw the drop shadow first as a simple +1, +1 shift
            if (style.dropShadow) {

                var shadowLayout = GetTextLayout(rect.Width, rect.Height, style, text);

                var shadowBrush = GetBrush(style.dropShadowBrush);
                context.DrawTextLayout(
                    new RawVector2((float) rect.X + 1, (float) rect.Y + 1),
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
            using var gradientBrush = brush.QueryInterfaceOrNull<LinearGradientBrush>();
            if (gradientBrush != null)
            {
                gradientBrush.StartPoint = new RawVector2(0, rect.Y + metrics.Top);
                gradientBrush.EndPoint = new RawVector2(0, rect.Y + metrics.Top + metrics.Height);
            }

            context.DrawTextLayout(
                new RawVector2( rect.X + metrics.Left, rect.Y + metrics.Top ),
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

        private void MeasureText(TextLayout textLayout, out TextMetrics metrics)
        {
            var lineMetrics = textLayout.GetLineMetrics();

            var dWriteMetrics = textLayout.Metrics;
            metrics.width = (int)MathF.Ceiling(dWriteMetrics.WidthIncludingTrailingWhitespace);
            metrics.height = (int)MathF.Ceiling(dWriteMetrics.Height);
            metrics.lineHeight = (int)MathF.Round(lineMetrics[0].Height);
            metrics.lines = lineMetrics.Length;
        }

        public void AddFont(string filename)
        {
            var fontData = Tig.FS.ReadBinaryFile(filename);

            fonts.Add(new FontFile(filename, fontData));
            fontCollection?.Dispose(); // Has to be rebuilt...
            textFormats.Clear();
        }

        /**
         * Checks if this text engine can provide the given font family. If false, it means it would
         * use a fallback font.
         */
        public bool HasFontFamily(string name)
        {
            if (fontCollection == null) {
                LoadFontCollection();
            }

            return fontCollection.FindFamilyName(name, out _);
        }

        public void Dispose()
        {
            dWriteFactory.UnregisterFontFileLoader(fontLoader);
            dWriteFactory.UnregisterFontCollectionLoader(fontLoader);

            _fontCollKeyStream.Dispose();
        }
    }
}