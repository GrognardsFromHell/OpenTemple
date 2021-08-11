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
        }

        [TearDown]
        public void TakeScreenshotOfFailure()
        {
        }
    }
}