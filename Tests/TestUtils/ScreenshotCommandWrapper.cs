using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using OpenTemple.Core.Time;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = System.Drawing.Size;

namespace OpenTemple.Tests.TestUtils
{
    public class ScreenshotCommandWrapper : TestCommand
    {
        private readonly TestCommand _delegate;

        private readonly bool _recordVideo;

        private readonly List<(TimePoint, Image<Bgra32>)> _frames = new();

        public ScreenshotCommandWrapper(TestCommand @delegate, bool recordVideo = false) : base(@delegate.Test)
        {
            _delegate = @delegate;
            _recordVideo = recordVideo;
        }

        public override TestResult Execute(TestExecutionContext context)
        {
            if (!(context.TestObject is HeadlessGameTest headlessGameTest))
            {
                return _delegate.Execute(context);
            }

            // Record every frame, if requested
            var game = headlessGameTest.Game;
            void RecordFrame() => _frames.Add((TimePoint.Now, game.TakeScreenshot()));
            if (_recordVideo)
            {
                game.OnRenderFrame += RecordFrame;
            }

            try
            {
                return _delegate.Execute(context);
            }
            finally
            {
                game.OnRenderFrame -= RecordFrame;
                TakeScreenshot(game);
            }
        }

        private void TakeScreenshot(HeadlessGameHelper game)
        {
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Success)
            {
                if (_recordVideo && _frames.Count > 0)
                {
                    CreateVideo();
                }

                try
                {
                    game.RenderFrame();
                    var filename = Test.FullName + "_AfterFailure.png";
                    game.TakeScreenshot().SaveAsPng(filename);
                    TestContext.WriteLine("Took screenshot after failure: " + filename);
                    TestContext.AddTestAttachment(filename, "Post-Test Screenshot");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine("Failed to take screenshot after failure: " + e);
                }
            }
        }

        private void CreateVideo()
        {
            var filename = Path.GetFullPath(Test.FullName + "_Video.mp4");
            var (firstFrameTime, firstFrame) = _frames[0];

            using var encoder = new H264Encoder(filename, new Size(firstFrame.Width, firstFrame.Height));

            foreach (var (time, image) in _frames)
            {
                if (!image.TryGetSinglePixelSpan(out var rawPixelData))
                {
                    throw new InvalidOperationException("Image is not encoded as contiguous data!");
                }

                encoder.Encode(image, (long)(time - firstFrameTime).TotalMilliseconds);
            }

            encoder.Dispose();
            TestContext.AddTestAttachment(filename, "Video");
        }
    }
}