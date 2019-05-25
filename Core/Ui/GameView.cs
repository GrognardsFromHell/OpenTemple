using System;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui
{
    // TODO: Migrate this to an actual widget
    public class GameView : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private int mResizeListener;
        private RenderingDevice mDevice;
        private int mWidth;
        private int mHeight;
        private float mSceneScale;
        private RectangleF mSceneRect;

        public int Width => mWidth;
        public int Height => mHeight;

        private readonly IMainWindow _mainWindow;

        public GameView(RenderingDevice device, IMainWindow mainWindow, int width, int height)
        {
            mWidth = width;
            mHeight = height;

            mDevice = device;
            _mainWindow = mainWindow;
            _mainWindow.SetMouseMoveHandler(OnMouseMove);
            mResizeListener = mDevice.AddResizeListener(Resize);

            var camera = device.GetCamera();
            Resize((int) camera.GetScreenWidth(), (int) camera.GetScreenHeight());
        }

        private void OnMouseMove(int x, int y, int wheelDelta)
        {
            // Map to game view
            var pos = MapToScene(x, y);
            x = pos.X;
            y = pos.Y;

            // Account for a resized screen
            if (x < 0 || y < 0 || x >= mWidth || y >= mHeight)
            {
                if (Globals.Config.Window.Windowed)
                {
                    if ((x > -7 && x < mWidth + 7 && x > -7 && y < mHeight + 7))
                    {
                        if (x < 0)
                            x = 0;
                        else if (x > mWidth)
                            x = mWidth;
                        if (y < 0)
                            y = 0;
                        else if (y > mHeight)
                            y = mHeight;
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
            mDevice.RemoveResizeListener(mResizeListener);
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

        public void Resize(int width, int height)
        {
            var widthFactor = width / (float) mWidth;
            var heightFactor = height / (float) mHeight;
            mSceneScale = MathF.Min(widthFactor, heightFactor);

            // Calculate the rectangle on the back buffer where the scene will
            // be stretched to
            var drawWidth = mSceneScale * mWidth;
            var drawHeight = mSceneScale * mHeight;
            var drawX = (width - drawWidth) / 2;
            var drawY = (height - drawHeight) / 2;
            mSceneRect = new RectangleF(drawX, drawY, drawWidth, drawHeight);
        }
    }
}