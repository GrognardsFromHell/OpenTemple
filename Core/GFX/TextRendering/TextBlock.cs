using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTemple.Core.Logging;
using SharpGen.Runtime;
using SharpGen.Runtime.Win32;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

namespace OpenTemple.Core.GFX.TextRendering
{
    public sealed class TextBlock : IDisposable
    {
        private const int CaretInvisible = -1;

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static readonly Brush White = new(PackedLinearColorA.White);

        private static readonly Brush DefaultCaretBrush = White;

        private static readonly Brush DefaultSelectionBrush = new(PackedLinearColorA.OfFloats(1, 1, 1, 0.5f));

        /// <summary>
        /// For width and height, this is the tolerance in pixels for them being considered equal.
        /// </summary>
        private const double PixelTolerance = 0.001;

        [NotNull]
        private readonly TextEngine _engine;

        [MaybeNull]
        private IDWriteTextLayout3 _layout;

        [NotNull]
        private string _text = "";

        private float _maxWidth = float.MaxValue;

        private float _maxHeight = float.MaxValue;

        private TextRange _selectionRange;

        [MaybeNull]
        private TextStyle _defaultStyle;

        public Brush CaretBrush { get; set; } = DefaultCaretBrush;

        public Brush SelectionBrush { get; set; } = DefaultSelectionBrush;

        private int _caretPosition;

        public float ScrollX { get; set; }

        public float ScrollY { get; set; }

        internal TextBlock(TextEngine engine)
        {
            _engine = engine;
        }

        public void Dispose()
        {
            FreeLayout();
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                Debug.Assert(value > 0 && value <= float.MaxValue);
                if (Math.Abs(_maxWidth - value) > PixelTolerance)
                {
                    _maxWidth = value;
                    MarkDirty();
                }
            }
        }

