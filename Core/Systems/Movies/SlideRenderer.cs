#nullable enable
using System;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Systems.Movies
{
    public class SlideRenderer : IDisposable
    {
        private readonly IMainWindow _mainWindow;
        private readonly RenderingDevice _device;
        private readonly WidgetImage _image;
        private readonly SubtitleRenderer? _subtitleRenderer;
        private readonly string? _musicPath;

        public SlideRenderer(string slidePath, string? musicPath,
            MovieSubtitles? subtitles) : this(Tig.MainWindow, Tig.RenderingDevice, slidePath, musicPath, subtitles)
        {
        }

        public SlideRenderer(IMainWindow mainWindow, RenderingDevice device, string slidePath, string? musicPath,
            MovieSubtitles? subtitles)
        {
            _mainWindow = mainWindow;
            _device = device;
            _musicPath = musicPath;
            _image = new WidgetImage(slidePath);
            if (subtitles != null)
            {
                _subtitleRenderer = new SubtitleRenderer(_device, subtitles);
            }
        }

        public void Dispose()
        {
            _image?.Dispose();
        }

        public void Run()
        {
            _mainWindow.IsCursorVisible = false;

            int streamId = -1;
            if (_musicPath != null)
            {
                Tig.Sound.tig_sound_alloc_stream(out streamId, tig_sound_type.TIG_ST_VOICE);
                Tig.Sound.StreamPlayMusicOnce(streamId, _musicPath, 0, true, -1);
                if (!Tig.Sound.IsStreamActive(streamId))
                {
                    streamId = -1;
                }
            }

            var keyPressed = false;
            var stopwatch = Stopwatch.StartNew();
            while (!keyPressed)
            {
                if (streamId == -1)
                {
                    // Show slides for 3s without audio
                    if (stopwatch.ElapsedMilliseconds >= 3000)
                    {
                        break;
                    }
                }
                else
                {
                    // Otherwise until the stream is done
                    if (!Tig.Sound.IsStreamPlaying(streamId))
                    {
                        break;
                    }
                }

                _device.ClearCurrentColorTarget(LinearColorA.Black);

                var movieRect = MovieRenderer.GetMovieRect(
                    _device,
                    _image.GetPreferredSize().Width,
                    _image.GetPreferredSize().Height
                );
                _image.SetBounds(new Rectangle(
                    (int)movieRect.X,
                    (int)movieRect.Y,
                    (int)movieRect.Width,
                    (int)movieRect.Height
                ));
                _image.Render();

                _subtitleRenderer?.Render(stopwatch.Elapsed.TotalSeconds);

                _device.Present();

                Tig.SystemEventPump.PumpSystemEvents();

                MovieRenderer.ProcessMessages(ref keyPressed);
            }

            if (streamId != -1)
            {
                Tig.Sound.FreeStream(streamId);
            }

            _mainWindow.IsCursorVisible = true;
        }
    }
}