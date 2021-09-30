using System;
using System.Collections.Generic;
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

            var frames = new List<(TimePoint, Image<Bgra32>)>();
            for (var i = 0; i < 37; i++)
            {
                Device.BeginDraw();
                var timeSinceStart = i * 50;
                TimePoint.SetFakeTime(startTime + TimeSpan.FromMilliseconds(timeSinceStart));
                effect.Render(Viewport);
                Device.EndDraw();

                var screenshot = TakeScreenshot();
                MarkWorldPositions(screenshot, locations);
                frames.Add((new TimePoint(TimePoint.TicksPerMillisecond * timeSinceStart), screenshot));

                // Assert against reference images at certain intervals
                if (i == 0 || i == 6 || i == 11 || i == 36)
                {
                    var refImage = GetRefImage("Ref" + timeSinceStart);
                    ImageComparison.AssertImagesEqual(screenshot, refImage);
                }
            }

            ScreenshotCommandWrapper.AttachVideo("lightning_bolt.mp4", frames);
        }

        private static string GetRefImage(string suffix)
        {
            return "Core/Systems/Vfx/LightningBolt" + suffix + ".png";
        }

        public override void Dispose()
        {
            TimePoint.ClearFakeTime();
            _renderer?.Dispose();
            base.Dispose();
        }
    }
}