        public bool HasMaxWidth => _maxWidth < float.MaxValue;

        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                Debug.Assert(value > 0 && value <= float.MaxValue);
                if (Math.Abs(_maxHeight - value) > PixelTolerance)
                {
                    _maxHeight = value;
                    MarkDirty();
                }
            }
        }

        public bool HasMaxHeight => _maxHeight < float.MaxValue;

        public TextStyle DefaultStyle
        {
            get => _defaultStyle;
            set
            {
                if (_defaultStyle != value)
                {
                    _defaultStyle = value;
                    MarkDirty();
                }
            }
        }

        public RectangleF BoundingBox
        {
            get
            {
                UpdateLayout();
                var metrics = _layout.Metrics;
                return new RectangleF(
                    metrics.Left,
                    metrics.Top,
                    metrics.WidthIncludingTrailingWhitespace,
                    metrics.Height
                );
            }
        }

        public void ClearSelection()
        {
            _selectionRange = default;
        }

        public void SelectAll()
        {
            SetSelection(0, _text.Length);
        }

        public void SetSelection(int from, int length)
        {
            _selectionRange = new TextRange() {StartPosition = from, Length = length};
            ClampSelection();
        }

        private void ClampSelection()
        {
            if (_selectionRange.Length == 0)
            {
                _selectionRange = default;
            }
            else
            {
                _selectionRange.StartPosition = Math.Clamp(_selectionRange.StartPosition, 0, _text.Length - 1);
                _selectionRange.Length =
                    Math.Clamp(_selectionRange.Length, 0, _text.Length - _selectionRange.StartPosition);
            }
        }

        public void HideCaret()
        {
            _caretPosition = CaretInvisible;
        }

        public void ShowCaret(int position)
        {
            _caretPosition = Math.Clamp(position, 0, _text.Length);
        }

        public void Render(float x, float y, RectangleF? clipRect = null)
        {
            UpdateLayout();

            x = MathF.Round(x - ScrollX);
            y = MathF.Round(y - ScrollY);

            _engine.BeginDraw();

            if (clipRect.HasValue)
            {
                _engine.context.PushAxisAlignedClip(clipRect.Value, AntialiasMode.Aliased);
            }

            if (_selectionRange.Length > 0)
            {
                RenderSelection(_selectionRange.StartPosition, _selectionRange.Length, x, y);
            }

            var defaultBrush = _engine.GetBrush(White);

            _engine.context.DrawTextLayout(new PointF(x, y), _layout, defaultBrush);

            if (_caretPosition != CaretInvisible)
            {
                RenderCaret(x, y, _caretPosition);
            }

            if (clipRect.HasValue)
            {
                _engine.context.PopAxisAlignedClip();
            }

            _engine.EndDraw();
        }

        private void RenderCaret(float x, float y, int caretPosition)
        {
            var rect = GetCaretRectangle(x, y, caretPosition);
            var brush = _engine.GetBrush(CaretBrush);
            _engine.context.FillRectangle(rect, brush);
        }

        public RectangleF GetCaretRectangle(float x, float y, int caretPosition)
        {
            UpdateLayout();

            x -= ScrollX;
            y -= ScrollY;

            _layout.HitTestTextPosition(caretPosition, new RawBool(false), out var hitX, out var hitY, out var metrics);

            x += hitX;
            // Shitty solution to ligatures and caret position within the ligature
            if (metrics.Length > 1)
            {
                var offsetInLigature = caretPosition - metrics.TextPosition;
                if (offsetInLigature > 0 && offsetInLigature < metrics.Length)
                {
                    // It's obviously not correct to assume that a ligature is evenly spaced
                    // Supposedly, OpenType has tables that specify caret positions within ligatures,
                    // But I don't know how to access these here. This also breaks for RTL text and similar
                    x += metrics.Width / metrics.Length * offsetInLigature;
                }
            }

            x = MathF.Round(x);
            y = MathF.Round(y + hitY);

            return new RectangleF(x, y, 1, metrics.Height);
        }

        private void RenderSelection(int start, int length, float x, float y)
        {
            Debug.Assert(start >= 0 && start < _text.Length);
            Debug.Assert(length > 0 && start + length <= _text.Length);

            var err = _layout.HitTestTextRange(start, length, x, y, null, 0, out var count);
            if (!err.Success && err.Code != ErrorCodeHelper.ToResult(ErrorCode.InsufficientBuffer))
            {
                Logger.Warn("Failed to hit-test text-layout during rendering selection [{0}-{1}] for text '{2}': {3}",
                    start, start + length, _text, err);
                return;
            }

            if (count == 0)
            {
                return;
            }

            var pool = ArrayPool<HitTestMetrics>.Shared.Rent(count);
            try
            {
                if (!_layout.HitTestTextRange(start, length, x, y, pool, pool.Length, out count).Success)
                {
                    Logger.Warn("Failed to hit-test text-layout during rendering selection [{0}-{1}] for text '{2}'",
                        start, start + length, _text);
                    return;
                }

                RenderSelection(pool.AsSpan(0, count));
            }
            finally
            {
                ArrayPool<HitTestMetrics>.Shared.Return(pool);
            }
        }

        private void RenderSelection(Span<HitTestMetrics> metrics)
        {
            var brush = _engine.GetBrush(SelectionBrush);

            var prevAntiAliasMode = _engine.context.AntialiasMode;
            _engine.context.AntialiasMode = AntialiasMode.Aliased;
            try
            {
                foreach (ref var metric in metrics)
                {
                    RectangleF rect = new(metric.Left, metric.Top, metric.Width, metric.Height);
                    _engine.context.FillRectangle(in rect, brush);
                }
            }
            finally
            {
                _engine.context.AntialiasMode = prevAntiAliasMode;
            }
        }

        private void UpdateLayout()
        {
            if (_layout != null)
            {
                return;
            }

            var defaultFormat =
                _defaultStyle != null ? _engine.GetTextFormat(_defaultStyle) : _engine.DefaultTextFormat;

            var layout = _engine.dWriteFactory.CreateTextLayout(
                _text,
                defaultFormat,
                _maxWidth,
                _maxHeight
            );

            if (_defaultStyle != null && _defaultStyle.disableLigatures)
            {
                layout.SetTypography(_engine._noLigaturesTypography, new TextRange(){StartPosition = 0, Length = -1});
            }

            _layout = layout.QueryInterface<IDWriteTextLayout3>();
        }

        public void SetText([NotNull]
            string text)
        {
            FreeLayout();
            _text = text;
        }

        private void FreeLayout()
        {
            _layout?.Dispose();
            _layout = null;
        }

        private void MarkDirty()
        {
            FreeLayout();
        }

        public bool HitTest(float x, float y, out int closestPosition)
        {
            UpdateLayout();

            x -= ScrollX;
            y -= ScrollY;

            _layout.HitTestPoint(x, y, out var trailingHitRaw, out var insideRaw, out var metrics);
            closestPosition = metrics.TextPosition;
            if (trailingHitRaw)
            {
                closestPosition++;
            }
            return insideRaw;
        }

    }
}