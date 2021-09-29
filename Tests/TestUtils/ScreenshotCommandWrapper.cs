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

        private readonly bool _always;

        private readonly List<(TimePoint, Image<Bgra32>)> _frames = new();

        public ScreenshotCommandWrapper(TestCommand @delegate, bool always, bool recordVideo = false) : base(
            @delegate.Test)
        {
            _delegate = @delegate;
            _recordVideo = recordVideo;
            _always = always;
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
            if (_always || TestContext.CurrentContext.Result.Outcome != ResultState.Success)
            {
                try
                {
                    TimePoint.SetFakeTime(TimePoint.Now + TimeSpan.FromMilliseconds(100));
                    game.RenderFrame();
                    // OnRenderFrame callback should not be registered anymore, hence we have to
                    // queue it manually as the last video frame
                    _frames.Add((TimePoint.Now, game.TakeScreenshot()));
                }
                catch (Exception e)
                {
                    TestContext.WriteLine("Failed to render screen for final screenshot: " + e);
                }

                if (_recordVideo && _frames.Count > 0)
                {
                    CreateVideo();
                }

                try
                {
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
            AttachVideo(Test.FullName + "_Video.mp4", _frames);
        }

        public static void AttachVideo(string path, IList<(TimePoint, Image<Bgra32>)> frames)
        {
            var filename = Path.GetFullPath(path);
            var (firstFrameTime, firstFrame) = frames[0];

            using var encoder = new H264Encoder(filename, new Size(firstFrame.Width, firstFrame.Height));

            foreach (var (time, image) in frames)
            {
                encoder.Encode(image, (long)(time - firstFrameTime).TotalMilliseconds);
            }

            // Repeat the last frame to get a hold-time of 1 second on the last frame
            var (lastFrameTime, lastFrame) = frames[^1];
            encoder.Encode(lastFrame, (long)(lastFrameTime - firstFrameTime).TotalMilliseconds + 1000);

            encoder.Dispose();
            TestContext.AddTestAttachment(filename, "Video");
        }
    }
}