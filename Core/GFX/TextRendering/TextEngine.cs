using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Interop.Drawing;
using RectangleF = System.Drawing.RectangleF;

#nullable enable

namespace OpenTemple.Core.GFX.TextRendering;

public class TextEngine : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private bool _batching;

    private readonly NativeDrawingEngine _nativeEngine;

    public TextEngine(IntPtr d3d11Device, bool debugDevice)
    {
        _nativeEngine = new NativeDrawingEngine(d3d11Device, debugDevice);
    }

    // Clipping
    private bool _enableClipRect;
    private RectangleF _clipRect;

    private void BeginDraw()
    {
        if (_batching)
        {
            return;
        }

        _nativeEngine.BeginDraw();

        if (_enableClipRect)
        {
            _nativeEngine.PushClipRect(_clipRect, false);
        }
    }

    private void EndDraw()
    {
        if (_batching)
        {
            return;
        }

        if (_enableClipRect)
        {
            _nativeEngine.PopClipRect();
        }

        _nativeEngine.EndDraw();
    }

    public void BeginBatch()
    {
        if (_batching)
        {
            throw new InvalidOperationException("Recursive call to " + nameof(BeginBatch));
        }

        BeginDraw();
        _batching = true;
    }

    public void EndBatch()
    {
        if (!_batching)
        {
            throw new InvalidOperationException("Cannot call " + nameof(EndBatch) + " without calling " +
                                                nameof(BeginBatch));
        }

        _batching = false;
        EndDraw();
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        if (_batching)
        {
            throw new InvalidOperationException("Cannot call " + nameof(SetScissorRect) + " while batching");
        }

        _enableClipRect = true;
        _clipRect = new RectangleF(
            x,
            y,
            width,
            height
        );
    }

    public void ResetScissorRect()
    {
        if (_batching)
        {
            throw new InvalidOperationException("Cannot call " + nameof(ResetScissorRect) + " while batching");
        }

        _enableClipRect = false;
    }

    private SizeF _canvasSize;

    public SizeF CanvasSize
    {
        get => _canvasSize;
        set
        {
            _canvasSize = value;
            _nativeEngine.SetCanvasSize(value);
        }
    }

    public void SetRenderTarget(IntPtr d3d11RenderTargetTexture)
    {
        _nativeEngine.SetRenderTarget(d3d11RenderTargetTexture);
    }

    public void RenderText(
        RectangleF rect,
        ComputedStyles styles,
        ReadOnlySpan<char> text
    )
    {
        var nativeParagraphStyle = CreateParagraphStyle(styles);
        var nativeTextStyle = CreateTextStyle(styles);

        using var layout = CreateNativeTextLayout(text, ref nativeParagraphStyle, ref nativeTextStyle,
            rect.Width, rect.Height);

        if (!_batching)
        {
            BeginDraw();
        }
        _nativeEngine.RenderTextLayout(layout, rect.X, rect.Y, 1f);
        if (!_batching)
        {
            EndDraw();
        }
    }

    public TextMetrics MeasureText(ComputedStyles styles,
        ReadOnlySpan<char> text, int maxWidth = 0, int maxHeight = 0)
    {
        var nativeParagraphStyle = CreateParagraphStyle(styles);
        var nativeTextStyle = CreateTextStyle(styles);

        using var layout = _nativeEngine.CreateTextLayout(ref nativeParagraphStyle, ref nativeTextStyle,
            text, maxWidth, maxHeight);

        layout.GetMetrics(out var metrics);

        return new TextMetrics()
        {
            width = metrics.Width,
            height = metrics.Height,
            lines = metrics.LineCount
        };
    }

    public void AddFont(string filename)
    {
        using var ownedFontData = Tig.FS.ReadFile(filename);
        _nativeEngine.AddFontFile(filename, ownedFontData.Memory.Span);
        _nativeEngine.ReloadFontFamilies();
    }

    public void LogLoadedFontFamilies()
    {
        var fontFamilies = _nativeEngine.FontFamilies;
        Logger.Info("Loaded {0} additional font families", fontFamilies.Count);
        foreach (var fontFamily in fontFamilies)
        {
            Logger.Info(" - '{0}'", fontFamily);
        }
    }

    public void Dispose()
    {
        _nativeEngine.Dispose();
    }

    public TextLayout CreateTextLayout(ComputedStyles styles, ReadOnlySpan<char> text, float maxWidth,
        float maxHeight)
    {
        var nativeParagraphStyle = CreateParagraphStyle(styles);
        var nativeTextStyle = CreateTextStyle(styles);

        var layout = CreateNativeTextLayout(
            text,
            ref nativeParagraphStyle,
            ref nativeTextStyle,
            maxWidth,
            maxHeight
        );

        return new TextLayout(layout);
    }

    public TextLayout CreateTextLayout(Paragraph paragraph, float maxWidth, float maxHeight)
    {
        var flow = paragraph.TextFlow;

        var nativeParagraphStyle = CreateParagraphStyle(paragraph.ComputedStyles);
        var nativeTextStyle = CreateTextStyle(paragraph.ComputedStyles);

        var nativeTextLayout = CreateNativeTextLayout(flow.Text, ref nativeParagraphStyle, ref nativeTextStyle,
            maxWidth, maxHeight);

        foreach (var element in flow.Elements)
        {
            var elementStyle = CreateTextStyle(element.Source.ComputedStyles);
            NativeTextStyleProperty properties = default;

            if (elementStyle.FontFace != nativeTextStyle.FontFace)
            {
                properties |= NativeTextStyleProperty.FontFace;
            }

            if (Math.Abs(elementStyle.FontSize - nativeTextStyle.FontSize) > 0.1f)
            {
                properties |= NativeTextStyleProperty.FontSize;
            }

            if (elementStyle.Color != nativeTextStyle.Color)
            {
                properties |= NativeTextStyleProperty.Color;
            }

            if (elementStyle.Underline != nativeTextStyle.Underline)
            {
                properties |= NativeTextStyleProperty.Underline;
            }

            if (elementStyle.LineThrough != nativeTextStyle.LineThrough)
            {
                properties |= NativeTextStyleProperty.LineThrough;
            }

            if (elementStyle.FontStretch != nativeTextStyle.FontStretch)
            {
                properties |= NativeTextStyleProperty.FontStretch;
            }

            if (elementStyle.FontStyle != nativeTextStyle.FontStyle)
            {
                properties |= NativeTextStyleProperty.FontStyle;
            }

            if (elementStyle.FontWeight != nativeTextStyle.FontWeight)
            {
                properties |= NativeTextStyleProperty.FontWeight;
            }

            nativeTextLayout.SetStyle(element.Start, element.Length, properties, ref elementStyle);
        }

        return new TextLayout(nativeTextLayout);
    }

    private NativeTextLayout CreateNativeTextLayout(
        ReadOnlySpan<char> text,
        ref NativeParagraphStyle nativeParagraphStyle,
        ref NativeTextStyle nativeTextStyle,
        float maxWidth,
        float maxHeight
    )
    {
        return _nativeEngine.CreateTextLayout(
            ref nativeParagraphStyle,
            ref nativeTextStyle,
            text,
            maxWidth,
            maxHeight
        );
    }

    private static NativeParagraphStyle CreateParagraphStyle(ComputedStyles style)
    {
        NativeParagraphStyle nativeStyle;
        nativeStyle.HangingIndent = style.HangingIndent;
        nativeStyle.Indent = style.Indent;
        nativeStyle.TabStopWidth = style.TabStopWidth;
        nativeStyle.TextAlignment = (NativeTextAlign) style.TextAlignment;
        nativeStyle.ParagraphAlignment = (NativeParagraphAlign) style.ParagraphAlignment;
        nativeStyle.WordWrap = (NativeWordWrap) style.WordWrap;
        nativeStyle.TrimMode = (NativeTrimMode) style.TrimMode;
        nativeStyle.TrimmingSign = (NativeTrimmingSign) style.TrimmingSign;
        return nativeStyle;
    }

    private static NativeTextStyle CreateTextStyle(ComputedStyles style)
    {
        NativeTextStyle nativeStyle;
        nativeStyle.FontFace = style.FontFace;
        nativeStyle.FontSize = style.FontSize;
        nativeStyle.Color = style.Color.Pack();
        nativeStyle.Underline = style.Underline;
        nativeStyle.LineThrough = style.LineThrough;
        nativeStyle.FontStretch = (NativeFontStretch) style.FontStretch;
        nativeStyle.FontStyle = (NativeFontStyle) style.FontStyle;
        nativeStyle.FontWeight = (NativeFontWeight) style.FontWeight;
        nativeStyle.DropShadowColor = style.DropShadowColor.Pack();
        nativeStyle.OutlineColor = style.OutlineColor.Pack();
        nativeStyle.OutlineWidth = style.OutlineWidth;
        nativeStyle.Kerning = false;
        return nativeStyle;
    }

    public void RenderBackgroundAndBorder(float x, float y, float width, float height, ComputedStyles styles)
    {
        if (styles.BackgroundColor.A <= 0 && (!(styles.BorderWidth > 0) || styles.BorderColor.A <= 0))
        {
            return;
        }

        if (!_batching)
        {
            BeginDraw();
        }

        NativeBackgroundAndBorderStyle nativeStyle;
        nativeStyle.BackgroundColor = styles.BackgroundColor.Pack();
        nativeStyle.BorderWidth = styles.BorderWidth;
        nativeStyle.BorderColor = styles.BorderColor.Pack();
        nativeStyle.RadiusX = 0;
        nativeStyle.RadiusY = 0;
        _nativeEngine.RenderBackgroundAndBorder(
            x, y, width, height,
            ref nativeStyle
        );

        if (!_batching)
        {
            EndDraw();
        }
    }

    public void RenderTextLayout(float x, float y, TextLayout textLayout, in TextRenderOptions options = default)
    {
        Matrix3x2 currentTransform;
        if (options.HasRotation)
        {
            var transform2d = Matrix3x2.CreateRotation(options.RotationAngle, new Vector2(options.RotationCenter.X, options.RotationCenter.Y));
            _nativeEngine.GetTransform(out currentTransform);
            _nativeEngine.SetTransform(ref transform2d);
        }
        else
        {
            currentTransform = default;
        }

        if (!_batching)
        {
            BeginDraw();
        }

        var opacity = options.HasOpacity ? options.Opacity : 1f;

        _nativeEngine.RenderTextLayout(
            textLayout.NativeTextLayout,
            x,
            y,
            opacity
        );

        if (!_batching)
        {
            EndDraw();
        }

        if (options.HasRotation)
        {
            _nativeEngine.SetTransform(ref currentTransform);
        }
    }
}