using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using FfmpegBink.Interop;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Movies;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Interop;
using SkiaSharp;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Scenes
{
    public class MoviePlayerScene : Canvas, IScene
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public MoviePlayerScene()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public MoviePlayerScene(string moviePath, MovieSubtitles? subtitles, MoviePlayerFlags moviePlayerFlags,
            int soundtrackId) : this()
        {
            if (!Tig.FS.TryGetRealPath(moviePath, out var fullMoviePath))
            {
                Logger.Warn("Only movies that are present outside of archives can be played: {0}", moviePath);
                return;
            }

            var control = new MoviePlayerControl();
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

    public class MoviePlayerControl : Control
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private VideoPlayer _player;

        private readonly object _mutex = new();

        private byte[] _frameData;
        private int _frameDataWidth;
        private int _frameDataHeight;
        private int _frameDataStride;
        private bool _newFrameData;
        private SKImage _image;
        private SoLoudDynamicSource _soundSource;

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

        public MoviePlayerControl()
        {
            Focusable = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_player != null)
            {
                _player.Stop();
            }
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
            }
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
            lock (_mutex)
            {
                if (_newFrameData)
                {
                    unsafe
                    {
                        fixed (void* pixelData = _frameData)
                        {
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
                var skiaContext = (ISkiaDrawingContextImpl) context.PlatformImpl;
                skiaContext.SkCanvas.DrawImage(
                    _image,
                    new SKRect(0,  0, _image.Width, _image.Height),
                    Bounds.ToSKRect()
                );
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
            if (change.Property == MoviePathProperty)
            {
                _player?.Dispose();
                _player = null;
            }
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

    [Flags]
    public enum MoviePlayerFlags
    {
        Unskippable = 1,
        Flag2 = 2,
        Flag4 = 4,
        Flag8 = 8,
    }
}
