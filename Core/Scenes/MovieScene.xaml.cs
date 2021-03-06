using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using FfmpegBink.Interop;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Movies;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Interop;
using SkiaSharp;
using Canvas = OpenTemple.Widgets.Canvas;
using TextBlock = OpenTemple.Widgets.TextBlock;

#nullable enable

namespace OpenTemple.Core.Scenes
{
    public class MoviePlayerScene : Canvas, IScene
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public MoviePlayerScene()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public MoviePlayerScene(string moviePath, MovieSubtitles? subtitles) : this()
        {
            if (!Tig.FS.TryGetRealPath(moviePath, out var fullMoviePath))
            {
                Logger.Warn("Only movies that are present outside of archives can be played: {0}", moviePath);
                return;
            }

            var control = new MoviePlayerControl();
            control.Subtitles = subtitles;
            control.MoviePath = fullMoviePath;
            control.OnEnd += (_, _) => IsAtEnd = true;
            SetLeft(control, 0.0);
            SetTop(control, 0.0);
            SetRight(control, 0.0);
            SetBottom(control, 0.0);

            Children.Add(control);
        }

        public IControl UiContent => this;

        public bool IsAtEnd { get; private set; }
    }

    public class MoviePlayerControl : Panel
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static readonly SKPaint HighQualityPaint = new () {FilterQuality = SKFilterQuality.High};

        private readonly object _mutex = new();
        private readonly TextBlock _subtitleText;

        private VideoPlayer? _player;
        private byte[]? _frameData;
        private int _frameDataWidth;
        private int _frameDataHeight;
        private int _frameDataStride;
        private bool _newFrameData;
        private SKImage? _image;
        private SoLoudDynamicSource? _soundSource;
        private double _currentTime;
        private int _currentSubtitleLine;

        public static readonly RoutedEvent<RoutedEventArgs> EndEvent =
            RoutedEvent.Register<RoutedEventArgs>(nameof(OnEnd), RoutingStrategies.Bubble, typeof(MoviePlayerScene));

        public event EventHandler<RoutedEventArgs> OnEnd
        {
            add => AddHandler(EndEvent, value);
            remove => RemoveHandler(EndEvent, value);
        }

        public static readonly AvaloniaProperty MoviePathProperty =
            AvaloniaProperty.Register<MoviePlayerControl, string>(nameof(MoviePath));

        public string MoviePath
        {
            get => (string) GetValue(MoviePathProperty);
            set => SetValue(MoviePathProperty, value);
        }

        public static readonly AvaloniaProperty SubtitlesProperty =
            AvaloniaProperty.Register<MoviePlayerControl, MovieSubtitles>(nameof(Subtitles));

        public MovieSubtitles? Subtitles
        {
            get => (MovieSubtitles?) GetValue(SubtitlesProperty);
            set => SetValue(SubtitlesProperty, value);
        }

        public MoviePlayerControl()
        {
            Focusable = true;

            _subtitleText = new TextBlock();
            _subtitleText.Classes.Add("subtitle");
            LogicalChildren.Add(_subtitleText);
            VisualChildren.Add(_subtitleText);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_player != null)
            {
                _player.Stop();
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_player != null)
            {
                _player.Stop();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return Size.Empty;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var subtitleSize = _subtitleText.DesiredSize;
            var x = (finalSize.Width - subtitleSize.Width) / 2;
            var y = 0.9 * finalSize.Height; // Position at 90% of height
            _subtitleText.Arrange(new Rect(new Point(x, y), subtitleSize));
            return finalSize;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _player = new VideoPlayer();
            _player.OnVideoFrame += PlayerOnOnVideoFrame;
            _player.OnAudioSamples += PlayerOnOnAudioSamples;
            _player.OnStop += PlayerOnOnStop;
            if (!_player.Open(MoviePath))
            {
                Logger.Warn("Failed to open movie {0}: {1}", MoviePath, _player.Error);
                return;
            }

            Focus();

            if (_player.HasAudio)
            {
                Logger.Info("Movie Audio: {0} Channels, {1}Hz", _player.AudioChannels, _player.AudioSampleRate);
                _soundSource = new SoLoudDynamicSource(_player.AudioChannels, _player.AudioSampleRate);
                Tig.Sound.PlayDynamicSource(_soundSource);
            }

            _player.Play();
        }

        private void PlayerOnOnVideoFrame(in VideoFrame frame)
        {
            lock (_mutex)
            {
                if (_frameData == null || _frameData.Length != frame.PixelData.Length)
                {
                    _frameData = new byte[frame.PixelData.Length];
                }

                frame.PixelData.CopyTo(_frameData);
                _frameDataStride = frame.Stride;
                _frameDataWidth = frame.Width;
                _frameDataHeight = frame.Height;
                _newFrameData = true;

                _currentTime = frame.Time;
            }

            Dispatcher.UIThread.Post(UpdateSubtitles);
            Dispatcher.UIThread.Post(InvalidateVisual);
        }

        private void PlayerOnOnAudioSamples(in AudioSamples samples)
        {
            if (_soundSource != null)
            {
                _soundSource.PushSamples(samples.Plane1, samples.Plane2);
            }
        }

        private void PlayerOnOnStop()
        {
            TriggerStop();
        }

        public override void Render(DrawingContext context)
        {
            // Update the Skia image if needed
            lock (_mutex)
            {
                if (_newFrameData)
                {
                    unsafe
                    {
                        fixed (void* pixelData = _frameData)
                        {
                            _image?.Dispose();
                            _image = SKImage.FromPixelCopy(
                                new SKImageInfo(
                                    _frameDataWidth,
                                    _frameDataHeight,
                                    SKColorType.Bgra8888
                                ),
                                (IntPtr) pixelData,
                                _frameDataStride
                            );
                        }
                    }
                }
            }

            if (_image != null)
            {
                context.Custom(new DrawingOp(_image, Bounds));
            }
        }

        private class DrawingOp : ICustomDrawOperation
        {
            private readonly SKImage _image;

            public DrawingOp(SKImage image, Rect bounds)
            {
                Bounds = bounds;
                _image = image;
            }

            public void Dispose()
            {
            }

            public bool HitTest(Point p) => true;

            public void Render(IDrawingContextImpl context)
            {
                var videoSource = new Rect(0, 0, _image.Width, _image.Height);

                // Fit movie into rect
                var wFactor = Bounds.Width / videoSource.Width;
                var hFactor = Bounds.Height / videoSource.Height;
                var scale = Math.Min(wFactor, hFactor);
                var destRect = Bounds.CenterRect(videoSource * scale);

                var skiaContext = (ISkiaDrawingContextImpl) context;
                skiaContext.SkCanvas.DrawImage(
                    _image,
                    videoSource.ToSKRect(),
                    destRect.ToSKRect(),
                    HighQualityPaint
                );
            }

            public Rect Bounds { get; }

            public bool Equals(ICustomDrawOperation? other)
            {
                return other is DrawingOp op
                       && ReferenceEquals(_image, op._image)
                       && Bounds == op.Bounds;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            lock (_mutex)
            {
                _soundSource?.Dispose();
                _soundSource = null;

                _player?.Dispose();
                _player = null;
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == SubtitlesProperty)
            {
                UpdateSubtitles();
            }
            else if (change.Property == MoviePathProperty)
            {
                _player?.Dispose();
                _player = null;
            }
        }

        private void UpdateSubtitles()
        {
            var subtitles = Subtitles;
            if (subtitles == null)
            {
                _currentSubtitleLine = -1;
                _subtitleText.Text = null;
                return;
            }

            static bool IsCurrent(in MovieSubtitleLine line, double time)
            {
                return time >= line.StartTime / 1000.0 && time < line.StartTime / 1000.0 + line.Duration / 1000.0;
            }

            if (_currentSubtitleLine != -1 && _currentSubtitleLine < subtitles.Lines.Count)
            {
                var line = subtitles.Lines[_currentSubtitleLine];
                if (IsCurrent(line, _currentTime))
                {
                    return;
                }
            }

            for (var i = 0; i < subtitles.Lines.Count; i++)
            {
                var line = subtitles.Lines[i];
                if (IsCurrent(line, _currentTime))
                {
                    Logger.Debug("Subtitle Line @ {0}: '{1}'", line.StartTime, line.Text);
                    _currentSubtitleLine = i;
                    _subtitleText.Text = line.Text;
                    _subtitleText.Foreground = new SolidColorBrush(line.Color.Pack());

                    _subtitleText.Measure(new Size(_subtitleText.MaxWidth, _subtitleText.MaxHeight));
                    var x = (int) ((Bounds.Width - _subtitleText.DesiredSize.Width) / 2);
                    var y = (int) (Bounds.Height - Bounds.Height / 10);
                    _subtitleText.Arrange(new Rect(x, y, _subtitleText.DesiredSize.Width,
                        _subtitleText.DesiredSize.Height));

                    return;
                }
            }

            _currentSubtitleLine = -1;
            _subtitleText.Text = null;
        }

        private void TriggerStop()
        {
            Dispatcher.UIThread.Post(() =>
            {
                var e = new RoutedEventArgs(EndEvent);
                RaiseEvent(e);
            });
        }
    }
}
