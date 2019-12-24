using System;
using System.Data;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui
{
    // TODO: Migrate this to an actual widget
    public class GameView : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private float mSceneScale;
        private RectangleF mSceneRect;

        public Size RenderResolution { get; private set; }

        public event Action<Size> OnRenderResolutionChanged;

        public Size Size { get; private set; }

        private readonly IMainWindow _mainWindow;

        public GameView(IMainWindow mainWindow, Size renderResolution, Size size)
        {
            RenderResolution = renderResolution;
            Size = size;
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

        public void Dispose()
        {
            _mainWindow.SetMouseMoveHandler(null);
        }

        public Point MapToScene(int x, int y)
        {
            // Move it into the scene rectangle coordinate space
            var localX = x - mSceneRect.X;
            var localY = y - mSceneRect.Y;

            // Scale it to the coordinate system that was used to render the scene
            localX = MathF.Floor(localX / mSceneScale);
            localY = MathF.Floor(localY / mSceneScale);

            return new Point((int) localX, (int) localY);
        }

        public Point MapFromScene(int x, int y)
        {
            var localX = x * mSceneScale;
            var localY = y * mSceneScale;

            // move it into the scene rectangle coordinate space
            localX += mSceneRect.X + 1;
            localY += mSceneRect.Y + 1;

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
            Size = new Size(width, height);
            UpdateScale();
        }

        private void UpdateScale()
        {
            var widthFactor = Size.Width / (float) RenderResolution.Width;
            var heightFactor = Size.Height / (float) RenderResolution.Height;
            mSceneScale = MathF.Min(widthFactor, heightFactor);

            // Calculate the rectangle on the back buffer where the scene will
            // be stretched to
            var drawWidth = mSceneScale * RenderResolution.Width;
            var drawHeight = mSceneScale * RenderResolution.Height;
            var drawX = (Size.Width - drawWidth) / 2;
            var drawY = (Size.Height - drawHeight) / 2;
            mSceneRect = new RectangleF(drawX, drawY, drawWidth, drawHeight);
        }

    }
}