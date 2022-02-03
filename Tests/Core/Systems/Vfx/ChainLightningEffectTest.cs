using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Vfx;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.Systems.Vfx;

public class ChainLightningEffectTest : RenderingTest
{
    private ChainLightningRenderer _renderer;

    [SetUp]
    public void SetUp()
    {
        // Fixed seed ensures this test is repeatable
        ThreadSafeRandom.Seed = 12345;
        _renderer = new ChainLightningRenderer(Device, MdfFactory, new PerlinNoise());
    }

    // Vanilla would crash/corrupt memory if more than 600 line segments were being generated
    [Test]
    public void TestVeryLongChainLightning()
    {
        Device.BeginDraw();
        _renderer.Render(Viewport.Camera, 0, 0, CameraCenter, CameraCenter + new Vector3(0, 0, 10000));
        Device.EndDraw();

        ImageComparison.AssertImagesEqual(TakeScreenshot(), "Core/Systems/Vfx/ChainLightningVeryLong.png");
    }

    [Test]
    public void TestHeightDifferenceChainLightning()
    {
        var a = CameraCenter - new Vector3(200, -25, 0);
        var b = CameraCenter;
        var c = CameraCenter + new Vector3(200, 25, 0);

        Device.BeginDraw();
        _renderer.Render(Viewport.Camera, 0, 0, a, b);
        _renderer.Render(Viewport.Camera, 0, 250, b, c);
        Device.EndDraw();

        var screenshot = TakeScreenshot();
        MarkWorldPositions(screenshot, a, b, c);
        ImageComparison.AssertImagesEqual(screenshot, "Core/Systems/Vfx/ChainLightningHeightDifference.png");
    }

    [Test]
    public void TestRender()
    {
        var startTime = TimePoint.Now;
        var from = CameraCenter - new Vector3(300, 0, -250);
        var targets = new List<ChainLightningTarget>()
        {
            new(CameraCenter + new Vector3(300, 0, -250)),
            new(CameraCenter + new Vector3(300, 0, 250)),
            new(CameraCenter + new Vector3(500, 0, 0))
        };
        var locations = new[] { from }.Concat(targets.Select(t => t.Location)).ToArray();

        var effect = new ChainLightningEffect(
            _renderer,
            startTime,
            from,
            targets
        );

        void Render(int timeSinceStart)
        {
            TimePoint.SetFakeTime(startTime + TimeSpan.FromMilliseconds(timeSinceStart));
            effect.Render(Viewport);
        }

        var frames = RenderFrameSequence(45, Render, locations);

        ScreenshotCommandWrapper.AttachVideo("chain_lightning.mp4", frames);

        // Assert against reference images at certain intervals
        // Points are chosen where arcs become visible/invisible.
        CompareReferenceFrames(frames, "Core/Systems/Vfx/ChainLightning", 0, 300, 550, 1800, 2200);
    }

    public override void Dispose()
    {
        TimePoint.ClearFakeTime();
        _renderer?.Dispose();
        base.Dispose();
    }
}