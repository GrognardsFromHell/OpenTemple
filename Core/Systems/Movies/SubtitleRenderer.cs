using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;

#nullable enable

namespace OpenTemple.Core.Systems.Movies
{
    public class SubtitleRenderer
    {
        private const int MaxWidth = 700;

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly RenderingDevice _device;
        private readonly MovieSubtitles _subtitles;
        private int _currentSubtitleLine = -1;
        private TextLayout? _currentLineLayout;
        private ComputedStyles? _backgroundStyle;

        public SubtitleRenderer(RenderingDevice device, MovieSubtitles subtitles)
        {
            _device = device;
            _subtitles = subtitles;
        }

        private void Update(double currentTime)
        {
            static bool IsCurrent(in MovieSubtitleLine line, double time)
            {
                return time >= line.StartTime / 1000.0 && time < line.StartTime / 1000.0 + line.Duration / 1000.0;
            }

            if (_currentSubtitleLine != -1 && _currentSubtitleLine < _subtitles.Lines.Count)
            {
                var line = _subtitles.Lines[_currentSubtitleLine];
                if (IsCurrent(line, currentTime))
                {
                    return;
                }
            }

            _currentLineLayout?.Dispose();
            _currentLineLayout = null;

            for (var i = 0; i < _subtitles.Lines.Count; i++)
            {
                var line = _subtitles.Lines[i];
                if (IsCurrent(line, currentTime))
                {
                    Logger.Debug("Subtitle Line @ {0}: '{1}'", line.StartTime, line.Text);
                    _currentSubtitleLine = i;

                    var paragraph = new Paragraph();
                    paragraph.AddStyle("movie-subtitles");
                    paragraph.LocalStyles.Color = line.Color;
                    paragraph.AppendContent(line.Text);
                    _backgroundStyle = paragraph.ComputedStyles;

                    _currentLineLayout = _device.TextEngine.CreateTextLayout(paragraph, MaxWidth, 0);
                    return;
                }
            }

            _currentSubtitleLine = -1;
        }

        public void Render(double currentTime)
        {
            // Update current line
            Update(currentTime);

            if (_currentLineLayout != null)
            {
                var canvasSize = _device.UiCanvasSize;
                var x = (canvasSize.Width - _currentLineLayout.LayoutWidth) / 2;
                // Put lines at 10% margin from bottom
                var y = canvasSize.Height - canvasSize.Height / 10 - _currentLineLayout.OverallHeight;

                // Draw a background
                if (_backgroundStyle != null)
                {
                    var boundingRect = _currentLineLayout.BoundingRectangle;
                    boundingRect.Inflate(4, 4);
                    _device.TextEngine.RenderBackgroundAndBorder(
                        x + boundingRect.X,
                        y + boundingRect.Y,
                        boundingRect.Width,
                        boundingRect.Height,
                        _backgroundStyle
                    );
                }

                _device.TextEngine.RenderTextLayout(x, y, _currentLineLayout);
            }
        }
    };
}