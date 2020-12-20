using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class GameView : WidgetContainer
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private float _sceneScale;
        private RectangleF _sceneRect;

        public Size RenderResolution { get; private set; }

        public event Action<Size> OnRenderResolutionChanged;

        private readonly IMainWindow _mainWindow;

        // It should get it's own camera at some point
        public WorldCamera Camera => Tig.RenderingDevice.GetCamera();

        public GameView(IMainWindow mainWindow, Size renderResolution, Size size) : base(size)
        {
            RenderResolution = renderResolution;
            UpdateScale();

            _mainWindow = mainWindow;
            _mainWindow.SetMouseMoveHandler(OnMouseMove);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _mainWindow.SetMouseMoveHandler(null);
            }
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

        public void SetRenderResolution(int width, int height)
        {
            RenderResolution = new Size(width, height);
            UpdateScale();
            OnRenderResolutionChanged?.Invoke(RenderResolution);
        }

        public void SetSize(int width, int height)
        {
            SetSize(new Size(width, height));
            UpdateScale();
        }

        private void UpdateScale()
        {
            var widthFactor = Width / (float) RenderResolution.Width;
            var heightFactor = Height / (float) RenderResolution.Height;
            _sceneScale = MathF.Min(widthFactor, heightFactor);

            // Calculate the rectangle on the back buffer where the scene will
            // be stretched to
            var drawWidth = _sceneScale * RenderResolution.Width;
            var drawHeight = _sceneScale * RenderResolution.Height;
            var drawX = (Width - drawWidth) / 2;
            var drawY = (Height - drawHeight) / 2;
            _sceneRect = new RectangleF(drawX, drawY, drawWidth, drawHeight);
        }
    }
}