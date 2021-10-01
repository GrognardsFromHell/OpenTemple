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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTemple.Tests.Core.Systems.Vfx
{
    public class CallLightningEffectTest : RenderingTest
    {
        private CallLightningRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            // Fixed seed ensures this test is repeatable
            ThreadSafeRandom.Seed = 12345;
            _renderer = new CallLightningRenderer(Device, MdfFactory, new PerlinNoise());
        }

        [Test]
        public void TestRender()
        {
            var startTime = TimePoint.Now;
            var target = CameraCenter + new Vector3(300, 0, 300);

            var effect = new CallLightningEffect(
                _renderer,
                startTime,
                target
            );

            void Render(int timeSinceStart)
            {
                TimePoint.SetFakeTime(startTime + TimeSpan.FromMilliseconds(timeSinceStart));
                effect.Render(Viewport);
            }

            var frames = RenderFrameSequence(36, Render, new []{target}, interval: 25);

            AttachVideo("call_lightning.mp4", frames);

            // Assert against reference images at certain intervals
            // Points are chosen where arcs become visible/invisible.
            CompareReferenceFrames(frames, "Core/Systems/Vfx/CallLightning", 0, 150, 200, 500, 800);
        }

        public override void Dispose()
        {
            TimePoint.ClearFakeTime();
            _renderer?.Dispose();
            base.Dispose();
        }
    }
}