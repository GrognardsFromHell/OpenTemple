using System.Diagnostics;
using System.Numerics;
using NUnit.Framework;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// More lightweight than the full game test. This initializes a headless renderer and the necessary
    /// subsystems for testing rendering in isolation from game systems.
    /// </summary>
    public abstract class RenderingTest : RealGameFiles
    {
        protected HeadlessMainWindow Window;
        protected RenderingDevice Device;
        protected MdfMaterialFactory MdfFactory;
        protected IGameViewport Viewport;

        // What the camera is centered on
        protected readonly Vector3 CameraCenter = new(1000, 0, 1000);

        [SetUp]
        public void SetUpRendering()
        {
            Window = new HeadlessMainWindow();
            // The D2D debug layer can trigger a debug-break when it's unloaded and it detects a leak,
            // which is quite bad for unit tests.
            Device = new RenderingDevice(FS, Window, debugDevice: Debugger.IsAttached);
            MdfFactory = new MdfMaterialFactory(FS, Device);

            var camera = new WorldCamera();
            camera.ViewportSize = Device.UiCanvasSize.ToSize();
            camera.CenterOn(CameraCenter.X, CameraCenter.Y, CameraCenter.Z);
            Viewport = new MockViewport(camera, Window);
        }

        public override void Dispose()
        {
            MdfFactory?.Dispose();
            Device?.Dispose();
            Window?.Dispose();

            base.Dispose();
        }

        protected Image<Bgra32> TakeScreenshot()
        {
            return HeadlessGameHelper.TakeScreenshot(Device);
        }

        /// <summary>
        /// Paint crosshairs at the given in-world locations on the given screenshots. Assumes the current Camera
        /// is still applicable to the given screenshot.
        /// </summary>
        protected void MarkWorldPositions(Image<Bgra32> screenshot, params Vector3[] points)
        {
            void FlipPixel(int x, int y)
            {
                if (x >= 0 && x < screenshot.Width && y >= 0 && y < screenshot.Height)
                {
                    var pixel = screenshot[x, y];
                    pixel.R ^= 0xFF;
                    pixel.G ^= 0xFF;
                    pixel.B ^= 0xFF;
                    screenshot[x, y] = pixel;
                }
            }

            foreach (var point in points)
            {
                var screenPos = Viewport.WorldToScreen(point);
                var sx = (int)screenPos.X;
                var sy = (int)screenPos.Y;

                for (var x = -5; x <= 5; x++)
                {
                    FlipPixel(sx + x, sy);
                }

                for (var y = -5; y <= 5; y++)
                {
                    if (y != 0)
                    {
                        FlipPixel(sx, sy + y);
                    }
                }
            }
        }
    }
}