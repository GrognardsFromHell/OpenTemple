using System;
using System.Diagnostics;
using NUnit.Framework;

namespace OpenTemple.Tests.TestUtils
{
    [Category("NeedsRealFiles")]
    [NonParallelizable]
    public abstract class HeadlessGameTest
    {
        private static HeadlessGameHelper _game;

        public HeadlessGameHelper Game => _game;

        [OneTimeSetUp]
        public static void StartGame()
        {
            Trace.Assert(_game == null);
            _game = new HeadlessGameHelper();
        }

        [OneTimeTearDown]
        public static void StopGame()
        {
            try
            {
                _game?.Dispose();
            }
            finally
            {
                _game = null;
            }

            // Run all pending finalizers, because sometimes these will crash and
            // if it happens later, it will be harder to associate them with the test
            // that actually caused the problem
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TearDown]
        public void TakeScreenshotOfFailure()
        {
        }
    }
}