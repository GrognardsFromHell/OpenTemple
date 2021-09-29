using System.Numerics;
using NUnit.Framework;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;
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
        protected WorldCamera Camera;

        // What the camera is centered on
        protected readonly Vector3 CameraCenter = new(1000, 0, 1000);

        [SetUp]
        public void SetUpRendering()
        {
            Window = new HeadlessMainWindow();
            Device = new RenderingDevice(FS, Window, debugDevice: true);
            MdfFactory = new MdfMaterialFactory(FS, Device);

            Camera = new WorldCamera();
            Camera.ViewportSize = Device.UiCanvasSize.ToSize();
            Camera.CenterOn(CameraCenter.X, CameraCenter.Y, CameraCenter.Z);
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
    }
}