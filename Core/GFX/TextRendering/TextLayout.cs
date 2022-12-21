using System;
using System.Drawing;
using OpenTemple.Interop.Drawing;



namespace OpenTemple.Core.GFX.TextRendering;

public sealed class TextLayout : IDisposable
{
    internal readonly NativeTextLayout NativeTextLayout;

    private bool _metricsInvalid = true;
    private bool _lineMetricsInvalid = true;

    private NativeMetrics _metrics;
    private NativeLineMetrics[] _lineMetrics;

    private bool _isTrimmed;

    public float OverallWidth
    {
        get
        {
            RefreshMetrics();
            return _metrics.Left + _metrics.Width;
        }
    }

    public float OverallHeight
    {
        get
        {
            RefreshMetrics();
            return _metrics.Top + _metrics.Height;
        }
    }

    public int LineCount
    {
        get
        {
            RefreshMetrics();
            return _metrics.LineCount;
        }
    }

    public bool IsTrimmed
    {
        get
        {
            RefreshLineMetrics();
            return _isTrimmed;
        }
    }

    public float LayoutWidth
    {
        get
        {
            RefreshMetrics();
            return _metrics.LayoutWidth;
        }
        set
        {
            NativeTextLayout.SetMaxWidth(value);
            _metricsInvalid = true;
            _lineMetricsInvalid = true;
        }
    }

    public float LayoutHeight
    {
        get
        {
            RefreshMetrics();
            return _metrics.LayoutHeight;
        }
        set
        {
            NativeTextLayout.SetMaxHeight(value);
            _metricsInvalid = true;
            _lineMetricsInvalid = true;
        }
    }

    private void RefreshMetrics()
    {
        if (_metricsInvalid)
        {
            NativeTextLayout.GetMetrics(out _metrics);
            _metricsInvalid = false;
        }
    }

    private void RefreshLineMetrics()
    {
        if (_lineMetricsInvalid)
        {
            _lineMetrics = NativeTextLayout.GetLineMetrics();
            _isTrimmed = false;
            foreach (var lineMetric in _lineMetrics)
            {
                if (lineMetric.IsTrimmed)
                {
                    _isTrimmed = true;
                    break;
                }
            }

            _lineMetricsInvalid = false;
        }
    }

    public TextLayout(NativeTextLayout layout)
    {
        _lineMetrics = Array.Empty<NativeLineMetrics>();
        NativeTextLayout = layout;
    }

    public bool TryHitTest(float x, float y, out int start, out int length)
    {
        return NativeTextLayout.HitTestPoint(x, y, out start, out length, out _);
    }

    public RectangleF HitTestPosition(int index, bool after)
    {
        var rect = NativeTextLayout.HitTestTextPosition(index, after);
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public RectangleF[] HitTestRange(int start, int length)
    {
        var rects = NativeTextLayout.HitTestTextRange(start, length);
        var result = new RectangleF[rects.Length];
        for (var index = 0; index < rects.Length; index++)
        {
            var rect = rects[index];
            result[index] = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        return result;
    }

    public void Dispose()
    {
        NativeTextLayout.Dispose();
    }

    /// <summary>
    /// The bounding rectangle of the text, relative to the layout box initially given to the text layout
    /// </summary>
    public RectangleF BoundingRectangle
    {
        get
        {
            RefreshMetrics();
            return new RectangleF(_metrics.Left, _metrics.Top, _metrics.Width, _metrics.Height);
        }
    }
}