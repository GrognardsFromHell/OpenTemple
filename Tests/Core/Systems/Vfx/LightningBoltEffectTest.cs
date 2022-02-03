using System;
using System.Numerics;
using NUnit.Framework;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Vfx;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.Systems.Vfx;

public class LightningBoltEffectTest : RenderingTest
{
    private LightningBoltRenderer _renderer;

    [SetUp]
    public void SetUp()
    {
        // Fixed seed ensures this test is repeatable
        ThreadSafeRandom.Seed = 12345;
        _renderer = new LightningBoltRenderer(Device, MdfFactory, new PerlinNoise());
    }

    [Test]
    public void TestRender()
    {
        var startTime = TimePoint.Now;
        var from = CameraCenter - new Vector3(0, 0, 0);
        var to = CameraCenter - new Vector3(300, 0, 0);
        var locations = new[] { from, to };

        var effect = new LightningBoltEffect(
            _renderer,
            startTime,
            from,
            to
        );

        void Render(int timeSinceStart)
        {
            TimePoint.SetFakeTime(startTime + TimeSpan.FromMilliseconds(timeSinceStart));
            effect.Render(Viewport);
        }

        var frames = RenderFrameSequence(37, Render, locations);

        ScreenshotCommandWrapper.AttachVideo("lightning_bolt.mp4", frames);

        // Assert against reference images at certain intervals
        CompareReferenceFrames(frames, "Core/Systems/Vfx/LightningBolt", 0, 300, 550, 1800);
    }

    public override void Dispose()
    {
        TimePoint.ClearFakeTime();
        _renderer?.Dispose();
        base.Dispose();
    }
}