using System;
using System.Data;
using System.Drawing;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Ui
{
    public class GameView : DirectXView, IGameViewport
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private RectangleF _sceneRect;

        public Size RenderResolution { get; private set; }

        private float _sceneScale = float.NaN; // TODO

        private GameRenderer _gameRenderer;

        public LocAndOffsets CenteredOn { get; }

        public WorldCamera Camera { get; } = new ();

        private event Action _onResize;

        event Action IGameViewport.OnResize
        {
            add => _onResize += value;
            remove => _onResize -= value;
        }

        public GameView()
        {
            Globals.ConfigManager.OnConfigChanged += ReloadConfig;
        }

        private void ReloadConfig()
        {
            RenderScale = Globals.Config.Rendering.RenderScale;
            MultiSampling = new MultiSampleSettings(
                Globals.Config.Rendering.IsAntiAliasing,
                Globals.Config.Rendering.MSAASamples,
                Globals.Config.Rendering.MSAAQuality
            );
        }

        protected override void OnRender(RenderingDevice device, PixelSize pixelSize)
        {
            if (_gameRenderer == null || _gameRenderer.Device != device)
            {
                _gameRenderer?.Dispose();
                _gameRenderer = new GameRenderer(device, this);
            }

            device.BeginDraw();
            _gameRenderer.Render();
            device.EndDraw();
        }

        protected override void OnResize(PixelSize pixelSize)
        {
            base.OnResize(pixelSize);

            Camera.SetScreenWidth(pixelSize.Width, pixelSize.Height);

            _onResize?.Invoke();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            GameViews.Add(this);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            GameViews.Remove(this);
        }

        private void OnMouseMove(int x, int y, int wheelDelta)
        {
            // Map to game view
            var pos = MapToScene(x, y);
            x = pos.X;
            y = pos.Y;

            // Account for a resized screen
            if (x < 0 || y < 0 || x >= RenderResolution.Width || y >= RenderResolution.Height)
            {
                if (Globals.Config.Window.Windowed)
                {
                    if ((x > -7 && x < RenderResolution.Width + 7 && x > -7 && y < RenderResolution.Height + 7))
                    {
                        if (x < 0)
                            x = 0;
                        else if (x > RenderResolution.Width)
                            x = RenderResolution.Width;
                        if (y < 0)
                            y = 0;
                        else if (y > RenderResolution.Height)
                            y = RenderResolution.Height;
                        Tig.Mouse.MouseOutsideWndSet(false);
                        Tig.Mouse.SetPos(x, y, wheelDelta);
                        return;
                    }
                    else
                    {
                        Tig.Mouse.MouseOutsideWndSet(true);
                    }
                }
                else
                {
                    Logger.Info("Mouse outside resized window: {0},{1}, wheel: {2}", x, y, wheelDelta);
                }

                return;
            }

            Tig.Mouse.MouseOutsideWndSet(false);
            Tig.Mouse.SetPos(x, y, wheelDelta);
        }

        public Point MapToScene(int x, int y)
        {
            // Move it into the scene rectangle coordinate space
            var localX = x - _sceneRect.X;
            var localY = y - _sceneRect.Y;

            // Scale it to the coordinate system that was used to render the scene
            localX = MathF.Floor(localX / _sceneScale);
            localY = MathF.Floor(localY / _sceneScale);

            return new Point((int) localX, (int) localY);
        }

        public Point MapFromScene(int x, int y)
        {
            var localX = x * _sceneScale;
            var localY = y * _sceneScale;

            // move it into the scene rectangle coordinate space
            localX += _sceneRect.X + 1;
            localY += _sceneRect.Y + 1;

            return new Point((int) localX, (int) localY);
        }


        //        public void SetRenderResolution(int width, int height)
//        {
//            RenderResolution = new Size(width, height);
//            UpdateScale();
//            OnRenderResolutionChanged?.Invoke(RenderResolution);
//        }

//        public void SetSize(int width, int height)
//        {
//            SetSize(new Size(width, height));
//            UpdateScale();
//        }
//
//        private void UpdateScale()
//        {
//            var widthFactor = Width / (float) RenderResolution.Width;
//            var heightFactor = Height / (float) RenderResolution.Height;
//            _sceneScale = MathF.Min(widthFactor, heightFactor);
//
//            // Calculate the rectangle on the back buffer where the scene will
//            // be stretched to
//            var drawWidth = _sceneScale * RenderResolution.Width;
//            var drawHeight = _sceneScale * RenderResolution.Height;
//            var drawX = (Width - drawWidth) / 2;
//            var drawY = (Height - drawHeight) / 2;
//            _sceneRect = new RectangleF(drawX, drawY, drawWidth, drawHeight);
//        }

        public void TakeScreenshot(string path, Size size = default)
        {
            throw new NotImplementedException();
        }

    }
}