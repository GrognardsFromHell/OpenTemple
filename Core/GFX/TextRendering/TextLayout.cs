using System;
using OpenTemple.Interop.Text;

namespace OpenTemple.Core.GFX.TextRendering
{
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
            get => _metrics.LayoutWidth;
            set
            {
                NativeTextLayout.SetMaxWidth(value);
                _metricsInvalid = true;
            }
        }

        public float LayoutHeight
        {
            get => _metrics.LayoutHeight;
            set
            {
                NativeTextLayout.SetMaxHeight(value);
                _metricsInvalid = true;
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
            NativeTextLayout = layout;
        }

        public bool TryHitTest(float x, float y, out int start, out int length)
        {
            return NativeTextLayout.HitTest(x, y, out start, out length, out _);
        }

        public void Dispose()
        {
            NativeTextLayout?.Dispose();
        }
    }
}