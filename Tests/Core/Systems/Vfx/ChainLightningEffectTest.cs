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

            ImageComparison.AssertImagesEqual(TakeScreenshot(), GetRefImage("VeryLong"));
        }

        [Test]
        public void TestRender()
        {
            var startTime = TimePoint.Now;
            var from = CameraCenter - new Vector3(300, 0, -250);
            var targets = new List<ChainLightningTarget>()
            {
                new (CameraCenter + new Vector3(300, 0, -250)),
                new (CameraCenter + new Vector3(300, 0, 250)),
                new (CameraCenter + new Vector3(500, 0, 0))
            };
            var locations = new[] { from }.Concat(targets.Select(t => t.Location)).ToArray();

            var effect = new ChainLightningEffect(
                _renderer,
                startTime,
                from,
                targets
            );

            var frames = new List<(TimePoint, Image<Bgra32>)>();
            for (var i = 0; i < 45; i++)
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
                // Points are chosen where arcs become visible/invisible.
                if (i == 0 || i == 6 || i == 11 || i == 36 || i == 44)
                {
                    var refImage = GetRefImage("Ref" + timeSinceStart);
                    ImageComparison.AssertImagesEqual(screenshot, refImage);
                }
            }

            ScreenshotCommandWrapper.AttachVideo("chain_lightning.mp4", frames);
        }

        private static string GetRefImage(string suffix)
        {
            return "Core/Systems/Vfx/ChainLightning" + suffix + ".png";
        }

        public override void Dispose()
        {
            TimePoint.ClearFakeTime();
            _renderer?.Dispose();
            base.Dispose();
        }
    }
}