#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using FfmpegBink.Interop;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Interop;

namespace OpenTemple.Core.Systems.Movies
{
    public class MovieRenderer : IDisposable
    {
        private readonly IMainWindow _mainWindow;

        private readonly RenderingDevice _device;

        private readonly Material _material;

        private readonly VideoPlayer _player;

        private ResourceRef<DynamicTexture> _texture;

        private readonly SoLoudDynamicSource? _soundSource;

        private readonly SubtitleRenderer? _subtitleRenderer;

        // Current time in movie in seconds
        private double _currentTime;

        public MovieRenderer(VideoPlayer player, MovieSubtitles? subtitles) : this(Tig.MainWindow, Tig.RenderingDevice,
            player, subtitles)
        {
        }

        public MovieRenderer(IMainWindow mainWindow, RenderingDevice device, VideoPlayer player,
            MovieSubtitles? subtitles)
        {
            _mainWindow = mainWindow;
            _device = device;
            _material = CreateMaterial(device);
            _player = player;

            if (subtitles != null)
            {
                _subtitleRenderer = new SubtitleRenderer(_device, subtitles);
            }

            _texture = device.CreateDynamicTexture(BufferFormat.X8R8G8B8,
                player.VideoWidth, player.VideoHeight);

            if (_player.HasAudio)
            {
                _soundSource = new SoLoudDynamicSource(_player.AudioChannels, _player.AudioSampleRate);
                Tig.Sound.PlayDynamicSource(_soundSource);
            }

            player.OnVideoFrame += UpdateFrame;
            player.OnAudioSamples += PushAudioSamples;
        }

        private void UpdateFrame(in VideoFrame frame)
        {
            lock (this)
            {
                _texture.Resource.UpdateRaw(frame.PixelData, frame.Stride);
                _currentTime = frame.Time;
            }
        }

        private void PushAudioSamples(in AudioSamples samples)
        {
            if (_soundSource != null)
            {
                _soundSource.PushSamples(samples.Plane1, samples.Plane2);
            }
        }

        public void Run()
        {
            _mainWindow.IsCursorVisible = false;
            _player.Play();

            var keyPressed = false;
            while (!_player.AtEnd && !keyPressed)
            {
                RenderFrame();
                Thread.Sleep(3);

                // Resizing the window can cause device interaction which might conflict with the update frame thread
                lock (this)
                {
                    Tig.SystemEventPump.PumpSystemEvents();
                }

                ProcessMessages(ref keyPressed);
            }

            _player.Stop();
            _mainWindow.IsCursorVisible = true;
        }

        internal static void ProcessMessages(ref bool keyPressed)
        {
            while (Tig.MessageQueue.TryGetMessage(out var msg))
            {
                // Allow skipping the movie via key-press or mouse-press
                if (msg.type == MessageType.KEYSTATECHANGE && msg.KeyStateChangeArgs.down
                    || msg.type == MessageType.MOUSE && (msg.MouseArgs.flags & MouseEventFlag.LeftClick) != 0)
                {
                    // TODO Wait for the key to be unpressed again
                    keyPressed = true;
                    break;
                }
            }
        }

        private void RenderFrame()
        {
            lock (this)
            {
                var movieRect = GetMovieRect(_device, _player.VideoWidth, _player.VideoHeight);
                // TODO UV should be manipulated for certain vignettes since they have been letterboxed in the bink file!!!

                // Set vertex shader
                Span<MovieVertex> vertices = stackalloc MovieVertex[4];
                vertices[0] = new MovieVertex(movieRect.Left, movieRect.Top, 0, 0);
                vertices[1] = new MovieVertex(movieRect.Right, movieRect.Top, 1, 0);
                vertices[2] = new MovieVertex(movieRect.Right, movieRect.Bottom, 1, 1);
                vertices[3] = new MovieVertex(movieRect.Left, movieRect.Bottom, 0, 1);
                using var vertexBuffer = _device.CreateVertexBuffer<MovieVertex>(vertices);

                Span<ushort> indices = stackalloc ushort[6]
                {
                    0, 1, 2,
                    0, 2, 3
                };
                using var indexBuffer = _device.CreateIndexBuffer(indices);

                BufferBinding binding = new (_device, _material.VertexShader);
                binding.AddBuffer<MovieVertex>(vertexBuffer, 0)
                    .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                    .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                    .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

                _device.ClearCurrentColorTarget(LinearColorA.Black);

                _device.SetMaterial(_material);
                _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);
                binding.Bind();

                _device.SetIndexBuffer(indexBuffer);

                _device.SetTexture(0, _texture.Resource);
                _device.DrawIndexed(PrimitiveType.TriangleList, 4, 6);

                _subtitleRenderer?.Render(_currentTime);

                _device.Present();
            }
        }

        internal static RectangleF GetMovieRect(RenderingDevice device, int movieWidth, int movieHeight)
        {
            var screenWidth = device.UiCanvasSize.Width;
            var screenHeight = device.UiCanvasSize.Height;

            // Fit movie into rect
            var wFactor = screenWidth / movieWidth;
            var hFactor = screenHeight / movieHeight;
            var scale = MathF.Min(wFactor, hFactor);
            var movieW = scale * movieWidth;
            var movieH = scale * movieHeight;

            // Center on screen
            return new RectangleF(
                (screenWidth - movieW) / 2,
                (screenHeight - movieH) / 2,
                movieW,
                movieH
            );
        }

        private static Material CreateMaterial(RenderingDevice device)
        {
            var depthStencilState = new DepthStencilSpec
            {
                depthEnable = false
            };
            var samplerState = new SamplerSpec();
            samplerState.magFilter = TextureFilterType.Linear;
            samplerState.minFilter = TextureFilterType.Linear;
            samplerState.mipFilter = TextureFilterType.Linear;
            var samplers = new[]
            {
                new MaterialSamplerSpec(default, samplerState)
            };
            var vs = device.GetShaders().LoadVertexShader("gui_vs");
            var ps = device.GetShaders().LoadPixelShader("textured_simple_ps");

            return device.CreateMaterial(
                new BlendSpec(),
                depthStencilState,
                new RasterizerSpec(),
                samplers,
                vs,
                ps
            );
        }

        public void Dispose()
        {
            _soundSource?.Dispose();
            _material.Dereference();
            _texture.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly struct MovieVertex
        {
            private static readonly int Size = Marshal.SizeOf<MovieVertex>();

            private readonly Vector4 pos;
            private readonly uint color;
            private readonly Vector2 uv;

            public MovieVertex(float x, float y, float u, float v)
            {
                pos = new Vector4(x, y, 0, 1);
                color = 0xFFFFFFFF;
                uv = new Vector2(u, v);
            }
        }
    }
}