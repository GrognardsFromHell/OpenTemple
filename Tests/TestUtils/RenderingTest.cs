using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Styles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTemple.Tests.TestUtils;

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
        camera.CenterOn(CameraCenter);
        Viewport = new MockViewport(camera, Window);

        Globals.UiStyles = new UiStyles(FS);
    }

    public override void Dispose()
    {
        MdfFactory?.Dispose();
        Device?.Dispose();
        Window?.Dispose();

        base.Dispose();
    }

    /// <summary>
    /// Renders a number of frames in 50ms intervals and returns them.
    /// Additionally, certain in-world positions are marked in the images.
    /// </summary>
    protected List<(TimePoint, Image<Bgra32>)> RenderFrameSequence(
        int frameCount,
        Action<int> render,
        Vector3[] markPositions = null,
        int interval = 50)
    {
        var frames = new List<(TimePoint, Image<Bgra32>)>();
        for (var i = 0; i < frameCount; i++)
        {
            var timeSinceStart = i * interval;

            Device.BeginDraw();
            render(timeSinceStart);
            Device.EndDraw();

            var screenshot = TakeScreenshot();
            if (markPositions != null)
            {
                MarkWorldPositions(screenshot, markPositions);
            }

            frames.Add((new TimePoint(TimePoint.TicksPerMillisecond * timeSinceStart), screenshot));
        }

        return frames;
    }

    /// <summary>
    /// Compares certain time points in a list of rendered frames against reference images.
    /// </summary>
    protected void CompareReferenceFrames(IList<(TimePoint, Image<Bgra32>)> frames,
        string refImagesPrefix,
        params int[] timePointsInMs)
    {
        foreach (var timePointInMs in timePointsInMs)
        {
            var timePoint = new TimePoint(TimePoint.TicksPerMillisecond * timePointInMs);
            var frame = frames.FirstOrDefault(f => f.Item1 == timePoint);
            if (frame.Item2 == null)
            {
                throw new ArgumentException("No frame was rendered @ "
                                            + timePointInMs
                                            + ". Available: "
                                            + string.Join(", ", frames.Select(f => f.Item1.Milliseconds)));
            }

            var refImage = refImagesPrefix + "Ref" + timePointInMs + ".png";
            ImageComparison.AssertImagesEqual(frame.Item2, refImage);
        }
    }

    protected void AttachVideo(string path, IList<(TimePoint, Image<Bgra32>)> frames)
    {
        ScreenshotCommandWrapper.AttachVideo(path, frames);
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